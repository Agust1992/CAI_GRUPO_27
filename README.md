# CAI Grupo 27

Trabajo práctico de Construcción de Aplicaciones Informáticas.  
E-commerce con microservicios en C# y .NET 10.

## Integrantes

- Luciano Armas
- Christian Schuon
- Agustin Acosta

## Estado del trabajo

El sistema se divide en cinco APIs:

- `Products.API`: productos, categorías y stock.
- `Cart.API`: carrito de compras.
- `Users.API`: registro, inicio de sesión, bloqueo y consulta de usuarios.
- `Notifications.API`: creación y consulta de notificaciones.
- `Orders.API`: pendiente de integrar en esta solución.

Todos los servicios del TP se agregan a `ECommerce.sln`. La carpeta `MiniApi` y el archivo `MiniApi.slnx` conservan la referencia original de la cátedra; no son un sexto microservicio.

Durante el desarrollo, cada API utiliza su propia base SQLite local y Dapper. Los servicios se comunican por HTTP y no acceden directamente a las bases de datos de otras APIs.

## Arquitectura

```text
Cliente ──HTTP──> Products.API
   │                 ▲
   ├──HTTP──> Cart.API ──HTTP──┘
   ├──HTTP──> Users.API
   ├──HTTP──> Notifications.API ──HTTP──> Users.API
   └──HTTP──> Orders.API

Products.API consulta Orders.API antes de borrar un producto.
Dentro de cada API, el recorrido es Controller -> Service -> Repository.
```

## Puertos

| Servicio | HTTP | HTTPS | Swagger |
| --- | --- | --- | --- |
| Products.API | `http://localhost:5208` | `https://localhost:7149` | `/swagger` |
| Cart.API | `http://localhost:5104` | No configurado | `/swagger` |
| Users.API | `http://localhost:5290` | `https://localhost:7085` | `/swagger` |
| Notifications.API | `http://localhost:5103` | `https://localhost:7086` | `/swagger` |
| Orders.API | A coordinar con Christian | A coordinar | `/swagger` |

El puerto `5103` está asignado a Notifications.API. Products.API todavía tiene `http://localhost:5103/` como dirección provisional para Orders.API en `Services:Orders`. Hay que reemplazarla por el puerto que se acuerde con Christian antes de probar esa integración.

Notifications.API consulta Users.API por HTTP en `http://localhost:5290`, de acuerdo con el perfil `http` de Users.API. Las direcciones entre servicios se configuran en el `appsettings.json` de cada API.

## Requisitos y compilación

Se necesita el SDK de .NET 10. Desde la raíz del repositorio:

```powershell
dotnet --version
dotnet restore ECommerce.sln
dotnet build ECommerce.sln --no-restore
```

El primer comando debe mostrar una versión 10.x.

## Products.API

Swagger: `http://localhost:5208/swagger`

Para iniciar:

```powershell
dotnet run --project src/Products.API/Products.API.csproj --launch-profile http
```

Endpoints principales:

- `GET /api/products?nombre=&categoria=`
- `GET /api/products/{id}`
- `POST /api/products`
- `PUT /api/products/{id}`
- `DELETE /api/products/{id}`
- `GET /health`
- `GET /health/ready`
- `GET /health/live`

Los IDs son Guid y los importes son decimales. El servidor asigna el ID y la fecha de creación. Se validan los datos y se impide repetir el mismo nombre y categoría.

Antes de borrar, Products consulta Orders. Una orden pendiente o confirmada impide el borrado. Si Orders no responde, Products conserva el producto y devuelve `PRD-005`. La dirección se configura en `Services:Orders`. El contrato está documentado en [docs/INTEGRACION_ORDERS.md](docs/INTEGRACION_ORDERS.md).

Los ejemplos están en [src/Products.API/Products.API.http](src/Products.API/Products.API.http) y las reglas de prueba en [src/Products.API/PRUEBAS.md](src/Products.API/PRUEBAS.md).

## Cart.API

Swagger: `http://localhost:5104/swagger`

Para iniciar:

```powershell
dotnet run --project src/Cart.API/Cart.API.csproj --launch-profile http
```

Endpoints principales:

- `GET /api/cart/{userId}`
- `POST /api/cart/{userId}/items`
- `PUT /api/cart/{userId}/items/{productId}`
- `DELETE /api/cart/{userId}/items/{productId}`
- `DELETE /api/cart/{userId}`
- `GET /health`
- `GET /health/ready`
- `GET /health/live`

Cart consulta Products por HTTP para comprobar que el producto exista y tenga stock. Al agregar un artículo, suma cantidades; al modificarlo, reemplaza la cantidad. Comprueba la cantidad final contra el stock disponible, pero no reserva ni descuenta stock. Vaciar el carrito elimina sus artículos, no el carrito.

Los ejemplos están en [src/Cart.API/Cart.API.http](src/Cart.API/Cart.API.http) y las reglas de prueba en [src/Cart.API/PRUEBAS.md](src/Cart.API/PRUEBAS.md).

## Users.API

Swagger: `http://localhost:5290/swagger`

Para iniciar:

```powershell
dotnet run --project src/Users.API/Users.API.csproj --launch-profile http
```

Endpoints principales:

- `POST /api/users/register`
- `POST /api/users/login`
- `GET /api/users/{id}`
- `GET /health`
- `GET /health/ready`
- `GET /health/live`

Users permite registrar usuarios, iniciar sesión y consultar un usuario por ID. La contraseña se guarda como hash y no se devuelve en las respuestas. Al tercer intento de inicio de sesión incorrecto se bloquea la cuenta y se devuelve `403 USR-004`.

El health de disponibilidad comprueba la conexión a SQLite, la existencia de la tabla `users` y las columnas que necesita el servicio.

## Notifications.API

Swagger: `http://localhost:5103/swagger`

Para iniciar:

```powershell
dotnet run --project src/Notifications.API/Notifications.API.csproj --launch-profile http
```

Endpoints principales:

- `POST /api/notifications/send`
- `GET /api/notifications/{userId}`
- `GET /health`
- `GET /health/ready`
- `GET /health/live`

Notifications registra notificaciones y permite consultar las de un usuario. Antes de registrar una notificación, consulta Users.API por HTTP para comprobar que el usuario exista. Para probar la comunicación, iniciar primero Users.API y después Notifications.API.

## Persistencia

Cada servicio utiliza una base SQLite local independiente:

- Products: `products.db`
- Cart: `cart.db`
- Users: `users.db`
- Notifications: `notifications.db`

Las bases se crean localmente al iniciar cada API. La configuración deberá revisarse para el entorno que se use en la evaluación.

## Códigos de error

| Servicio | Código | Significado |
| --- | --- | --- |
| Products | `PRD-001` | Producto no encontrado |
| Products | `PRD-002` | Datos del producto inválidos |
| Products | `PRD-003` | Ya existe un producto con ese nombre y categoría |
| Products | `PRD-004` | El producto tiene órdenes activas y no puede eliminarse |
| Products | `PRD-005` | Error interno o fallo al consultar Orders |
| Cart | `CRT-001` | Carrito no encontrado |
| Cart | `CRT-002` | Producto inexistente o artículo no encontrado |
| Cart | `CRT-003` | Stock insuficiente |
| Cart | `CRT-004` | Cantidad o datos del carrito inválidos |
| Cart | `CRT-005` | Error interno o fallo al consultar Products |
| Users | `USR-001` | El email ya está registrado |
| Users | `USR-002` | Datos del usuario inválidos |
| Users | `USR-003` | Credenciales incorrectas |
| Users | `USR-004` | Usuario bloqueado por intentos fallidos |
| Users | `USR-005` | Usuario bloqueado por detección de fraude |
| Users | `USR-006` | Error interno al procesar el usuario |
| Users | `USR-007` | Usuario no encontrado |
| Users | `USR-008` | Método HTTP no permitido |
| Users | `USR-009` | Tipo de contenido no soportado |
| Notifications | `NTF-001` | Usuario no encontrado |
| Notifications | `NTF-002` | Datos de la notificación inválidos |
| Notifications | `NTF-003` | El usuario no tiene notificaciones |
| Notifications | `NTF-004` | Error interno al procesar la notificación |

Las respuestas de error incluyen `errorCode`, `errorMessage` y `correlationId`. Los errores HTTP 404, 405 y 415 que genera el framework también usan ese formato. Los logs incluyen servicio, endpoint y correlación, y agregan el código de error cuando corresponde.

## Monitoreo y registros

Las APIs implementadas exponen:

- `/health`: estado general.
- `/health/ready`: disponibilidad de la base de datos y dependencias necesarias.
- `/health/live`: indica si la API está encendida.

Los servicios usan Serilog para registrar solicitudes y errores. Los registros JSON se guardan en la carpeta `logs` de cada servicio. El encabezado `X-Correlation-Id` permite relacionar una solicitud con sus registros y, cuando corresponde, con las llamadas entre servicios.

## Pruebas

El procedimiento de prueba manual está en [tests/README.md](tests/README.md). Los archivos `.http` de cada API también contienen solicitudes de ejemplo.

Las pruebas disponibles en ese procedimiento cubren Productos y Carrito. La integración real de Products con Orders y las pruebas conjuntas con Users y Notifications quedan pendientes de la integración del grupo. Las capturas de Swagger están en [docs/capturas](docs/capturas).

## Pendiente

- Acordar con Christian un puerto para Orders distinto de `5103`.
- Actualizar `Services:Orders` en Products.API con el puerto acordado.
- Probar Products con Orders real.
- Probar juntos Users y Notifications.
- Confirmar la configuración de persistencia que se utilizará en la evaluación.