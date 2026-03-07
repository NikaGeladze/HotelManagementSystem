using System.Net;
using HMS.Application.Exceptions;
using HMS.Application.Models;

namespace HMS.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var apiResponse = new CommonResponse<object>();

        switch (ex)
        {
            case NotFoundException:
                apiResponse.Message = ex.Message;
                apiResponse.StatusCode = HttpStatusCode.NotFound;
                apiResponse.IsSuccess = false;
                break;

            case ConflictException:
                apiResponse.Message = ex.Message;
                apiResponse.StatusCode = HttpStatusCode.Conflict;
                apiResponse.IsSuccess = false;
                break;

            case UnauthorizedException:
                apiResponse.Message = ex.Message;
                apiResponse.StatusCode = HttpStatusCode.Unauthorized;
                apiResponse.IsSuccess = false;
                break;

            case ValidationException e:
                apiResponse.Message = ex.Message;
                apiResponse.StatusCode = HttpStatusCode.BadRequest;
                apiResponse.IsSuccess = false;
                apiResponse.Result = e.Errors;
                break;

            case ArgumentException:
                apiResponse.Message = ex.Message;
                apiResponse.StatusCode = HttpStatusCode.BadRequest;
                apiResponse.IsSuccess = false;
                break;

            default:
                apiResponse.Message = $"An unexpected error occurred: {ex.Message}";
                apiResponse.StatusCode = HttpStatusCode.InternalServerError;
                apiResponse.IsSuccess = false;
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = Convert.ToInt32(apiResponse.StatusCode);

        return context.Response.WriteAsJsonAsync(apiResponse);
    }
}