using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Salon.Common;

public class HttpError(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class NotFoundError(string message) : HttpError(StatusCodes.Status404NotFound, message);

public class ConflictError(string message) : HttpError(StatusCodes.Status409Conflict, message);

/// <summary>Writes errors as { statusCode, message }. Validation errors carry a list of messages.</summary>
public class HttpErrorHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not HttpError error)
        {
            return false;
        }
        context.Response.StatusCode = error.StatusCode;
        await context.Response.WriteAsJsonAsync(new { statusCode = error.StatusCode, message = error.Message }, cancellationToken);
        return true;
    }

    public static IActionResult Validation(ActionContext context)
    {
        var messages = context.ModelState
            .SelectMany(entry => entry.Value!.Errors.Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? $"{entry.Key} is invalid" : e.ErrorMessage))
            .ToList();
        return new BadRequestObjectResult(new { statusCode = StatusCodes.Status400BadRequest, message = messages });
    }
}
