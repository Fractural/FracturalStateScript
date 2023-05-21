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
            get => GetNodeVar<float>(nameof(Speed));
            set => SetNodeVar(nameof(Speed), value);
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