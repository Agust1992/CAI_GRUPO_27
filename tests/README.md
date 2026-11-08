# Pruebas manuales de Productos y Carrito

Iniciar las APIs siguiendo el README principal. Productos se prueba en
http://localhost:5208/swagger y Carrito en http://localhost:5104/swagger.
En cada operacion, usar Try it out, completar los datos y presionar Execute.
Revisar el HTTP status y el cuerpo de la respuesta.

Usar productos y usuarios de prueba, no datos de trabajo ni la BD del profesor.
Las operaciones POST, PUT y DELETE modifican datos. Los archivos .http de cada
API contienen solicitudes de ejemplo; no es necesario ejecutarlos para usar
Swagger.

## 1. Crear un producto

Enviar POST /api/products:

```json
{
  "nombre": "Notebook de prueba",
  "descripcion": "Producto para la demostracion",
  "precio": 1000,
  "stock": 5,
  "categoria": "Tecnologia"
}
```

Esperado: 201, encabezado Location y producto con Id y fechaCreacion asignados
por el servidor. Copiar el Id para los siguientes pasos. Si ya existe ese
nombre y categoria, elegir otro nombre de prueba.

## 2. Consultar y modificar

- GET /api/products/{id}: 200 con el producto creado.
- GET /api/products con nombre=Notebook y categoria=Tecnologia: 200 con el
  producto entre los resultados. Tambien probar cada filtro por separado.
- PUT /api/products/{id}: usar los mismos campos, cambiar precio a 1200 y
  descripcion. Mantener stock en 5. Esperado: 200 con los cambios, el mismo Id
  y la fechaCreacion anterior.
- GET del mismo Id: debe conservar los cambios.
- POST con el mismo nombre y categoria: 409 PRD-003, sin crear otro producto.
- GET con un Guid que no exista: 404 PRD-001.

## 3. Validaciones de Productos

Partir de un cuerpo valido y cambiar un dato por vez, tanto en POST como PUT.
Esperado en los siguientes casos: 400 PRD-002.

- Nombre vacio, solo espacios, nulo o de mas de 100 caracteres.
- Descripcion de mas de 500 caracteres.
- Precio cero, negativo, nulo o texto no numerico como "hola".
- Stock negativo, fraccionario como 1.5, nulo o texto no numerico.
- Stock mayor al rango de int, por ejemplo 2147483648.
- Categoria vacia o nula.
- Precio o stock omitidos del JSON.

Comprobar por GET que los datos anteriores no hayan cambiado y que no se haya
creado un producto por una peticion rechazada. Stock cero y descripcion nula
son validos. La categoria no esta restringida a la lista de ejemplos.

## 4. Agregar y modificar el carrito

Usar el producto anterior y un Guid de usuario de prueba sin carrito, por
ejemplo a1b2c3d4-0000-0000-0000-111122223333. Si ese usuario ya tiene un carrito,
usar otro Guid de prueba para comenzar con el estado esperado.

1. GET /api/cart/{userId}: inicialmente 404 CRT-001.
2. POST /api/cart/{userId}/items con el producto y cantidad 2: 200, cantidad 2.
3. Repetir POST con cantidad 1: 200, el mismo item queda con cantidad 3.
4. PUT /api/cart/{userId}/items/{productId} con cantidad 4: 200, quedan 4, no 7.
5. POST con cantidad 2: 422 CRT-003, porque el total seria 6 y el stock es 5.
6. GET del carrito: debe seguir con cantidad 4.
7. GET del producto: su stock sigue en 5, porque el carrito no reserva unidades.

Cuerpo del POST, reemplazando el identificador por el Id real:

```json
{
  "productoId": "ID-DEL-PRODUCTO",
  "cantidad": 2
}
```

Cuerpo del PUT:

```json
{
  "cantidad": 4
}
```

El texto ID-DEL-PRODUCTO es un marcador: no enviarlo literalmente, porque no
es un Guid valido.

## 5. Validaciones del carrito

- Cantidad cero, negativa, fraccionaria, nula, texto no numerico o fuera del
  rango de int: 400 CRT-004. Probar POST y PUT.
- Cantidad omitida: 400 CRT-004.
- Producto inexistente en Products: 404 CRT-002. Usar un Guid cuya inexistencia
  se haya comprobado en Productos.
- Otro usuario sin carrito: GET devuelve 404 CRT-001.
- Identificadores mal formados: 400 CRT-004.

Despues de un rechazo, comprobar que el carrito no cambio. Los numeros validos
entre comillas pueden convertirse a numeros; un texto no numerico se rechaza.

## 6. Retirar y vaciar

1. DELETE /api/cart/{userId}/items/{productId}: 204 sin cuerpo.
2. GET del carrito: 200 con items vacio.
3. Agregar nuevamente una unidad del producto: 200.
4. DELETE /api/cart/{userId}: 204 sin cuerpo.
5. GET: 200 con items vacio. El carrito del usuario se conserva.
6. Repetir el vaciado: 204.

## 7. Borrar productos y comprobar Orders

Esta parte requiere Orders real y sus datos de prueba, pendientes de integrar.

- Producto incluido en una orden Pendiente o Confirmada: DELETE devuelve 409
  PRD-004 y GET confirma que el producto sigue existiendo.
- Producto sin referencias activas: DELETE devuelve 204 y el GET siguiente,
  404 PRD-001.
- Si Orders no esta disponible, DELETE devuelve 500 PRD-005 y el producto se
  conserva. Es una respuesta controlada, no permiso para omitir la consulta.

No presentar la integracion real como probada antes de incorporar Orders.

## 8. Salud, logs y dependencias

Abrir en el navegador /health, /health/ready y /health/live de cada puerto.
En el caso sano, las seis respuestas son 200 con JSON Healthy. No borrar
ni renombrar tablas de una base de trabajo para provocar un error.

Al ejecutar un error desde Swagger, copiar el correlationId de la respuesta.
Buscarlo en la consola y en el archivo JSON de logs. Deben poder identificarse
servicio, endpoint, fecha, nivel y codigo de error cuando corresponda. Los
errores de negocio son Warning y los inesperados Error.

Solo en un entorno de prueba, detener Productos y enviar un POST valido a
Carrito: debe devolver 500 CRT-005, no 404 CRT-002. Consultar un carrito existente
no requiere que Productos responda. Volver a iniciar Productos al terminar.

Para comprobar persistencia, guardar los Ids de prueba, detener y volver a
iniciar las APIs con la misma configuracion. Los productos y carritos deben
seguir disponibles.

## Alcance

Anotar el resultado observado, no marcar como probado un caso que no se ejecuto.
Las capturas de Swagger del repo muestran errores reales de recurso inexistente.
El historial de comprobaciones previas figura en PRUEBAS.md de cada servicio.

Este procedimiento es manual. No reproduce las pruebas automatizadas anteriores
de concurrencia, timeout o alteracion de tablas. Esas herramientas de desarrollo
se conservaron fuera del repo; la entrega no incluye scripts PowerShell ni un
servidor simulado. La prueba completa con Users, Orders y Notifications reales
sigue pendiente del trabajo del grupo.
