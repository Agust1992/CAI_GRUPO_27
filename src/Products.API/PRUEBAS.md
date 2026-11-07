# Productos: operaciones y pruebas

Este avance separa las rutas en `Controllers/ProductosController.cs` y las
reglas en `Services/ProductoService.cs`. El repositorio conserva SQLite y
Dapper de la MiniApi, que se mantiene como base de persistencia del equipo.

## Para probar

Desde la raiz, con el SDK de .NET 10 activo:

```powershell
dotnet build ECommerce.sln
dotnet run --project src/Products.API --launch-profile http
```

Swagger queda en `http://localhost:5208/swagger`. Las solicitudes de ejemplo
estan en `Products.API.http`. Reemplazar `ProductId` por el Guid de un alta.
Las pruebas sobre una base propia agregan o modifican datos: usar productos de
prueba. Las comprobaciones automaticas de este avance usaron una base temporal
separada, sin tocar `products.db`.

## Comportamiento de este avance

- POST `/api/products`: 201 con producto y encabezado Location.
- GET `/api/products`: 200 con todos los productos. `nombre` busca una parte
  del nombre y `categoria` compara la categoria completa, sin distinguir
  mayusculas de minusculas. Se pueden combinar los dos filtros.
- GET `/api/products/{id}`: 200 o 404 con PRD-001. El ID es un Guid;
  un ID con formato incorrecto devuelve 400 con PRD-002.
- PUT `/api/products/{id}`: 200 con el producto actualizado o 404 con PRD-001.
- DELETE `/api/products/{id}`: 204 o 404 con PRD-001.
- Antes de borrar se consulta Orders: una referencia Pendiente o Confirmada
  devuelve 409 PRD-004. Si Orders falla, devuelve PRD-005 y no se borra.
- Datos invalidos, JSON mal formado o campos obligatorios ausentes: 400 con
  PRD-002. El stock debe estar presente; cero es valido.
- Nombre y categoria iguales a otro producto: 409 con PRD-003. La comparacion
  de duplicados es exacta. Una modificacion tampoco puede generar un duplicado;
  actualizar el propio producto sin cambiar nombre y categoria sigue permitido.
- Fallo inesperado: 500 con PRD-005, sin SQL, excepciones ni stack traces en la
  respuesta. Se registra el detalle internamente.

Los errores incluyen ProblemDetails, errorCode, errorMessage y correlationId.
Las excepciones y sus handlers siguen el ejemplo del apendice B de la consigna.
Tambien se formatean los errores HTTP del framework: 405 y 415 conservan su
estado HTTP y usan PRD-002 para identificar una solicitud invalida. No se crean
codigos nuevos fuera del catalogo. Los cuerpos son application/problem+json.

Productos conserva un X-Correlation-Id valido recibido, lo devuelve y lo envia
a Orders. Serilog registra inicio/fin y duracion, con Service, Endpoint y
CorrelationId. Los errores de negocio son Warning y los inesperados Error.
Los archivos son JSON estructurado; ya no se guardan cuerpos de solicitudes.

Swagger incorpora comentarios XML, esquemas con los campos propios de error y
ejemplos JSON de alta, modificacion, listado, detalle y errores, incluido PRD-004.

## Contrato de productos

Las solicitudes de alta y modificacion usan `nombre`, `descripcion`, `precio`,
`stock` y `categoria`. La descripcion es opcional. Precio y stock deben estar
presentes en el JSON. Los DTOs son `CrearProductoRequest` y
`ActualizarProductoRequest`.

Las respuestas agregan `id` (Guid) y `fechaCreacion` (fecha UTC en ISO 8601).
El servidor asigna ambos al crear; no se pueden elegir ni cambiar enviandolos
en un request. No se exponen el ID numerico interno ni la fecha de modificacion.

Ejemplo de alta:

```json
{
  "nombre": "Notebook",
  "descripcion": "Producto de ejemplo",
  "precio": 1500.50,
  "stock": 10,
  "categoria": "Tecnologia"
}
```

## Bases anteriores

Al iniciar, se agregan las columnas `public_id` y `price_decimal` si faltan.
Cada producto anterior recibe un Guid una sola vez. Se conserva el ID numerico
interno y no se borran ni recrean tablas. La adaptacion se hace dentro de una
transaccion; al reiniciar no se generan identificadores nuevos para esas filas.

SQLite guardaba los precios anteriores como REAL. Los precios nuevos o
modificados tambien se guardan como texto decimal exacto en `price_decimal`,
para evitar redondeos. La columna anterior se conserva por compatibilidad.
Los precios anteriores se leen tal como estaban guardados: no se intenta
reconstruir precision que la base vieja ya pudiera haber perdido.

## Casos comprobados

Alta, consulta, filtros solos y combinados, modificacion, borrado y recurso
inexistente. Nombre de 100/101 caracteres, descripcion de 500/501, campos
nulos o ausentes, precio cero o negativo y stock cero o negativo. Los pedidos
invalidos no modificaron datos. Se verifico que una modificacion preserve la
fecha de creacion y que los productos sigan guardados al reiniciar la API.

En seis altas simultaneas iguales, una devolvio 201 y cinco devolvieron 409.
Se simulo un fallo de persistencia en otra base temporal para comprobar PRD-005.
Estas son comprobaciones HTTP contra la API, no un proyecto de tests unitarios.
Se verificaron el contrato en espanol, los tipos en Swagger, IDs invalidos,
fechas asignadas por el servidor y la precision de los precios. Tambien se
probo una base con el esquema anterior: conserva sus productos e IDs internos,
admite modificaciones por Guid y no cambia los Guid al reiniciar.
La solucion compilo sin errores ni advertencias despues de actualizar las
dependencias indicadas abajo.

## Salud

Se publican `/health`, `/health/ready` y `/health/live` con respuestas JSON.
El estado general consulta API y SQLite; ready consulta SQLite y live solo API.
La verificacion de SQLite comprueba la tabla y sus columnas, sin crear bases
faltantes ni devolver excepciones internas al cliente.

Se prueba una falla temporal de la tabla: ready y health deben devolver 503
Unhealthy, mientras live conserva 200 Healthy. Luego se restaura la tabla y
ready debe volver a 200 sin reiniciar. Se usan bases temporales separadas.
El ejemplo para los otros integrantes esta en `docs/EJEMPLO_SALUD.md`.

## Pendientes antes de integrar con los otros servicios

- Verificar la consulta y la correlacion con Orders.API real de Christian.
  El contrato usado esta en docs/INTEGRACION_ORDERS.md.
- Revisar con Agustin y Christian la convencion comun de logs y errores.
- Cada integrante adapta y verifica los controles de salud en sus servicios.
- Revisar con el grupo el conjunto de evidencias y la demo de entrega. Las
  capturas actuales de errores estan en docs/capturas.

Las pruebas repetibles estan en tests/ProbarProductos.ps1 y no usan datos de
trabajo. Para cerrar los avisos de dependencias se actualizaron
Microsoft.Data.Sqlite a 10.0.14, sin suprimirlos. Se retiro el panel opcional
HealthChecks UI y su recolector, que fallaba al iniciar por una dependencia
faltante. Tambien se quitaron sus paquetes de almacenamiento y KubernetesClient.
Se conserva UI.Client para escribir el JSON de los tres controles obligatorios.

La ruta anterior `/products` se reemplazo por `/api/products`; no hay dos juegos
de endpoints en paralelo. No se modificaron MiniApi, Orders, Users, Cart ni
Notifications.
