using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Products.API.ExceptionHandlers;

public class ClientErrorFactory : IClientErrorFactory
{
    public IActionResult GetClientError(ActionContext context, IClientErrorActionResult clientError)
    {
        int estado = clientError.StatusCode ?? 400;
        var problema = RespuestaError.CrearErrorHttp(context.HttpContext, estado);
        var respuesta = new ObjectResult(problema) { StatusCode = estado };
        respuesta.ContentTypes.Add("application/problem+json");
        return respuesta;
    }
}
