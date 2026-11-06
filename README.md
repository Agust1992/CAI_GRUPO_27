# CAI Grupo 27

Trabajo practico de Construccion de Aplicaciones Informaticas.

## Integrantes

- Luciano Armas
- Christian Schuon
- Agustin Acosta

## Idea del proyecto

Vamos a armar un e-commerce en C# y .NET usando una arquitectura de microservicios.

La base que paso la catedra esta copiada en la carpeta `MiniApi`. La dejamos en el repo para tener la referencia original y usarla como punto de partida.

Tambien empezamos una primera adaptacion en `src/Products.API`, orientada al manejo de productos, stock y categorias.

## Estado actual

Por ahora el repo tiene:

- La base original de la catedra en `MiniApi`.
- Una solucion del proyecto en `ECommerce.sln`.
- El microservicio `Products.API` con endpoints para productos.
- Persistencia local con SQLite.
- Swagger, health checks, logging y middleware de auditoria, siguiendo la estructura de la MiniApi.

Microservicios pensados para el TP:

- `Products.API`: productos, stock y categorias.
- `Users.API`: usuarios, login y bloqueo.
- `Orders.API`: ordenes.
- `Cart.API`: carrito.
- `Notifications.API`: notificaciones.

## Como correr Products.API

Se necesita el SDK de .NET 10. Swagger queda en `http://localhost:5208/swagger`
con el perfil `http`.

Restaurar y compilar:

```bash
dotnet restore ECommerce.sln
dotnet build ECommerce.sln --no-restore
```

Ejecutar:

```bash
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

Los IDs de productos son Guid. El JSON usa `nombre`, `descripcion`, `precio`,
`stock` y `categoria`. El servidor agrega `id` y `fechaCreacion` al crear.
Los ejemplos y las pruebas estan en `src/Products.API/Products.API.http` y
`src/Products.API/PRUEBAS.md`.

`/health/live` comprueba el proceso y `/health/ready` la base de Productos.
Si la base no esta disponible, ready devuelve 503 aunque live siga respondiendo
200. El ejemplo para adaptar en los otros servicios esta en
`docs/EJEMPLO_SALUD.md`.

La base anterior se adapta sin borrar productos. Los Guid asignados se
conservan en los siguientes inicios. Sigue pendiente confirmar la libreria de
persistencia mencionada por la consigna.

## Pendiente

- Agregar los otros proyectos al repo.
- Documentar puertos.
- Completar Productos: restriccion de borrado por ordenes activas (PRD-004),
  documentacion Swagger y tareas transversales de logging y correlacion.
- Subir capturas de Swagger cuando vayamos probando.

