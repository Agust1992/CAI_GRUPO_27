. (Join-Path $PSScriptRoot 'ApiPruebas.ps1')
Compilar 'ECommerce.sln'
Compilar 'tests/Dependencias/Dependencias.csproj'
$entorno = NuevoEntorno
try {
    $dependencias = IniciarApi $entorno 'dependencias' 'tests/Dependencias/Dependencias.csproj' @{} '/health'
    $productos = IniciarApi $entorno 'productos' 'src/Products.API/Products.API.csproj' @{'Services__Orders'=($dependencias.Url + '/')}
    $carrito = IniciarApi $entorno 'carrito' 'src/Cart.API/Cart.API.csproj' @{'Services__Products'=($productos.Url + '/')}
    $usuario = [guid]::NewGuid().ToString()
    $alta = Solicitar POST ($productos.Url + '/api/products') @{nombre='Producto para Cart';precio=100;stock=5;categoria='Otros'}
    Comprobar ($alta.Estado -eq 201) 'No se pudo crear el producto real.'
    $id = $alta.Json.id
    $ruta = "/api/cart/$usuario"
    $base = $carrito.Url
    ComprobarError (Solicitar GET ($base+$ruta)) 404 'CRT-001'
    $agregado = Solicitar POST ($base+$ruta+'/items') @{productoId=$id;cantidad=2}
    Comprobar ($agregado.Estado -eq 200 -and $agregado.Json.items[0].cantidad -eq 2) 'Fallo el agregado real.'
    $agregado = Solicitar POST ($base+$ruta+'/items') @{productoId=$id;cantidad=1}
    Comprobar ($agregado.Json.items.Count -eq 1 -and $agregado.Json.items[0].cantidad -eq 3) 'POST no sumo cantidades.'
    $actualizado = Solicitar PUT ($base+$ruta+'/items/'+$id) @{cantidad=4}
    Comprobar ($actualizado.Estado -eq 200 -and $actualizado.Json.items[0].cantidad -eq 4) 'PUT no reemplazo la cantidad.'
    ComprobarError (Solicitar POST ($base+$ruta+'/items') @{productoId=$id;cantidad=2}) 422 'CRT-003'
    ComprobarError (Solicitar PUT ($base+$ruta+'/items/'+$id) @{cantidad=0}) 400 'CRT-004'
    ComprobarError (Solicitar POST ($base+$ruta+'/items') '{}' 'text/plain') 415 'CRT-004'
    ComprobarError (Solicitar PATCH ($base+$ruta)) 405 'CRT-004'
    ComprobarError (Solicitar GET ($base+'/api/cart/no-es-guid')) 400 'CRT-004'
    $conservado = Solicitar GET ($base+$ruta)
    Comprobar ($conservado.Json.items[0].cantidad -eq 4) 'Una solicitud invalida cambio el carrito.'
    Comprobar ((Solicitar GET ($productos.Url+'/api/products/'+$id)).Json.stock -eq 5) 'Cart desconto stock.'
    $segundoUsuario = [guid]::NewGuid().ToString()
    Comprobar ((Solicitar POST ($base+'/api/cart/'+$segundoUsuario+'/items') @{productoId=$id;cantidad=5}).Estado -eq 200) 'Los usuarios no tienen carritos independientes.'
    $swagger = (Solicitar GET ($base+'/swagger/v1/swagger.json')).Json
    $ejemplo = $swagger.paths.'/api/cart/{userId}/items'.post.responses.'422'.content.'application/problem+json'.example
    Comprobar ($ejemplo.errorCode -eq 'CRT-003') 'Falta el ejemplo de stock en Swagger.'
    $ejemplos404 = $swagger.paths.'/api/cart/{userId}/items/{productId}'.put.responses.'404'.content.'application/problem+json'.examples
    Comprobar ($ejemplos404.carritoNoEncontrado.value.errorCode -eq 'CRT-001' -and $ejemplos404.productoNoEncontrado.value.errorCode -eq 'CRT-002') 'Faltan ejemplos de 404.'
    foreach ($salud in @('/health','/health/ready','/health/live')) { Comprobar ((Solicitar GET ($base+$salud)).Estado -eq 200) 'Fallo un control de salud de Cart.' }

    $concurrente = [guid]::NewGuid().ToString()
    $cliente = [Net.Http.HttpClient]::new()
    $tareas = [Collections.Generic.List[Threading.Tasks.Task[Net.Http.HttpResponseMessage]]]::new()
    try {
        $json = @{productoId=$id;cantidad=1} | ConvertTo-Json -Compress
        for ($i=0; $i -lt 6; $i++) { $tareas.Add($cliente.PostAsync(($base+'/api/cart/'+$concurrente+'/items'), [Net.Http.StringContent]::new($json,[Text.Encoding]::UTF8,'application/json'))) }
        $correctos=0; $sinStock=0
        foreach ($tarea in $tareas) {
            $r=$tarea.GetAwaiter().GetResult()
            if ([int]$r.StatusCode -eq 200) { $correctos++ }
            elseif ([int]$r.StatusCode -eq 422) { $sinStock++ }
            else { throw "Concurrencia devolvio $($r.StatusCode)" }
            $r.Dispose()
        }
        Comprobar ($correctos -eq 5 -and $sinStock -eq 1) 'No se controlo el stock en solicitudes simultaneas.'
    } finally { $cliente.Dispose() }
    Comprobar ((Solicitar GET ($base+'/api/cart/'+$concurrente)).Json.items[0].cantidad -eq 5) 'Se perdieron sumas simultaneas.'

    Stop-Process -Id $carrito.Proceso.Id
    $carrito.Proceso.WaitForExit()
    $carrito = IniciarApi $entorno 'carrito' 'src/Cart.API/Cart.API.csproj' @{'Services__Products'=($productos.Url + '/')}
    $base=$carrito.Url
    Comprobar ((Solicitar GET ($base+$ruta)).Json.items[0].cantidad -eq 4) 'Se perdieron datos al reiniciar.'
    Comprobar ((Solicitar DELETE ($productos.Url+'/api/products/'+$id)).Estado -eq 204) 'No se pudo eliminar el producto de prueba sin ordenes.'
    ComprobarError (Solicitar PUT ($base+$ruta+'/items/'+$id) @{cantidad=1}) 404 'CRT-002'
    Comprobar ((Solicitar DELETE ($base+$ruta+'/items/'+$id)).Estado -eq 204) 'No se pudo quitar el item.'
    Comprobar ((Solicitar DELETE ($base+$ruta)).Estado -eq 204) 'No se pudo vaciar.'
    Comprobar ((Solicitar GET ($base+$ruta)).Json.items.Count -eq 0) 'El carrito no quedo vacio.'

    $simulado = IniciarApi $entorno 'carrito-simulado' 'src/Cart.API/Cart.API.csproj' @{'Services__Products'=($dependencias.Url + '/')}
    $base=$simulado.Url
    $productoSimulado=[guid]::NewGuid().ToString()
    foreach ($modo in @('NoExiste','Error','Invalido','Incompleto','OtroId','StockNegativo','SinStock','Timeout')) {
        $null=Solicitar POST ($dependencias.Url+'/control/productos') @{modo=$modo}
        $r=Solicitar POST ($base+$ruta+'/items') @{productoId=$productoSimulado;cantidad=1}
        if ($modo -eq 'NoExiste') { ComprobarError $r 404 'CRT-002' }
        elseif ($modo -eq 'SinStock') { ComprobarError $r 422 'CRT-003' }
        else { ComprobarError $r 500 'CRT-005' }
    }
    Comprobar ((Solicitar GET ($dependencias.Url+'/control/correlacion')).Json.correlationId -eq 'prueba-cai') 'Cart no propago la correlacion.'
    Stop-Process -Id $dependencias.Proceso.Id
    $dependencias.Proceso.WaitForExit()
    ComprobarError (Solicitar POST ($base+$ruta+'/items') @{productoId=$productoSimulado;cantidad=1}) 500 'CRT-005'
    ComprobarError (Solicitar GET ($base+$ruta)) 404 'CRT-001'
    CerrarEntorno $entorno
    foreach ($nombre in @('productos','carrito','carrito-simulado')) {
        $logs = @(Get-ChildItem (Join-Path $entorno.Carpeta ($nombre+'/logs')) -Filter '*.json' | ForEach-Object { Get-Content $_.FullName } | ForEach-Object { $_ | ConvertFrom-Json })
        $finales = @($logs | Where-Object {$null -ne $_.Properties.Elapsed -and $_.Properties.CorrelationId -and $_.Properties.Service -and $_.Properties.Endpoint})
        Comprobar ($finales.Count -gt 0) "Faltan logs estructurados en $nombre."
    }
    Write-Output "Carrito: $script:Comprobaciones comprobaciones correctas. Evidencia: $($entorno.Carpeta)"
} finally { CerrarEntorno $entorno }
