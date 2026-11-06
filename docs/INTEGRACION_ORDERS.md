# Consulta de ordenes antes de borrar productos

Products.API usa GET `/api/orders`, que ya aparece en la consigna. No se agrega
ningun endpoint obligatorio a Orders ni se accede a su base de datos.

La direccion se configura en `Services:Orders` o `Services__Orders`. El valor
local propuesto es `http://localhost:5103/`, a coordinar con Christian.

Products necesita que el listado devuelva el JSON de la consigna, con estado e
items.productoId. Los demas campos pueden estar presentes: se ignoran en esta
consulta. Ejemplo reducido:

```json
[
  {
    "estado": "Pendiente",
    "items": [
      { "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "cantidad": 2, "precioUnitario": 1500 }
    ]
  }
]
```

Si el producto figura en una orden Pendiente o Confirmada, se responde 409
PRD-004 y no se elimina. Enviada, Entregada y Cancelada no bloquean el borrado.
Una lista vacia permite eliminar. Si el producto no existe, se devuelve PRD-001
sin consultar Orders.

Si Orders no responde, devuelve un error o entrega datos invalidos, Products
devuelve 500 PRD-005 y conserva el producto. No se interpreta una falla como
ausencia de ordenes. El timeout es de cinco segundos y se propaga el encabezado
X-Correlation-Id.

Por esto, para probar un DELETE exitoso hay que tener Orders real disponible.
Los scripts de tests usan un servidor simulado, claramente separado de las APIs.
La comprobacion real entre los servicios queda pendiente de la integracion del
grupo. La consulta valida el estado recibido en ese momento; no reserva stock
ni agrega transacciones distribuidas.
