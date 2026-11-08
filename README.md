# CAI Grupo 27

Trabajo práctico de Construcción de Aplicaciones Informáticas.  
E-commerce con microservicios en C# y .NET 10.

## Integrantes

- Luciano Armas
- Christian Schuon
- Agustin Acosta

## Idea del proyecto

Vamos a armar un e-commerce usando una arquitectura de microservicios. La base entregada por la cátedra se conserva en `MiniApi` como referencia.

## Estado actual

El repositorio contiene:

- `Products.API`: productos, categorías y stock.
- `Cart.API`: carrito de compras; consulta `Products.API` por HTTP.
- `Users.API`: registro, inicio de sesión, bloqueo y consulta de usuarios.
- `Notifications.API`: creación y consulta de notificaciones; consulta `Users.API` por HTTP.
- `Orders.API`: servicio pendiente de integrar en esta solución.
- `MiniApi`: base original entregada por la cátedra.

Durante el desarrollo, cada servicio guarda sus datos en una base SQLite local. La conexión y la persistencia deberán ajustarse al entorno que se use para la evaluación.

## Puertos

| Servicio | HTTP | HTTPS | Swagger |
| :--- | :--- | :--- | :--- |
| Products.API | `http://localhost:5208` | Configurado en el perfil del proyecto | `/swagger` |
| Cart.API | `http://localhost:5104` | Configurado en el perfil del proyecto | `/swagger` |
| Users.API | `http://localhost:5290` | `https://localhost:7085` | `/swagger` |
| Notifications.API | `http://localhost:5103` | `https://localhost:7086` | `/swagger` |
| Orders.API | A coordinar | A coordinar | `/swagger` |

El puerto `5103` corresponde a `Notifications.API`; hay que acordar otro puerto para `Orders.API`.

## Requisitos

Se necesita el SDK de .NET 10. Desde la carpeta principal del repositorio:

```powershell
dotnet restore ECommerce.sln
dotnet build ECommerce.sln --no-restore
```

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
- `GET /health-ui`

Los IDs son Guid. Al crear un producto, el servidor asigna su ID y fecha de creación. Los pedidos de ejemplo están en `src/Products.API/Products.API.http`; las pruebas y reglas están en `src/Products.API/PRUEBAS.md`.

El borrado consulta `Orders.API` antes de eliminar. Su dirección se configura en `Services:Orders` y queda pendiente de coordinar con el grupo. Si Orders no responde, Products conserva el producto y devuelve `PRD-005`. El contrato de integración está en `docs/INTEGRACION_ORDERS.md`.

## Cart.API

Cart consulta `Products.API` por HTTP para comprobar que el producto exista y que haya stock. No descuenta ni reserva stock.

Para iniciar:

```powershell
dotnet run --project src/Cart.API/Cart.API.csproj --launch-profile http
```

Swagger: `http://localhost:5104/swagger`

Los ejemplos están en `src/Cart.API/Cart.API.http`; las reglas y pruebas están en `src/Cart.API/PRUEBAS.md`.

## Users.API

Users registra usuarios, inicia sesión, bloquea cuentas tras intentos fallidos y permite consultar un usuario por ID.

Para iniciar:

```powershell
dotnet run --project src/Users.API/Users.API.csproj --launch-profile http
```

Swagger: `http://localhost:5290/swagger`

Endpoints:

- `POST /api/users/register`
- `POST /api/users/login`
- `GET /api/users/{id}`
- `GET /health`
- `GET /health/ready`
- `GET /health/live`

La contraseña se guarda como hash y nunca se devuelve en las respuestas. El endpoint de consulta por ID también permite que `Notifications.API` compruebe si existe el usuario.

## Notifications.API

Notifications registra notificaciones y permite consultar las de un usuario. Antes de registrarlas, comprueba el usuario consultando `Users.API`.

Para iniciar:

```powershell
dotnet run --project src/Notifications.API/Notifications.API.csproj --launch-profile http
```

Swagger: `http://localhost:5103/swagger`

Para probar la comunicación entre ambas APIs, iniciar primero `Users.API` y después `Notifications.API`.

Endpoints:

- `POST /api/notifications/send`
- `GET /api/notifications/{userId}`
- `GET /health`
- `GET /health/ready`
- `GET /health/live`

## Códigos de error

| Servicio | Código | Significado |
| :--- | :--- | :--- |
| Products | `PRD-001` | Producto no encontrado |
| Products | `PRD-002` | Datos del producto inválidos |
| Products | `PRD-003` | Ya existe un producto con ese nombre y categoría |
| Products | `PRD-004` | El producto tiene órdenes activas y no puede eliminarse |
| Products | `PRD-005` | Error interno al procesar el producto |
| Cart | `CRT-001` | Carrito inexistente |
| Cart | `CRT-002` | Producto inexistente o artículo no encontrado |
| Cart | `CRT-003` | Stock insuficiente |
| Cart | `CRT-004` | Cantidad o datos inválidos |
| Cart | `CRT-005` | Error interno, de persistencia o de comunicación con Products |
| Users | `USR-001` | El email ya está registrado |
| Users | `USR-002` | Datos del usuario inválidos |
| Users | `USR-003` | Credenciales incorrectas |
| Users | `USR-004` | Usuario bloqueado por intentos fallidos |
| Users | `USR-005` | Usuario bloqueado por detección de fraude |
| Users | `USR-006` | Error interno al procesar el usuario |
| Users | `USR-007` | Usuario no encontrado |
| Notifications | `NTF-001` | Usuario no encontrado |
| Notifications | `NTF-002` | Datos de la notificación inválidos |
| Notifications | `NTF-003` | El usuario no tiene notificaciones |
| Notifications | `NTF-004` | Error interno al procesar la notificación |

## Monitoreo y registros

Todos los servicios implementados publican:

- `/health`: estado general.
- `/health/ready`: disponibilidad de la base de datos y dependencias necesarias.
- `/health/live`: indica si la API está encendida.

Las APIs usan Serilog para registrar pedidos y errores. Las respuestas de error incluyen un identificador de correlación para relacionarlas con los registros.

## Pruebas

Desde la raíz del repositorio, las pruebas repetibles disponibles son:

```powershell
.\tests\ProbarProductos.ps1
.\tests\ProbarCarrito.ps1
```

Son pruebas HTTP de integración. `tests/Dependencias` es un servidor simulado que usan las pruebas; no reemplaza a `Orders.API` ni a los servicios reales del grupo.

## Pendiente

- Integrar `Orders.API` y acordar sus puertos con el grupo.
- Probar la integración de Products con Orders real.
- Confirmar la configuración de persistencia que se utilizará en la evaluación.
- Agregar capturas de Swagger para la entrega.
- Completar el diagrama y la documentación general cuando estén integrados los servicios del grupo.

## Diagrama de arquitectura

```text
                    ┌──────────────┐
                    │   Cliente    │
                    └──────┬───────┘
                           │ HTTP
       ┌──────────┬────────┼──────────┬────────────┐
       ▼          ▼        ▼          ▼            ▼
 Products.API  Cart.API  Users.API  Orders.API  Notifications.API
       ▲          │                   │                │
       └──────────┘                   ├── consulta ───► Users.API
                                      └── consulta ───► Products.API
                                                        │
                                                        └── consulta ─► Users.API
```