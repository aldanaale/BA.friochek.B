using BA.Backend.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace BA.Backend.WebAPI.Middleware;

/// <summary>
/// Middleware de Compatibilidad para el Compañero.
/// Si la ruta NO empieza con /api/v1, desempaqueta la propiedad 'data' de ApiResponse 
/// para entregar un JSON plano.
/// </summary>
public class FlatResponseMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public FlatResponseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Solo aplanamos si el cliente lo pide explícitamente vía Header
        // (Esto evita que las nuevas rutas estandarizadas se aplanen por error)
        bool forceFlat = context.Request.Headers.ContainsKey("X-Flat-Response");

        if (!forceFlat)
        {
            await _next(context);
            return;
        }

        // Interceptamos el stream de respuesta
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        // Si la respuesta fue exitosa (200-201) y es JSON, intentamos desempaquetar
        if ((context.Response.StatusCode == 200 || context.Response.StatusCode == 201) && 
            context.Response.ContentType?.Contains("application/json") == true)
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            string responseText = await new StreamReader(responseBody).ReadToEndAsync();

            try {
                // Intentamos deserializar como ApiResponse genérico para extraer 'data'
                using var jsonDoc = JsonDocument.Parse(responseText);
                if (jsonDoc.RootElement.TryGetProperty("data", out var dataProp))
                {
                    // Reescribimos solo el contenido de 'data'
                    string flatJson = dataProp.GetRawText();
                    
                    // Limpiamos el stream y escribimos el nuevo JSON plano
                    context.Response.Body = originalBodyStream;
                    context.Response.ContentLength = System.Text.Encoding.UTF8.GetByteCount(flatJson);
                    await context.Response.WriteAsync(flatJson);
                    return;
                }
            } catch {
                // Si falla el parseo, devolvemos el original
            }
        }

        // Si no se procesó arriba, devolvemos el contenido original
        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBodyStream);
        context.Response.Body = originalBodyStream;
    }
}
