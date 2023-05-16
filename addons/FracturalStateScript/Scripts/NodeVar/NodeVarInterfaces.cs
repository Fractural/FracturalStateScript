namespace Fractural.StateScript
{
    public interface IResetNodeVar
    {
        void Reset();
    }
    public interface IGetNodeVar
    {
        object Value { get; }
    }

    public interface ISetNodeVar
    {
        object Value { set; }
    }
    public interface IValueNodeVar : ISetNodeVar, IGetNodeVar, IResetNodeVar { }
}