namespace Expense_Tracker_02.Services;

/// <summary>Simple success/failure result so services can report rule violations to controllers.</summary>
public record ServiceResult(bool Succeeded, string? Error = null)
{
    public static ServiceResult Ok() => new(true);
    public static ServiceResult Fail(string error) => new(false, error);
}
