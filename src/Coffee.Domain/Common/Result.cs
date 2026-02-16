namespace Coffee.Domain.Common;

/// <summary>
/// Generic result type for operations that can fail
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    private Result(bool isSuccess, T? value, string? error, string? errorCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result<T> Success(T value) => new(true, value, null, null);
    public static Result<T> Failure(string error, string? code = null) => new(false, default, error, code);

    public Result<TNew> Map<TNew>(Func<T, TNew> mapper) =>
        IsSuccess ? Result<TNew>.Success(mapper(Value!)) : Result<TNew>.Failure(Error!, ErrorCode);

    public async Task<Result<TNew>> MapAsync<TNew>(Func<T, Task<TNew>> mapper) =>
        IsSuccess ? Result<TNew>.Success(await mapper(Value!)) : Result<TNew>.Failure(Error!, ErrorCode);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, string?, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(Error!, ErrorCode);
}

/// <summary>
/// Non-generic result type for operations without return value
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string? ErrorCode { get; }

    private Result(bool isSuccess, string? error, string? errorCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string error, string? code = null) => new(false, error, code);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error, string? code = null) => Result<T>.Failure(error, code);
}
