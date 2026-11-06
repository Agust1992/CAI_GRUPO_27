# Productos: operaciones y pruebas

Este avance separa las rutas en `Controllers/ProductosController.cs` y las
reglas en `Services/ProductoService.cs`. El repositorio conserva SQLite y
Dapper de la MiniApi. Todavia hay que confirmar con la catedra la libreria de
persistencia mencionada en la consigna.

## Para probar

Desde la raiz, con el SDK de .NET 10 activo:

```powershell
dotnet build ECommerce.sln
dotnet run --project src/Products.API --launch-profile http
```

Swagger queda en `http://localhost:5208/swagger`. Las solicitudes de ejemplo
estan en `Products.API.http`. Reemplazar `ProductId` por el id de un alta.
Las pruebas sobre una base propia agregan o modifican datos: usar productos de
prueba. Las comprobaciones automaticas de este avance usaron una base temporal
separada, sin tocar `products.db`.

## Comportamiento de este avance

- POST `/api/products`: 201 con producto y encabezado Location.
- GET `/api/products`: 200 con todos los productos. `nombre` busca una parte
  del nombre y `categoria` compara la categoria completa, sin distinguir
  mayusculas de minusculas. Se pueden combinar los dos filtros.
- GET `/api/products/{id}`: 200 o 404 con PRD-001.
- PUT `/api/products/{id}`: 200 con el producto actualizado o 404 con PRD-001.
- DELETE `/api/products/{id}`: 204 o 404 con PRD-001.
- Datos invalidos, JSON mal formado o campos obligatorios ausentes: 400 con
  PRD-002. El stock debe estar presente; cero es valido.
- Nombre y categoria iguales a otro producto: 409 con PRD-003. La comparacion
  de duplicados es exacta. Una modificacion tampoco puede generar un duplicado;
  actualizar el propio producto sin cambiar nombre y categoria sigue permitido.
- Fallo inesperado: 500 con PRD-005, sin SQL, excepciones ni stack traces en la
  respuesta. Se registra el detalle internamente.

Los errores incluyen ProblemDetails, errorCode, errorMessage y correlationId.
Las excepciones y sus handlers siguen el ejemplo del apendice B de la consigna.

## Casos comprobados

Alta, consulta, filtros solos y combinados, modificacion, borrado y recurso
inexistente. Nombre de 100/101 caracteres, descripcion de 500/501, campos
nulos o ausentes, precio cero o negativo y stock cero o negativo. Los pedidos
invalidos no modificaron datos. Se verifico que una modificacion preserve la
fecha de creacion y que los productos sigan guardados al reiniciar la API.

En seis altas simultaneas iguales, una devolvio 201 y cinco devolvieron 409.
Se simulo un fallo de persistencia en otra base temporal para comprobar PRD-005.
Estas son comprobaciones HTTP contra la API, no un proyecto de tests unitarios.
La ultima ejecucion completo 77 comprobaciones HTTP. La solucion compilo sin
errores, con las dos advertencias de dependencias indicadas abajo.

## Pendientes antes de integrar con los otros servicios

- IDs Guid y campos JSON en espanol, incluidas las fechas del contrato final.
  Este avance aun conserva los IDs numericos y campos ingleses de la MiniApi.
- Borrado condicionado a ordenes Pendiente o Confirmada: PRD-004 aun no esta
  implementado. No tomar el DELETE actual como la regla final.
- Acordar con Christian el formato comun y la propagacion de X-Correlation-Id.
  El campo actual usa TraceIdentifier, pero no hay trazabilidad entre servicios.
- Completar Swagger con XML comments y ejemplos, logging JSON y los tres
  controles de salud, siguiendo el reparto del equipo.
- Revisar las advertencias de KubernetesClient y SQLitePCLRaw.lib.e_sqlite3.

La ruta anterior `/products` se reemplazo por `/api/products`; no hay dos juegos
de endpoints en paralelo. No se modificaron MiniApi, Orders, Users, Cart ni
Notifications.
