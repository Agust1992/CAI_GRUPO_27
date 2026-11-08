# CAI Grupo 27

Trabajo práctico de Construcción de Aplicaciones Informáticas.  
E-commerce con microservicios en C# y .NET 10.

## Integrantes

- Luciano Armas
- Christian Schuon
- Agustin Acosta

## Estado del trabajo

El sistema es un e-commerce dividido en cinco APIs:

- `Products.API`: productos, categorías y stock.
- `Cart.API`: carrito de compras.
- `Users.API`: registro, inicio de sesión, bloqueo y consulta de usuarios.
- `Notifications.API`: creación y consulta de notificaciones.
- `Orders.API`: pendiente de integrar en esta solución.

Todos los servicios del TP se agregan a `ECommerce.sln`. La carpeta `MiniApi` y el archivo `MiniApi.slnx` conservan la referencia original de la cátedra; no son un sexto microservicio.

Durante el desarrollo, cada API utiliza su propia base SQLite local y Dapper. Las conexiones y la persistencia deberán ajustarse al entorno que se use para la evaluación.

## Arquitectura

```text
                    ┌──────────────┐
                    │   Cliente    │
                    └──────┬───────┘
                           │ HTTP
       ┌──────────┬────────┼──────────┬─────────────────┐
       ▼          ▼        ▼          ▼                 ▼
 Products.API  Cart.API  Users.API  Orders.API  Notifications.API
       ▲          │                   ▲                 │
       └──────────┘                   │                 │
                                     consulta          consulta
                                       │                 ▼
                                       └──────────── Users.API

Products.API consulta Orders.API antes de borrar un producto.
Cart.API consulta Products.API para validar productos y stock.
Dentro de cada API, el recorrido general es Controller -> Service -> Repository. Los servicios se comunican entre sí por HTTP; no acceden directamente a las bases de datos de otras APIs.
Puertos
Servicio	HTTP	HTTPS	Swagger
Products.API	http://localhost:5208	Configurado en el perfil del proyecto	/swagger
Cart.API	http://localhost:5104	Configurado en el perfil del proyecto	/swagger
Users.API	http://localhost:5290	https://localhost:7085	/swagger
Notifications.API	http://localhost:5103	https://localhost:7086	/swagger
Orders.API	A coordinar	A coordinar	/swagger


El puerto 5103 corresponde a Notifications.API. Hay que acordar otro puerto para Orders.API.

Las direcciones entre servicios se configuran en los appsettings.json de cada API. Por ejemplo, Cart.API consulta Products.API, y Notifications.API consulta Users.API.
Requisitos y compilación
Se necesita el SDK de .NET 10. Desde la raíz del repositorio, comprobar la versión, restaurar los paquetes y compilar:
dotnet --version
dotnet restore ECommerce.sln
dotnet build ECommerce.sln --no-restore
El primer comando debe mostrar una versión 10.x.
Products.API
Swagger: http://localhost:5208/swagger

Para iniciar:
dotnet run --project src/Products.API/Products.API.csproj --launch-profile http
Endpoints principales:
- GET /api/products?nombre=&categoria=
- GET /api/products/{id}
- POST /api/products
- PUT /api/products/{id}
- DELETE /api/products/{id}
- GET /health
- GET /health/ready
- GET /health/live
- GET /health-ui
Los IDs son Guid y los importes son decimales. Al crear un producto, el servidor asigna su ID y fecha de creación. Se validan los datos y se impide repetir el mismo nombre y categoría.

El borrado consulta Orders.API antes de eliminar. Su dirección se configura en Services:Orders y queda pendiente de coordinar con el grupo. Una orden pendiente o confirmada puede impedir el borrado. Si Orders no responde, Products conserva el producto y devuelve PRD-005.

Los pedidos de ejemplo están en src/Products.API/Products.API.http. Las reglas y pruebas están en src/Products.API/PRUEBAS.md. El contrato para coordinar con Orders está en docs/INTEGRACION_ORDERS.md.
Cart.API
Swagger: http://localhost:5104/swagger

Para iniciar:
dotnet run --project src/Cart.API/Cart.API.csproj --launch-profile http
Cart consulta Products.API por HTTP para comprobar que el producto exista y que haya stock. No accede a la base de datos de Products, ni descuenta ni reserva stock.

Cart expone operaciones para consultar y vaciar el carrito, agregar o modificar artículos y eliminar artículos. Al agregar, suma cantidades; al modificar, reemplaza la cantidad. En ambos casos verifica la cantidad final contra el stock disponible.

Los pedidos de ejemplo están en src/Cart.API/Cart.API.http. Las reglas y pruebas están en src/Cart.API/PRUEBAS.md.
Users.API
Swagger: http://localhost:5290/swagger

Para iniciar:
dotnet run --project src/Users.API/Users.API.csproj --launch-profile http
Endpoints:
- POST /api/users/register
- POST /api/users/login
- GET /api/users/{id}
- GET /health
- GET /health/ready
- GET /health/live
Users permite registrar usuarios, iniciar sesión, bloquear cuentas tras intentos fallidos y consultar un usuario por ID. La contraseña se almacena como hash y no se devuelve en las respuestas.

Notifications.API consulta el endpoint de usuario por ID para verificar que el destinatario exista antes de guardar una notificación.
Notifications.API
Swagger: http://localhost:5103/swagger

Para iniciar:
dotnet run --project src/Notifications.API/Notifications.API.csproj --launch-profile http
Para probar la comunicación entre ambas APIs, iniciar primero Users.API y después Notifications.API.

Endpoints:
- POST /api/notifications/send
- GET /api/notifications/{userId}
- GET /health
- GET /health/ready
- GET /health/live
Notifications registra notificaciones y permite consultar las de un usuario. Antes de registrar una notificación, verifica que el usuario exista consultando Users.API.
Persistencia
Durante el desarrollo, cada servicio usa una base SQLite local independiente:
- Products: products.db
- Cart: cart.db
- Users: users.db
- Notifications: notifications.db
Las bases se crean localmente al iniciar cada API. La configuración deberá revisarse para el entorno que se use en la evaluación.
Códigos de error
Servicio	Código	Significado
Products	PRD-001	Producto no encontrado
Products	PRD-002	Datos del producto inválidos
Products	PRD-003	Ya existe un producto con ese nombre y categoría
Products	PRD-004	El producto tiene órdenes activas y no puede eliminarse
Products	PRD-005	Error interno o fallo al consultar Orders
Cart	CRT-001	Carrito no encontrado
Cart	CRT-002	Producto inexistente o artículo no encontrado
Cart	CRT-003	Stock insuficiente
Cart	CRT-004	Cantidad o datos del carrito inválidos
Cart	CRT-005	Error interno o fallo al consultar Products
Users	USR-001	El email ya está registrado
Users	USR-002	Datos del usuario inválidos
Users	USR-003	Credenciales incorrectas
Users	USR-004	Usuario bloqueado por intentos fallidos
Users	USR-005	Usuario bloqueado por detección de fraude
Users	USR-006	Error interno al procesar el usuario
Users	USR-007	Usuario no encontrado
Notifications	NTF-001	Usuario no encontrado
Notifications	NTF-002	Datos de la notificación inválidos
Notifications	NTF-003	El usuario no tiene notificaciones
Notifications	NTF-004	Error interno al procesar la notificación


Las respuestas de error incluyen un código de error y un identificador de correlación para relacionarlas con los registros de la API.
Monitoreo y registros
Las APIs implementadas exponen:
- /health: estado general.
- /health/ready: disponibilidad de la base de datos y dependencias necesarias.
- /health/live: indica si la API está encendida.
Los servicios usan Serilog para registrar pedidos y errores. Los registros JSON se guardan en la carpeta logs de cada servicio.

El encabezado X-Correlation-Id permite relacionar una solicitud con sus registros y, cuando corresponde, con las llamadas entre servicios.
Pruebas
Desde la raíz del repositorio se pueden ejecutar las pruebas HTTP disponibles:
.\tests\ProbarProductos.ps1
.\tests\ProbarCarrito.ps1
Los ejemplos de solicitudes también están en los archivos .http de cada API y se pueden ejecutar desde Visual Studio.

tests/Dependencias contiene un servidor simulado usado por algunas pruebas. No reemplaza a Orders.API ni a los servicios reales del grupo.

El procedimiento general de pruebas está en tests/README.md. Las capturas de respuestas disponibles están en docs/capturas.