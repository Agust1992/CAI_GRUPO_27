# Controles de salud

Ejemplo preparado en Products.API. Cada integrante debe adaptarlo a sus
servicios, como acordamos en el reparto. No es necesario copiar la base de
Productos a las otras APIs.

## Que responde cada ruta

- `/health`: estado general. En Productos ejecuta los controles de la API y
  de SQLite. Devuelve JSON con el estado y el resultado de cada control.
- `/health/live`: comprueba que el proceso responda. Solo ejecuta ApiStatusCheck;
  no consulta bases de datos ni otros servicios.
- `/health/ready`: comprueba la dependencia necesaria para atender solicitudes.
  En Productos verifica que SQLite se pueda abrir y que exista la estructura
  de la tabla de productos. No agrega ni modifica registros.

La MiniApi original conserva su panel de referencia. Products.API no monta
`/health-ui`: para este TP se mantienen los tres endpoints JSON obligatorios,
sin el recolector y las dependencias del panel opcional.

Healthy devuelve 200. Unhealthy devuelve 503. Si un control devuelve Degraded,
el estado general lo conserva y readiness devuelve 503: el servicio no debe
anunciar que esta listo cuando sus dependencias necesarias estan degradadas.

Una API puede estar viva y no estar lista. Por ejemplo, si falta la tabla de
productos, live sigue Healthy, pero ready y el estado general son Unhealthy.

## Archivos del ejemplo

`Extensions/ServicesExtensions.cs` registra los controles con etiquetas:

```csharp
services.AddHealthChecks()
    .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database", "ready"],
        timeout: TimeSpan.FromSeconds(5))
    .AddCheck<ApiStatusCheck>("api-status", tags: ["api", "live"]);
```

`Extensions/MiddlewareExtensions.cs` publica las tres rutas. El filtro de cada
ruta selecciona las etiquetas ready o live. Todas usan el escritor JSON de
HealthChecks UI que ya estaba en la base de la catedra.

`HealthChecks/SqliteHealthCheck.cs` abre la base existente y verifica sus
columnas, sin leer productos. No alcanza con SELECT 1: una base vacia tambien
puede responder esa consulta. El control no crea una base nueva si falta el
archivo. Tampoco devuelve al cliente la excepcion ni rutas internas; el detalle
queda en los logs del servidor.

## Como adaptarlo

1. Cambiar el nombre de servicio en ApiStatusCheck.
2. Mantener live independiente de bases de datos o llamadas HTTP externas.
3. Adaptar el control ready a la persistencia real del servicio. La consulta
   de Productos no sirve para comprobar Users u Orders.
4. Registrar las etiquetas y publicar las tres rutas con respuestas JSON.
5. Probar el caso sano, una dependencia no disponible y su recuperacion.

No hay que consultar todas las APIs del grupo desde todos los controles de
salud. Cada servicio debe verificar solo las dependencias necesarias para su
funcionamiento. Si cambia la persistencia por la libreria de la catedra, tambien
hay que adaptar ese control.

## Prueba manual

Con Products.API ejecutandose en su puerto actual:

```powershell
Invoke-RestMethod http://localhost:5208/health
Invoke-RestMethod http://localhost:5208/health/ready
Invoke-RestMethod http://localhost:5208/health/live
```

En el caso sano, las tres devuelven Healthy. Para probar fallas usar una base
de prueba separada, nunca borrar o renombrar tablas de una base de trabajo.
Las comprobaciones automaticas de este avance simulan que falta la tabla en
una base temporal, verifican 503 en ready y 200 en live, y restauran la tabla
para comprobar que ready vuelva a Healthy sin reiniciar la API.
