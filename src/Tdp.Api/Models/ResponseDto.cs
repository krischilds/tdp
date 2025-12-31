namespace Tdp.Api.Models;

public class ResponseDto<T>
{
    public bool Success { get; set; }
    public int Status { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? TraceId { get; set; }
    public object? Meta { get; set; }

    public static ResponseDto<T> Ok(T data, int status = 200, string? message = null, string? traceId = null, object? meta = null)
        => new() { Success = true, Status = status, Data = data, Message = message, TraceId = traceId, Meta = meta };

    public static ResponseDto<T> Created(T data, string? traceId = null)
        => new() { Success = true, Status = 201, Data = data, TraceId = traceId };
}