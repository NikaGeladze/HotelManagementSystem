using System.Net;

namespace HMS.Application.Models;

public class CommonResponse<T>
{
    public string Message { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public T? Result { get; set; }
}