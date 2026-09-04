using VContainer;
using VContainer.Unity;
using CarTurretGame.Input;
using CarTurretGame.Gameplay;

namespace CarTurretGame.Core
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<InputService>(Lifetime.Singleton)
                   .As<IInputService>();
                   
            builder.RegisterComponentInHierarchy<CarController>();
            // builder.RegisterComponentInHierarchy<TurretController>();
        }
    }
}