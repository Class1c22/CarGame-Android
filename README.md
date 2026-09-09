# CarGame-Android

Unity developer test assignment: a 2.5D/3D arcade game for Android where a turret-mounted car automatically drives forward along a track, dodges/takes damage from enemies, and shoots back, until it either reaches the finish line or runs out of health.

## About the game

- The player controls everything with a single action — tapping the screen: the first tap starts the car moving; subsequent taps (after a loss) restart the level.
- The car drives forward automatically, with randomized side-to-side drift along the road (so the movement isn't monotonous).
- The car has a turret that shoots at enemies, and a health (HP) system.
- On taking damage, the car briefly flashes white, and once HP drops below a threshold, a damage VFX (smoke/sparks) turns on.
- The level ends either in a win (reached the end of the level) or a loss (HP depleted), with a corresponding UI screen ("Level Failed" with a Retry button).

## Tech stack

- **Unity** 6000.3.22f1 (LTS branch 6000.0.x / 6000.3.x)
- **URP** (Universal Render Pipeline, Universal 3D template)
- **VContainer** — Dependency Injection
- **TextMeshPro** — all UI text
- Target platform — **Android**

## Architecture

The project is built around DI (VContainer): dependencies like `IInputService` are injected into components via `[Inject]` rather than looked up through `FindObjectOfType`/singletons.

Key scripts:

| Script | Responsibility |
|---|---|
| `CarController` | Forward movement, side drift, game state (`WaitingForTap` → `Moving` → `Finished`), health, win/lose, level restart |
| `CarDamageEffects` | Toggles the damage particle effect on/off based on the car's HP percentage |
| `CarDamageFlash` | Brief white material flash on the car when it takes damage |
| `DistanceProgressUI` | Level-progress UI bar (fill + car icon + score text riding the fill edge) |
| `LetterPopAnimation` | Sequential letter scale-up/scale-down effect for TMP text (e.g. the "Level Failed" screen) |

Enemy characters are hand-animated (Idle, Run, TakeDamage) in Blender; death is implemented not as a skeletal animation but as a physics-driven shatter effect (Blender Cell Fracture + Unity's `AddExplosionForce`).

## Repository structure

```
Assets/            — all game assets, scripts, scenes, prefabs
Packages/          — Unity Package Manager dependencies
ProjectSettings/   — Unity project settings
```


## Author

[Class1c22](https://github.com/Class1c22)
