public enum ActionStatus
{
    Success,
    InvalidState,
    MissingRequirement,
    OutOfRange,
    NotImplemented

}

public class ActionResult
{
    public ActionStatus Status { get; }
    public string Message { get; }

    public bool Succeeded => Status == ActionStatus.Success;

    private ActionResult(ActionStatus status, string message)
    {
        Status = status;
        Message = message;
    }

    public static ActionResult Success(string message = "")
    {
        return new ActionResult(ActionStatus.Success, message);
    }

    public static ActionResult Fail(ActionStatus status, string message)
    {
        return new ActionResult(status, message);
    }
}