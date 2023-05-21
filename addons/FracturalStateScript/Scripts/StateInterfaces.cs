using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    public interface IState : IAction
    {
        event System.Action Exited;
        void Reset();
    }

    public interface IAction
    {
        GDC.Dictionary NodeVars { get; set; }
        void Play();
    }
}