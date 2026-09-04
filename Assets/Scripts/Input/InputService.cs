using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace CarTurretGame.Input
{
    public class InputService : IInputService, ITickable
    {
        public event Action Tapped;
        public event Action<Vector2> DragDelta;

        private bool _isDragging;
        private Vector2 _lastPosition;

        public void Tick()
        {
            var pointer = Pointer.current;
            if (pointer == null) return;

            if (pointer.press.wasPressedThisFrame)
            {
                _isDragging = true;
                _lastPosition = pointer.position.ReadValue();
                Tapped?.Invoke();
            }
            else if (pointer.press.wasReleasedThisFrame)
            {
                _isDragging = false;
            }
            else if (_isDragging && pointer.press.isPressed)
            {
                var current = pointer.position.ReadValue();
                var delta = current - _lastPosition;

                if (delta.sqrMagnitude > 0.0001f)
                {
                    DragDelta?.Invoke(delta);
                    _lastPosition = current;
                }
            }
        }
    }
}