namespace TrojesDeMaranon.Application.Common;

public sealed record ApiResponse<T>(bool Success, T? Data, string? Message, IReadOnlyList<string> Errors)
{
    public static ApiResponse<T> Ok(T data, string? message = null) => new(true, data, message, []);
    public static ApiResponse<T> Fail(string error) => new(false, default, null, [error]);
    public static ApiResponse<T> Fail(IEnumerable<string> errors) => new(false, default, null, errors.ToArray());
}
