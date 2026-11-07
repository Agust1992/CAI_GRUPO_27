# Carrito

API de carrito en .NET 10. Se agrego a ECommerce.sln sin referencias al proyecto
de Productos: los servicios se comunican por HTTP.

## Ejecutar

Restaurar y compilar la solucion. Dejar Products.API ejecutandose en el puerto
5208. En otra terminal:

```powershell
dotnet run --project src/Cart.API --launch-profile http
```

Swagger: `http://localhost:5104/swagger`. Los ejemplos estan en `Cart.API.http`.
Para agregar un item, crear antes un producto desde Swagger de Productos y
copiar el Guid devuelto; no usar el ID numerico de una base anterior.

La direccion de Productos se configura en `Services:Products` de
appsettings.json, o con la variable `Services__Products`. No esta dentro de la
logica. El cliente usa IHttpClientFactory y un timeout de cinco segundos.
La base propia se configura en `ConnectionStrings:DefaultConnection`.

## Operaciones

- GET `/api/cart/{userId}` devuelve el carrito o 404 CRT-001 si nunca se creo.
- POST `/api/cart/{userId}/items` agrega un producto. Crea el carrito si hace
  falta. Devuelve 200 con el estado resultante.
- PUT `/api/cart/{userId}/items/{productId}` modifica la cantidad de un item
  existente y devuelve 200.
- DELETE `/api/cart/{userId}/items/{productId}` quita un item y devuelve 204.
- DELETE `/api/cart/{userId}` vacia los items y devuelve 204.

El JSON usa usuarioId, items, productoId, cantidad y fechaActualizacion. Los IDs
son Guid y la fecha se asigna en UTC por el servidor. No se devuelven precios
ni se agregan funciones de checkout o pagos.

## Reglas y decisiones

POST suma a la cantidad existente y PUT la reemplaza. Al sumar se compara el
total resultante contra el stock, no solamente lo que llega en el ultimo
pedido. La operacion se guarda dentro de
una transaccion para no perder sumas simultaneas.

Antes de guardar un alta o una modificacion se consulta Products.API para
validar existencia y stock. Carrito no lee la base de Productos, no reserva
unidades y no descuenta stock.
No se valida existencia en Users.API porque la consigna no lo exige para Cart.

La consigna no detalla todos los casos de items ausentes. En esta
implementacion, PUT de un producto que no esta en un carrito existente devuelve
404 CRT-002. DELETE de un item que ya no esta devuelve 204, sin cambiar la
fecha. Si no existe el carrito, devuelve CRT-001.

Vaciar conserva el carrito con una lista vacia; el GET siguiente devuelve 200.
Repetir el vaciado no modifica un carrito que ya estaba vacio. Revisar estos
criterios con el grupo si el docente indica otra interpretacion.

## Errores

- CRT-001, 404: carrito inexistente.
- CRT-002, 404: producto inexistente en Productos o item no encontrado en PUT.
- CRT-003, 422: stock insuficiente para la cantidad final.
- CRT-004, 400: cantidad o datos invalidos, incluidos IDs mal formados.
- CRT-005, 500: error de persistencia, dependencia caida, timeout o respuesta
  inesperada de Productos. Un 500 de Productos nunca se interpreta como 404.

Se usan las excepciones y los IExceptionHandler del mismo estilo que Productos.
Los errores incluyen ProblemDetails, errorCode, errorMessage y correlationId.
No incluyen excepciones internas, SQL ni cuerpos de error de otro servicio.

## Salud y logs

Las rutas `/health`, `/health/live` y `/health/ready` devuelven JSON. Live
comprueba el proceso y ready la estructura de la base local. Ready no consulta
Productos: sus fallas se controlan en cada POST y PUT. Consultar o vaciar un
carrito no requiere que Productos este disponible.

Serilog escribe consola y archivos JSON diarios en logs, que no se suben al
repositorio. Los eventos incluyen Service, Endpoint y CorrelationId. Se
registran inicio, fin y duracion; los errores de negocio son Warning y los
inesperados Error. No se registran cuerpos de solicitudes ni secretos.

Cart conserva un X-Correlation-Id recibido de hasta 100 caracteres ASCII sin
espacios, o genera un Guid. Lo devuelve como encabezado y lo propaga a
Productos. Falta coordinar con Christian y Agustin el ejemplo comun definitivo;
Productos tambien conserva el encabezado y lo envia a Orders. La propagacion
se prueba con Productos real y con la dependencia de Orders simulada.

## Persistencia y pendientes

Se usa SQLite/Dapper siguiendo la base MiniApi, con tablas carts y cart_items
propias. Esa es la base de persistencia adoptada por el equipo a partir de
la MiniApi.

Swagger incluye ejemplos JSON de solicitudes, respuestas y errores. Los errores
de validacion y de HTTP, incluidos 405 y 415, conservan el formato comun. El PUT
documenta los dos casos de 404: carrito inexistente e item inexistente.
Se actualizo Microsoft.Data.Sqlite a 10.0.14 para dejar de usar la dependencia
nativa con la advertencia de seguridad detectada en la auditoria.

Antes de cerrar: revisar los contratos comunes con el grupo y probar el
conjunto con Orders real. Las capturas actuales de errores estan en docs/capturas.

Las pruebas de este avance usan dos APIs reales con bases temporales. Tambien
se usa un servidor simulado para respuestas invalidas, errores y timeout de
Productos. Esa simulacion no reemplaza la prueba real de integracion.

Desde la raiz del repositorio, ejecutar `tests/ProbarCarrito.ps1` con el SDK de
.NET 10 activo. Verifica POST acumulativo, PUT de reemplazo, cantidades
invalidas, carrito o producto inexistente, stock insuficiente, usuarios
independientes, sumas simultaneas, borrado, vaciado y conservacion de datos al
reiniciar. Incluye respuestas invalidas, timeout y caida de Productos, ademas
de Swagger, logs JSON y correlacion. Imprime la carpeta de evidencia y detiene
los procesos que inicio. Estas son pruebas HTTP de integracion, no unitarias.
