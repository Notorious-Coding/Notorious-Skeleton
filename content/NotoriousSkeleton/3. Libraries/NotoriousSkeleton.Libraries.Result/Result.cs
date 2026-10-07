namespace NotoriousSkeleton.Libraries.Result;

public record Result
{
    public bool IsSuccess { get; init; }
    public List<Error> Errors { get; init; } = [];

    public bool HasErrors => Errors.Count > 0;

    public Result()
    {
        IsSuccess = true;
        Errors = [];
    }

    protected Result(List<Error> errors)
    {
        IsSuccess = false;
        Errors = errors;
    }

    public static Result Success() => new();
    public static Result Failure(Error error) => Failure([error]);

    public static Result Failure(List<Error> errors)
    {
        if (errors == null) throw new ArgumentNullException(nameof(errors));

        return new Result(errors);
    }

    public static implicit operator Result(Error error) => Failure(error);
    public static implicit operator Result(List<Error> errors) => Failure(errors);
}

public record Result<T> : Result
{

    public T? Value { get; init; }

    private Result(T value) : base() => Value = value;
    private Result(List<Error> errors) : base(errors) { }


    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(Error error) => new([error]);
    public static implicit operator Result<T>(List<Error> error) => new(error);

    public Result() : base()
    {
    }
}