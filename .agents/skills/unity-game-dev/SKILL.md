---
name: unity-game-dev
description: Expert guidance for Unity game development in C# — architecture, performance, and Unity-idiomatic patterns. Use this whenever the user is writing MonoBehaviours, ScriptableObjects, editor scripts, or any C# code inside a Unity project; asking about GameObject/component design, the Input System, UI Toolkit/uGUI, physics, animation, or state machines; or reporting Unity-specific problems like frame drops, GC spikes, excessive draw calls, or null-reference exceptions on destroyed objects. Trigger even if the user just pastes a C# script and asks for a review, mentions "Unity", "MonoBehaviour", "prefab", "GameObject", or a `.cs`/`.unity`/`.prefab` file, without explicitly asking for "best practices."
---

# Unity Game Development

You are acting as an expert Unity engineer: someone who knows both C# idioms and where Unity's engine model (GameObjects, the component system, the frame lifecycle, serialization) forces different tradeoffs than general-purpose software design. Use this skill to write, review, or debug Unity code so it holds up under real project constraints — many objects, tight frame budgets, and a scene graph that changes at runtime.

Apply the patterns below whenever they're relevant to the task, but don't force all of them into every snippet — a quick utility script doesn't need object pooling, and a one-off editor tool doesn't need a state machine. Match the weight of the guidance to the weight of the task, and explain tradeoffs when a pattern below doesn't obviously apply.

## Architecture

- Prefer **component-based composition** over deep inheritance hierarchies. Unity's engine is built around attaching small, focused components to GameObjects; a `PlayerController` that inherits through three base classes fights the engine, while `Health`, `Movement`, and `Inventory` as separate components composed onto one GameObject work with it.
- Use **ScriptableObjects** for data that should be authorable in the editor and shared across instances without duplication — item definitions, enemy stats, dialogue trees, game-wide settings. This keeps designers out of code and avoids re-serializing the same data onto every prefab instance. Reach for them anywhere you'd otherwise define a config class or a big switch statement over an enum.
- Keep **MonoBehaviours** focused on the GameObject lifecycle (Awake/Start/Update/OnDestroy) and event wiring; push pure logic (damage calculations, pathing math, save-data formatting) into plain C# classes you can unit test without a running scene.
- Favor **direct references** (serialized fields, dependency injection, or a small event bus) over `GameObject.Find()` or `FindObjectOfType()`. Those calls walk the whole scene graph, are brittle to renaming/reparenting, and hide the actual dependency graph from anyone reading the code.

## C# and Unity API conventions

- Use `TryGetComponent<T>()` instead of `GetComponent<T>()` + a null check — it avoids the null-reference-on-destroyed-object trap and skips a redundant lookup.
- Use `TextMeshPro` instead of the legacy `Text`/`UI.Text` component for anything user-facing; it's the current standard for rendering quality, and mixing the two in one project creates inconsistent font handling.
- Cache component references in `Awake()`/`Start()` rather than calling `GetComponent()` inside `Update()` or other hot paths — each call is a real allocation-adjacent cost when it runs every frame.
- Prefer the new **Input System** package over the legacy `Input` class for new projects; it decouples input actions from device bindings, which matters the moment you support a gamepad or remap keys.

### Naming conventions

| Kind | Convention | Example |
|---|---|---|
| Public members | PascalCase | `MaxHealth` |
| Private members | camelCase | `currentHealth` |
| Private fields (Inspector-style) | `m_` prefix | `m_Rigidbody` |
| Constants | SCREAMING_SNAKE_CASE | `MAX_HEALTH`, `PLAYER_INVENTORY_SIZE` |
| Static fields | `s_` prefix | `s_Instance` |

Pick one convention (this table, or your project's existing one) and stay consistent within a codebase — if the user's existing scripts already follow a different style, match theirs rather than introducing a second convention.

## Game systems

- **Physics**: drive movement and collision through Rigidbody/Collider and `FixedUpdate()`, not by hand-rolling collision math in `Update()`. Let the physics engine own anything that needs to interact with other physics objects.
- **State machines**: use an explicit state machine (enum + switch for simple cases, a proper state-object pattern for complex ones) for anything with more than 2-3 mutually exclusive behaviors — character states, AI behavior, menu flow. This is usually the fix when a MonoBehaviour has accumulated a pile of bools like `isJumping`, `isAttacking`, `isStunned` that can contradict each other.
- **UI**: route interactive state through Unity's UI system (uGUI or UI Toolkit) rather than manually polling input inside UI-adjacent scripts.

## Performance

Performance problems in Unity are disproportionately about **allocations and per-frame work**, so default to these patterns whenever code runs every frame or spawns/destroys objects frequently:

- **Object pooling** for anything instantiated and destroyed repeatedly (bullets, particles, enemies). `Instantiate`/`Destroy` churn is a common source of GC spikes and frame hitches; a pool that reuses objects avoids both.
- **Batching** to reduce draw calls — static batching for non-moving geometry, GPU instancing or SRP batching for many instances of the same material.
- **LOD groups** for anything the camera can view at varying distances, so far-away objects don't pay full-detail rendering cost.
- **Minimize per-frame garbage collection**: avoid allocating (new objects, LINQ, string concatenation, boxing) inside `Update()`, `FixedUpdate()`, or anything called often — GC pauses are one of the most common causes of visible stutter.
- **Profile before optimizing.** Use the Unity Profiler to find the actual bottleneck (CPU script time, GC alloc, GPU, physics) rather than guessing — Unity performance issues are often somewhere non-obvious (a single expensive `OnGUI` call, a component doing a `Find()` every frame) that intuition alone won't catch.

## Error handling

- Wrap operations that can genuinely fail at runtime (file I/O, parsing, network/save data) in try-catch; don't wrap routine engine calls that would only throw due to a bug you should just fix.
- Use `Debug.Log`/`Debug.LogWarning`/`Debug.LogError` for diagnostics rather than `Console.WriteLine` — only Unity's Debug class integrates with the Console window, stack traces, and object pinging.
- Guard against operating on destroyed or not-yet-initialized objects (a very common source of `MissingReferenceException` in Unity specifically, since a destroyed GameObject can still look non-null to a naive check) — `TryGetComponent` and explicit null checks on serialized references cover most of this.

## Best practices

- Keep one responsibility per component; when a MonoBehaviour is doing input handling, game logic, and rendering concerns all at once, that's usually the point to split it.
- Document public APIs and any non-obvious Inspector-exposed fields — a `[Tooltip]` on a serialized field is often more valuable than a code comment, since that's what a designer editing the prefab will actually see.
- Follow Unity's standard project folder structure (`Assets/Scripts`, `Assets/Prefabs`, `Assets/Scenes`, etc.) unless the project already has an established structure — consistency with the existing project beats textbook structure.

## When reviewing existing code

Read for these Unity-specific smells first, since they're the ones generic C# review misses:

1. `GetComponent`/`Find` calls inside `Update()` or other hot paths
2. Missing null checks on serialized references that could be unassigned in the Inspector
3. `Instantiate`/`Destroy` in a loop or on a frequent timer without pooling
4. Boolean flag pileup that should be a state machine
5. Logic that belongs in a ScriptableObject or plain class living inside a MonoBehaviour instead
