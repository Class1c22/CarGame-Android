using System;
using UnityEngine;

namespace CarTurretGame.Input
{
    /// <summary>
    /// Єдина точка входу для введення. Геймплейні скрипти (CarController,
    /// TurretController) залежать тільки від цього інтерфейсу, а не від
    /// конкретного Input API — легко підмінити реалізацію або замокати в тестах.
    /// </summary>
    public interface IInputService
    {
        event Action Tapped;
        event Action<Vector2> DragDelta; // накопичений зсув пальця/миші за кадр, для повороту турелі
    }
}