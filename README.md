# CAI Grupo 27

Trabajo practico de Construccion de Aplicaciones Informaticas.
Integrantes: Luciano Armas, Agustin Acosta y Christian Schuon.

## Estado del trabajo

El sistema es un e-commerce dividido en cinco APIs. En esta rama estan
implementadas Productos y Carrito. Users, Orders y Notifications todavia no
estan incorporados; la integracion con esos servicios queda pendiente.

Todos los servicios del TP se agregan a `ECommerce.sln`. La carpeta `MiniApi`
y `MiniApi.slnx` conservan la referencia original de la catedra y no son un
sexto microservicio. Seguimos su base de persistencia con SQLite y Dapper.
Productos y Carrito tienen bases propias, no una base compartida.

## Puertos

| Servicio | Puerto HTTP | Estado |
| --- | --- | --- |
| Products.API | 5208 | Implementado; Swagger en http://localhost:5208/swagger |
| Cart.API | 5104 | Implementado; Swagger en http://localhost:5104/swagger |
| Orders.API | 5103 | Direccion configurada en Productos; confirmar al integrar |
| Users.API | Por acordar | Pendiente de incorporar |
| Notifications.API | Por acordar | Pendiente de incorporar |

Las direcciones entre servicios estan en `appsettings.json`: `Services:Products`
en Carrito y `Services:Orders` en Productos. Tambien se pueden configurar con
`Services__Products` y `Services__Orders`, sin cambiar el codigo.

## Arquitectura actual

```text
Swagger o cliente -- HTTP --> Cart.API -- HTTP --> Products.API
Swagger o cliente ------------- HTTP -----------> Products.API

Cart.API ----- Dapper/SQLite --> cart.db
Products.API - Dapper/SQLite --> products.db

Products.API -- HTTP, solo antes de borrar --> Orders.API [pendiente]

Users.API y Notifications.API: pendientes de incorporar a la solucion.
```

La llamada directa del cliente a Productos representa sus operaciones de
catalogo. Carrito consulta Productos para validar existencia y stock; no lee
su base de datos. Dentro de cada API el recorrido es Controller -> Service ->
Repository. Los errores se convierten en respuestas HTTP con IExceptionHandler.

## Como ejecutar

Se necesita el SDK de .NET 10. Desde la raiz del repositorio, comprobarlo y
compilar una vez:

```powershell
dotnet --version
$env:DOTNET_PROCESSOR_COUNT = "2"
dotnet restore ECommerce.sln --disable-parallel
dotnet build ECommerce.sln --no-restore --disable-build-servers -m:1
```

La version que muestra el primer comando debe ser 10.x. El limite de procesos
ayuda a evitar demoras en la compilacion. No cambia las reglas de la aplicacion.

En una terminal, iniciar Productos:

```powershell
dotnet run --no-build --project src/Products.API --launch-profile http
```

En otra terminal, iniciar Carrito:

```powershell
dotnet run --no-build --project src/Cart.API --launch-profile http
```

Mantener ambas terminales abiertas. Ctrl+C detiene el servicio. La configuracion
de logs puede ocultar el mensaje `Now listening on`; comprobar que responda
`/health/live` o abrir Swagger. `--no-build` usa la ultima compilacion: despues
de modificar codigo hay que volver a compilar antes de ejecutarlo.

SQLite crea `products.db` y `cart.db` en los directorios de ejecucion. Bases,
logs, `bin` y `obj` no se suben al repositorio.

## Operaciones

Productos expone GET y POST en `/api/products`, y GET, PUT y DELETE en
`/api/products/{id}`. El listado permite combinar los filtros `nombre` y
`categoria`. Los IDs son Guid y los importes decimal. Se validan los datos y
se impide repetir nombre y categoria.

Antes de borrar se consulta Orders. Una orden Pendiente o Confirmada bloquea
el borrado. Si Orders no responde, no se borra el producto: se devuelve PRD-005.
Por eso el DELETE exitoso requiere Orders disponible; no hay que quitar esa
validacion para hacer una demostracion.

Carrito expone GET y DELETE en `/api/cart/{userId}`, POST en
`/api/cart/{userId}/items`, y PUT y DELETE en
`/api/cart/{userId}/items/{productId}`. POST suma cantidades y PUT las reemplaza.
Se verifica la cantidad final contra el stock, pero no se reserva ni descuenta
stock. Vaciar elimina los items, no el carrito.

Las reglas y solicitudes de ejemplo estan en [Productos](src/Products.API/PRUEBAS.md)
y [Carrito](src/Cart.API/PRUEBAS.md), y en los archivos `.http` de cada API.

## Codigos de error

| Codigo | HTTP | Significado |
| --- | --- | --- |
| PRD-001 | 404 | Producto no encontrado |
| PRD-002 | 400 | Datos de producto invalidos |
| PRD-003 | 409 | Nombre y categoria duplicados |
| PRD-004 | 409 | Producto incluido en una orden activa |
| PRD-005 | 500 | Error inesperado o fallo al consultar Orders |
| CRT-001 | 404 | Carrito no encontrado |
| CRT-002 | 404 | Producto inexistente o item no encontrado al modificar |
| CRT-003 | 422 | Stock insuficiente |
| CRT-004 | 400 | Cantidad o datos del carrito invalidos |
| CRT-005 | 500 | Error inesperado o fallo al consultar Productos |

Los errores incluyen ProblemDetails, `errorCode`, `errorMessage` y
`correlationId`. Los errores HTTP 405 y 415 conservan ese estado y usan el
codigo de solicitud invalida de su API. Los de negocio se registran como
Warning y los inesperados como Error, sin mostrar detalles internos al cliente.

## Pruebas y evidencias

Las [pruebas HTTP](tests/README.md) compilan e inician las APIs en puertos
libres, usan bases temporales y detienen sus procesos al terminar. El servidor
`tests/Dependencias` es una simulacion para provocar estados y fallas: no es
Orders real ni sustituye el trabajo de otro integrante.

Las capturas muestran respuestas reales de Swagger con los campos del catalogo:

- [Productos: 404 PRD-001](docs/capturas/productos-error-404.jpg).
- [Carrito: 404 CRT-001](docs/capturas/carrito-error-404.jpg).

Para repetirlas, ejecutar desde Swagger un GET con un Guid inexistente en
Productos y otro en Carrito. No se modifican registros con esas consultas.

En la demo se puede crear un producto, agregarlo a un carrito, modificar su
cantidad y provocar un error de stock. Mostrar luego `/health`, `/health/ready`
y `/health/live`, y los logs JSON en la carpeta `logs` de cada servicio. El
encabezado `X-Correlation-Id` permite relacionar una peticion con sus registros
y con la llamada de Carrito a Productos.

El [ejemplo de salud](docs/EJEMPLO_SALUD.md) sirve para los otros integrantes.
El [contrato con Orders](docs/INTEGRACION_ORDERS.md) explica la validacion del
borrado. Todavia falta incorporar los otros tres servicios, acordar sus
puertos, probar las integraciones reales y revisar el conjunto antes de integrar
esta rama en `main`.

