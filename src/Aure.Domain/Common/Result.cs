namespace Aure.Domain.Common;

public class Result
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; private set; } = string.Empty;

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);

    public static Result<T> Success<T>(T data) => new(data, true, string.Empty);
    public static Result<T> Failure<T>(string error) => new(default, false, error);
}

public class Result<T> : Result
{
    public T? Data { get; private set; }

    internal Result(T? data, bool isSuccess, string error) : base(isSuccess, error)
    {
        Data = data;
    }

    public static implicit operator Result<T>(T data) => Success(data);
}

public static class ResultExtensions
{
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper)
    {
        return result.IsSuccess && result.Data != null
            ? Result.Success(mapper(result.Data))
            : Result.Failure<TOut>(result.Error);
    }

    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<TOut>> mapper)
    {
        if (result.IsSuccess && result.Data != null)
        {
            var mappedData = await mapper(result.Data);
            return Result.Success(mappedData);
        }
        
        return Result.Failure<TOut>(result.Error);
    }

    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, string error)
    {
        if (result.IsFailure)
            return result;

        return result.Data != null && predicate(result.Data)
            ? result
            : Result.Failure<T>(error);
    }
}