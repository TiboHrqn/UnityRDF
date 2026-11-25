public interface IAction<TTarget>
{
    bool CheckPreconditions(TTarget target);
    void Execute(TTarget target);
}
