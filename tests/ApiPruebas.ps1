$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http
$script:RaizRepo = Split-Path $PSScriptRoot -Parent
$script:Comprobaciones = 0
$env:DOTNET_PROCESSOR_COUNT = '2'

function Comprobar($condicion, $mensaje) {
    if (!$condicion) { throw $mensaje }
    $script:Comprobaciones++
}

function Compilar($proyecto) {
    $version = & dotnet --version
    if ([int]$version.Split('.')[0] -lt 10) { throw 'Se necesita instalar o activar el SDK .NET 10 o superior.' }
    & dotnet build (Join-Path $script:RaizRepo $proyecto) --disable-build-servers -m:1 --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw 'Fallo la compilacion de pruebas.' }
}

function NuevoEntorno {
    $carpeta = Join-Path ([IO.Path]::GetTempPath()) ('cai-pruebas-' + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $carpeta | Out-Null
    return @{ Carpeta=$carpeta; Procesos=[Collections.Generic.List[Diagnostics.Process]]::new() }
}

function IniciarApi($entorno, $nombre, $proyecto, $variables=@{}, $salud='/health/live') {
    $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
    $listener.Start()
    $puerto = $listener.LocalEndpoint.Port
    $listener.Stop()
    $url = "http://127.0.0.1:$puerto"
    $directorio = Join-Path $entorno.Carpeta $nombre
    New-Item -ItemType Directory -Path $directorio -Force | Out-Null
    $rutaProyecto = Join-Path $script:RaizRepo $proyecto
    $nombreDll = [IO.Path]::GetFileNameWithoutExtension($rutaProyecto) + '.dll'
    $dll = Join-Path (Split-Path $rutaProyecto -Parent) ('bin\Debug\net10.0\' + $nombreDll)
    $env:ASPNETCORE_URLS = $url
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:ASPNETCORE_CONTENTROOT = Split-Path $rutaProyecto -Parent
    $env:ConnectionStrings__DefaultConnection = 'Data Source=' + (Join-Path $directorio 'pruebas.db') + ';Foreign Keys=True'
    foreach ($variable in $variables.GetEnumerator()) { Set-Item -Path ('Env:' + $variable.Key) -Value $variable.Value }
    $dotnet = (Get-Command dotnet).Source
    $proceso = Start-Process -FilePath $dotnet -ArgumentList ('"' + $dll + '"') -WorkingDirectory $directorio -WindowStyle Hidden -RedirectStandardOutput (Join-Path $directorio ('salida-' + $entorno.Procesos.Count + '.log')) -RedirectStandardError (Join-Path $directorio ('errores-' + $entorno.Procesos.Count + '.log')) -PassThru
    $entorno.Procesos.Add($proceso)
    for ($i=0; $i -lt 60; $i++) {
        if ($proceso.HasExited) { throw "$nombre se detuvo al iniciar. Revisar $directorio" }
        try {
            $respuesta = Solicitar GET ($url + $salud)
            if ($respuesta.Estado -eq 200) { return @{Url=$url;Proceso=$proceso;Carpeta=$directorio} }
        } catch {}
        Start-Sleep -Milliseconds 500
    }
    throw "$nombre no inicio a tiempo."
}

function Solicitar($metodo, $url, $datos=$null, $tipo='application/json') {
    $cliente = [Net.Http.HttpClient]::new()
    $cliente.Timeout = [TimeSpan]::FromSeconds(15)
    $request = [Net.Http.HttpRequestMessage]::new([Net.Http.HttpMethod]::new($metodo), $url)
    $request.Headers.Add('X-Correlation-Id', 'prueba-cai')
    if ($null -ne $datos) {
        if ($datos -is [string]) { $texto=$datos } else { $texto=$datos | ConvertTo-Json -Depth 12 -Compress }
        $request.Content = [Net.Http.StringContent]::new($texto, [Text.Encoding]::UTF8, $tipo)
    }
    try {
        $response = $cliente.SendAsync($request).GetAwaiter().GetResult()
        try {
            $texto = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            $json = $null
            if ($texto) { try { $json = $texto | ConvertFrom-Json } catch {} }
            $correlacion = ''
            if ($response.Headers.Contains('X-Correlation-Id')) { $correlacion = [string]::Join(',', $response.Headers.GetValues('X-Correlation-Id')) }
            return @{Estado=[int]$response.StatusCode;Json=$json;Texto=$texto;Correlacion=$correlacion;Tipo=$response.Content.Headers.ContentType.MediaType}
        } finally { $response.Dispose() }
    } finally { $request.Dispose(); $cliente.Dispose() }
}

function ComprobarError($respuesta, $estado, $codigo) {
    Comprobar ($respuesta.Estado -eq $estado) "Se esperaba HTTP $estado y llego $($respuesta.Estado): $($respuesta.Texto)"
    Comprobar ($respuesta.Json.errorCode -eq $codigo) "Falta el codigo $codigo."
    Comprobar ($respuesta.Json.status -eq $estado -and $respuesta.Json.title) 'Estado o titulo del error incorrectos.'
    Comprobar ($respuesta.Json.errorMessage -and $respuesta.Json.detail -and $respuesta.Json.type -and $respuesta.Json.instance) 'Error incompleto.'
    Comprobar ($respuesta.Tipo -eq 'application/problem+json') 'Tipo de contenido de error incorrecto.'
    Comprobar ($respuesta.Correlacion -eq 'prueba-cai' -and $respuesta.Json.correlationId -eq 'prueba-cai') 'Se perdio la correlacion.'
    Comprobar ($respuesta.Texto -notmatch 'stackTrace|SqliteException|no such table|SELECT ') 'El error expuso informacion interna.'
}

function CerrarEntorno($entorno) {
    foreach ($proceso in $entorno.Procesos) {
        if (!$proceso.HasExited) { Stop-Process -Id $proceso.Id; $proceso.WaitForExit() }
    }
}
