using Fractural.NodeVars;
using Fractural.StateScript;
using Godot;
using System;

namespace Tests
{
    public class MovementState : State2D
    {
        [NodeVar]
        public float Speed
        {
            get => GetDictNodeVar<float>(nameof(Speed));
            set => SetDictNodeVar(nameof(Speed), value);
        }

        [NodeVar(NodeVarOperation.Get)]
        public float GetVelocity
        {
            get => GetDictNodeVar<float>(nameof(GetVelocity));
            set => SetDictNodeVar(nameof(GetVelocity), value);
        }


        [NodeVar(NodeVarOperation.Set)]
        public float SetState
        {
            get => GetDictNodeVar<float>(nameof(SetState));
            set => SetDictNodeVar(nameof(SetState), value);
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