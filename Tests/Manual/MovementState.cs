using Fractural.NodeVars;
using Fractural.StateScript;
using Godot;
using System;

namespace Tests
{
    [Tool]
    public class MovementState : State2D
    {
        [NodeVar]
        public float Speed
        {
            get => this.GetNodeVar<float>(nameof(Speed));
            set => SetNodeVar(nameof(Speed), value);
        }

        [NodeVar(NodeVarOperation.Get)]
        public float GetVelocity
        {
            get => this.GetNodeVar<float>(nameof(GetVelocity));
            set => SetNodeVar(nameof(GetVelocity), value);
        }


        [NodeVar(NodeVarOperation.Set)]
        public float SetState
        {
            get => this.GetNodeVar<float>(nameof(SetState));
            set => SetNodeVar(nameof(SetState), value);
        }

        private bool _playing = false;
        private float _timer = 0;
        private const float Duration = 3f;

        public override void Play()
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