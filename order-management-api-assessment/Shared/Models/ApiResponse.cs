namespace order_management_api_assessment.Shared.Models;

public record ApiResponse<T>(bool Success, T? Data, string? Message, IEnumerable<string>? Errors = null);

public static class ApiResponse
{
    public static ApiResponse<T> Success<T>(T data, string? message = null) 
        => new(true, data, message);

    public static ApiResponse<object> Success(string message) 
        => new(true, null, message);

    public static ApiResponse<object> Error(string message) 
        => new(false, null, message);

    public static ApiResponse<T> Error<T>(string message) 
        => new(false, default, message);

    public static ApiResponse<object> Error(IEnumerable<string> errors) 
        => new(false, null, null, errors);

    public static ApiResponse<T> Error<T>(IEnumerable<string> errors) 
        => new(false, default, null, errors);
}