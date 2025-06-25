using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using order_management_api_assessment.Shared.Models;

namespace order_management_api_assessment.Shared.Behaviors;

public class ExceptionHandlingPipelineBehavior<TRequest, TResponse>(ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : class
{
    private readonly ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception ex)
        {
            return ex switch
            {
                ValidationException validationEx => LogAndCreateValidationErrorResponse(validationEx),
                
                ArgumentException => LogAndCreateErrorResponse(ex, "Invalid argument", 
                    "Invalid data provided. Please check your input and try again."),
                
                InvalidOperationException => LogAndCreateErrorResponse(ex, "Invalid operation", 
                    ex.Message.Contains("transition") 
                        ? "Invalid status transition. Please check the current order status and try again."
                        : "Unable to process the request at this time. Please try again."),
                
                DbUpdateException => LogAndCreateErrorResponse(ex, "Database error", 
                    "Unable to save changes. Please try again later."),
                
                _ => LogAndCreateErrorResponse(ex, "Unexpected error", 
                    "An unexpected error occurred. Please try again later.")
            };
        }
    }   
    
    private TResponse LogAndCreateErrorResponse(Exception ex, string errorType, string message)
    {
        if (ex is ArgumentException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "{ErrorType} in request {RequestType}: {Message}",
                errorType, typeof(TRequest).Name, ex.Message);
        }
        else
        {
            _logger.LogError(ex, "{ErrorType} occurred while processing request {RequestType}",
                errorType, typeof(TRequest).Name);
        }

        // Handle the (bool Success, string Message) response pattern
        if (typeof(TResponse) == typeof((bool Success, string Message)))
        {
            return (TResponse)(object)(false, message);
        }

        // Handle ApiResponse<object> pattern
        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(ApiResponse<>))
        {
            var genericArgType = typeof(TResponse).GetGenericArguments()[0];
            var errorMethod = typeof(ApiResponse).GetMethods()
                .Where(m => m.Name == nameof(ApiResponse.Error) && m.IsGenericMethod)
                .First()
                .MakeGenericMethod(genericArgType);
            var errorResponse = errorMethod.Invoke(null, new object[] { message });
            return (TResponse)errorResponse!;
        }

        throw new InvalidOperationException($"Unable to create error response for type {typeof(TResponse)}");
    }

    private TResponse LogAndCreateValidationErrorResponse(ValidationException validationEx)
    {
        _logger.LogWarning(validationEx, "Validation failed for request {RequestType}: {Errors}",
            typeof(TRequest).Name, string.Join(", ", validationEx.Errors.Select(e => e.ErrorMessage)));

        var errorMessages = validationEx.Errors.Select(e => e.ErrorMessage).ToList();

        // Handle ApiResponse<object> pattern with validation errors
        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(ApiResponse<>))
        {
            var genericArgType = typeof(TResponse).GetGenericArguments()[0];
            var errorMethod = typeof(ApiResponse).GetMethods()
                .Where(m => m.Name == nameof(ApiResponse.Error) && m.IsGenericMethod && 
                           m.GetParameters().Length == 1 && 
                           m.GetParameters()[0].ParameterType == typeof(IEnumerable<string>))
                .First()
                .MakeGenericMethod(genericArgType);
            var errorResponse = errorMethod.Invoke(null, new object[] { errorMessages });
            return (TResponse)errorResponse!;
        }

        throw new InvalidOperationException($"Unable to create validation error response for type {typeof(TResponse)}");
    }
}
