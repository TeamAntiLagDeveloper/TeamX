namespace TeamX.Shared.Results;

/// <summary>
/// Representa o resultado de uma operação sem retorno de dados.
/// </summary>
public class Result
{
    public bool Success { get; }
    public string Message { get; }

    public bool IsFailure => !Success;

    protected Result(bool success, string message)
    {
        Success = success;
        Message = message ?? string.Empty;
    }

    public static Result Ok(string message = "Sucesso")
        => new(true, message);

    public static Result Fail(string message)
        => new(false, message);
}

/// <summary>
/// Representa o resultado de uma operação que retorna dados.
/// </summary>
/// <typeparam name="T">Tipo do dado retornado em caso de sucesso.</typeparam>
public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool success, string message, T? data)
        : base(success, message)
    {
        Data = data;
    }

    public static Result<T> Ok(T data, string message = "Sucesso")
        => new(true, message, data);

    public new static Result<T> Fail(string message)
        => new(false, message, default);
}