using Fractural.Utils;
using Godot;
using GodotRollbackNetcode;
using System.Linq;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    public abstract class Player2D : Node2D, INetworkProcess, IGetLocalInput, INetworkPostProcess
    {
        // TODO MAYBE: Maybe consolidate saving into a single Player2D call?
        public delegate void OnNetworkProcessDelegate(GDC.Dictionary input);
        public GDC.Dictionary Input { get; private set; }

        private NodePath[] _statePaths;
        public IState[] States { get; private set; }

        public void _NetworkPreprocess(GDC.Dictionary input)
        {
            Input = input;
            foreach (var state in States)
                state.StatePreProcess();
        }

        public void _NetworkProcess(GDC.Dictionary input)
        {
            Input = input;
            foreach (var state in States)
                state.StateProcess();
        }

        public void _NetworkPostprocess(GDC.Dictionary input)
        {
            Input = input;
            foreach (var state in States)
                state.StatePostProcess();
        }

        public override void _Ready()
        {
            States = _statePaths.Select(x => GetNode<IState>(x)).ToArray();
        }

        public abstract GDC.Dictionary _GetLocalInput();
    }
}