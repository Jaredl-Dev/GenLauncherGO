# GenLauncherGO.Tests Guidance

- Prefer a small handwritten fake over a substitute when it makes stateful behavior clearer.
- Test observable behavior, safety, compatibility mappings, and important invariants. Do not test auto-properties,
  standard guards, framework behavior, private helpers, or DI descriptors individually.
- Keep headless Avalonia tests semantic: verify compiled AXAML loads and that meaningful user states expose the expected
  content, actions, accessibility, and theme resources. Protect exact appearance with the smallest practical rendered or
  golden-image coverage, not assertions over coordinates, margins, grid positions, control dimensions, template-part
  structure, or internal visual-tree shape.
- Do not use real-time animation midpoint assertions, no-throw framework smoke tests, or one test per obvious property,
  factory, or guard. Test application-owned state transitions and outcomes instead.
- Keep one focused composition test; do not mirror every registration.
- Use isolated temporary directories for file-system tests. Never require a real game installation, live network
  service, or production credential.
- Protect exact remote YAML binding and its single mapping into normalized concepts with representative fixtures.
- Reuse shared builders, fakes, the Avalonia headless UI runner, and canonical authorities instead of copying setup or
  expected constants.
- Structure tests as arrange, act, and assert separated by blank lines. Add phase comments only when a boundary is
  genuinely ambiguous; repeated act phases or branching assertions normally mean the behavior should be split.
- Reach for a shared helper in `Testing/` before writing setup. `GlobalUsings.cs` already imports that namespace, so
  no `using` is needed. Add a helper there only once a second caller exists.

## Naming helpers in `Testing/`

The prefix states what the helper does, so a reader knows from the call site whether it holds state, answers fixed
values, or is there to assert on.

| Prefix | Means |
| --- | --- |
| `Fake` | A hand-written working implementation, simplified but with real behavior and state. |
| `Recording` | Captures the calls a test asserts on, exposed as `List<>` properties. |
| `Stub` | Answers with fixed values and records nothing. |
| `Controllable` | The test decides when the operation completes, usually through a `TaskCompletionSource`. |
| `Test` | Builds inputs — paths, content, view models. Not a test double. |

A helper whose own name says more than the prefix would keeps that name instead: `CompletedGameProcessLaunchOperation`,
`QueueHttpMessageHandler`, `ManualTimeProvider`. Scopes that restore state on dispose end in `Scope`.
