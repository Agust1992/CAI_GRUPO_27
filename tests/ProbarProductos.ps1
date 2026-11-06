. (Join-Path $PSScriptRoot 'ApiPruebas.ps1')
Compilar 'src/Products.API/Products.API.csproj'
Compilar 'tests/Dependencias/Dependencias.csproj'
$entorno = NuevoEntorno
try {
    $ordenes = IniciarApi $entorno 'ordenes-simuladas' 'tests/Dependencias/Dependencias.csproj' @{} '/health'
    $productos = IniciarApi $entorno 'productos' 'src/Products.API/Products.API.csproj' @{'Services__Orders'=($ordenes.Url + '/')}
    $base = $productos.Url
    $numero = 0
    foreach ($modo in @('Pendiente','Confirmada','Enviada','Entregada','Cancelada','SinOrdenes','Error','Invalido','Incompleto','Desconocido','Timeout')) {
        $numero++
        $alta = Solicitar POST "$base/api/products" @{nombre="Prueba $numero";precio=100.50;stock=5;categoria='Otros'}
        Comprobar ($alta.Estado -eq 201) 'Fallo el alta de producto.'
        $id = $alta.Json.id
        $null = Solicitar POST ($ordenes.Url + '/control/ordenes') @{modo=$modo;productoId=$id}
        $borrado = Solicitar DELETE "$base/api/products/$id"
        if ($modo -eq 'Pendiente' -or $modo -eq 'Confirmada') {
            ComprobarError $borrado 409 'PRD-004'
            Comprobar ((Solicitar GET "$base/api/products/$id").Estado -eq 200) 'Se borro un producto con orden activa.'
        } elseif ($modo -in @('Error','Invalido','Incompleto','Desconocido','Timeout')) {
            ComprobarError $borrado 500 'PRD-005'
            Comprobar ((Solicitar GET "$base/api/products/$id").Estado -eq 200) 'Se borro sin comprobar Orders.'
        } else {
            Comprobar ($borrado.Estado -eq 204) 'No se pudo borrar sin orden activa.'
        }
        Comprobar ((Solicitar GET ($ordenes.Url + '/control/correlacion')).Json.correlationId -eq 'prueba-cai') 'No se propago la correlacion a Orders.'
    }
    ComprobarError (Solicitar GET "$base/api/products/00000000-0000-0000-0000-000000000001") 404 'PRD-001'
    ComprobarError (Solicitar GET "$base/api/products/no-es-guid") 400 'PRD-002'
    ComprobarError (Solicitar POST "$base/api/products" '{}' 'text/plain') 415 'PRD-002'
    ComprobarError (Solicitar PATCH "$base/api/products") 405 'PRD-002'
    $swagger = (Solicitar GET "$base/swagger/v1/swagger.json").Json
    $error409 = $swagger.paths.'/api/products/{id}'.delete.responses.'409'.content.'application/problem+json'
    Comprobar ($error409.example.errorCode -eq 'PRD-004') 'Falta el ejemplo de orden activa en Swagger.'
    Comprobar ($swagger.paths.'/api/products'.post.requestBody.content.'application/json'.example.precio -gt 0) 'Falta un ejemplo de alta valido.'
    Comprobar ($swagger.components.schemas.ErrorResponse.properties.errorCode -and $swagger.components.schemas.ErrorResponse.properties.correlationId) 'El esquema de error no muestra sus campos propios.'
    foreach ($ruta in @('/health','/health/ready','/health/live','/health-ui','/healthchecks-api')) {
        Comprobar ((Solicitar GET ($base + $ruta)).Estado -eq 200) "Fallo $ruta."
    }
    CerrarEntorno $entorno
    $logs = @(Get-ChildItem (Join-Path $productos.Carpeta 'logs') -Filter '*.json' | ForEach-Object { Get-Content $_.FullName } | ForEach-Object { $_ | ConvertFrom-Json })
    $correcto404 = @($logs | Where-Object {$_.Properties.RequestPath -like '*000000000001' -and $_.Properties.StatusCode -eq 404 -and $_.Level -eq 'Warning'})
    Comprobar ($correcto404.Count -gt 0) 'El log de 404 no tiene estado o nivel correctos.'
    foreach ($log in $logs) {
        if ($log.Properties.Endpoint) { Comprobar ($log.Properties.Service -eq 'Products.API' -and $log.Properties.CorrelationId) 'Log sin servicio o correlacion.' }
    }
    Write-Output "Productos: $script:Comprobaciones comprobaciones correctas. Evidencia: $($entorno.Carpeta)"
} finally { CerrarEntorno $entorno }
