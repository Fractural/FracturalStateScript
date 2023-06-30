using Fractural.NodeVars;
using Fractural.StateScript;
using Godot;
using System;

namespace Tests
{
    [CSharpScript]
    [Tool]
    public class MovementState : State2D
    {
        [NodeVar]
        public float Speed
        {
            get => this.PrivateGetNodeVar<float>(nameof(Speed));
            set => PrivateSetNodeVar(nameof(Speed), value);
        }

        [NodeVar(NodeVarOperation.GetPrivateSet)]
        public float GetVelocity
        {
            get => this.PrivateGetNodeVar<float>(nameof(GetVelocity));
            set => PrivateSetNodeVar(nameof(GetVelocity), value);
        }


        [NodeVar(NodeVarOperation.SetPrivateGet)]
        public float SetState
        {
            get => this.PrivateGetNodeVar<float>(nameof(SetState));
            set => PrivateSetNodeVar(nameof(SetState), value);
        }

        private bool _playing = false;
        private float _timer = 0;
        private const float Duration = 3f;

        protected override void _Play()
        {
            GD.Print("Movement state playing!");
            _playing = true;
            _timer = Duration;
        }

        public override void _Process(float delta)
        {
            if (_playing)
            {
                _timer -= delta;
                if (_timer <= 0)
                {
                    _playing = false;
                    InvokeExited();
                }
            }
        }
    }
}