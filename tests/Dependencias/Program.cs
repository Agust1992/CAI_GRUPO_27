var app = WebApplication.Create(args);
var estado = new EstadoPrueba();

app.MapGet("/health", () => Results.Ok());
app.MapPost("/control/ordenes", (ControlOrdenes datos) =>
{
    estado.Modo = datos.Modo;
    estado.ProductoId = datos.ProductoId;
    return Results.Ok();
});
app.MapGet("/control/correlacion", () => Results.Ok(new { estado.CorrelationId }));
app.MapPost("/control/productos", (ControlProducto datos) =>
{
    estado.ModoProducto = datos.Modo;
    return Results.Ok();
});
app.MapGet("/api/products/{id}", async Task<IResult> (Guid id, HttpContext context) =>
{
    estado.CorrelationId = context.Request.Headers["X-Correlation-Id"].ToString();
    string modo = estado.ModoProducto;
    if (modo == "NoExiste") { return Results.NotFound(); }
    if (modo == "Error") { return Results.StatusCode(503); }
    if (modo == "Invalido") { return Results.Text("no es JSON", "application/json"); }
    if (modo == "Incompleto") { return Results.Ok(new { id }); }
    if (modo == "OtroId") { return Results.Ok(new { id = Guid.NewGuid(), stock = 5 }); }
    if (modo == "StockNegativo") { return Results.Ok(new { id, stock = -1 }); }
    if (modo == "SinStock") { return Results.Ok(new { id, stock = 0 }); }
    if (modo == "Timeout") { await Task.Delay(7000); }
    return Results.Ok(new { id, stock = 5 });
});
app.MapGet("/api/orders", async Task<IResult> (HttpContext context) =>
{
    estado.CorrelationId = context.Request.Headers["X-Correlation-Id"].ToString();
    string modo = estado.Modo;
    if (modo == "Error") { return Results.StatusCode(503); }
    if (modo == "Invalido") { return Results.Text("no es JSON", "application/json"); }
    if (modo == "Incompleto") { return Results.Ok(new[] { new { estado = "Pendiente" } }); }
    if (modo == "Timeout") { await Task.Delay(7000); }
    List<object> ordenes = new List<object>();
    if (modo != "SinOrdenes" && modo != "Timeout")
    {
        ordenes.Add(new { estado = modo, items = new[] { new { productoId = estado.ProductoId } } });
    }
    return Results.Ok(ordenes);
});
app.Run();

class EstadoPrueba
{
    public string Modo { get; set; } = "SinOrdenes";
    public Guid ProductoId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string ModoProducto { get; set; } = "Normal";
}

class ControlOrdenes
{
    public string Modo { get; set; } = "SinOrdenes";
    public Guid ProductoId { get; set; }
}

class ControlProducto
{
    public string Modo { get; set; } = "Normal";
}
