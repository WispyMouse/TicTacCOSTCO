# Budgeted cascade CPU

This implementation is based on upstream commit `475c0b61c797f277ef993360b39c5c9d9bee271a` (September 25, 2026). It preserves that revision's completion rule: elimination leaving one survivor takes priority over a full-board draw ([upstream change](https://github.com/WispyMouse/TicTacCOSTCO/commit/475c0b61c797f277ef993360b39c5c9d9bee271a)).

## AI provenance

This contribution was chiefly generated and revised by Perplexity Computer, an AI assistant, at the direction of the submitting account `rmc3`. It is not presented as code personally written by `rmc3`; `Docs/AI-PROVENANCE.md` records the attribution, upstream boundary, submission roles, and verification limits.

## Use in Unity

The game scene's `AIThinkerLoop.BasicAICore` now references `Assets/Scripts/AIs/AICore_Search.asset`. The old AI assets and compatibility entry point remain available; assigning an old core restores its previous algorithm.

1. Open the project in its configured Unity editor and enter through `TitleScene`, which initializes the existing persistent game configuration.
2. On the game scene's `TurnOrderHolder`, set `InitialComputerPlayers` to the zero-based seats that should be CPU-controlled, for example `[1]` for human versus computer. The default empty list preserves the existing all-human seat configuration.
3. Edit `AICore_Search.asset` to select `Easy`, `Normal`, or `Hard` and resource limits. All CPU seats share this asset; there is no new player-facing difficulty menu in this patch.
4. Use the existing player-type toggle if it is exposed by your UI. Initial seats are loaded once in `Awake`; game resets preserve runtime toggles.

The current title-screen player-configuration scaffolding is not redesigned here. Player count and initial dimensions still use the existing persistent configuration; this patch does not claim to finish unrelated UI work.

## Settings

| Setting | Default | Meaning |
|---|---:|---|
| Strength | Normal | Logical-work and rollout preset; not an Elo rating |
| WorkBudgetScale | 1 | Multiplies the strength's size-adjusted work allowance; 0.1–4 |
| MaxTreeNodes | 1,024 | Caps stored MCTS nodes; not an exact managed-heap byte quota |
| MoveTimeLimitMilliseconds | 500 | Wall-clock search ceiling, including frames yielded during search |
| FrameBudgetMilliseconds | 2 | Target work slice, checked between bounded operations |
| ActivationPreference | 0 | Adjusts the move-ordering preference for immediate connections |
| InvestmentWeight | 1 | Adjusts investment features; separate from strength |
| Seed | 12,345 | Combined with the full input-state fingerprint |

Presentation delay remains separate and is controlled by the existing `AIThinkerLoop` timing curves. Each slice can exceed its target by one atomic operation, scheduling/GC delay, or runtime overhead; the implementation does not promise a hard real-time deadline.

Base work allowances are 6,000 / 32,000 / 128,000 units for Easy / Normal / Hard. A single smooth complexity adjustment multiplies these by `clamp(sqrt(emptyCells / 25) * sqrt(activePlayers / 2), 1, 8)`, before the asset's work scale. This expands consideration as the board/player count grows without separate board-specific bots; device deadlines can still truncate it.

The presets also change simulation caps, rollout horizons, initial candidate breadth, and root-choice noise. A completed root scan excludes immediate self-elimination if a surviving move exists; Easy may choose a near-best evaluated alternative but does not deliberately add an obvious-suicide chance. An interrupted root scan guarantees a legal fallback, not optimal survival.

## Architecture and rules boundary

- **CascadePosition:** Detached array-backed snapshot containing owners, accepted cell/direction masks, active players, current player, threshold, phase, and result. Scoring and transitions are differential-tested against the production `GameState`; departed players' marks remain.
- **CascadeSearch:** Resumable reward-vector MCTS, with progressive widening, cascade-aware move ranking, informed rollouts, and bounded heuristic evaluation at rollout cutoff. Each acting player optimizes its own reward rather than treating all opponents as a coalition.
- **Small exact search:** With two survivors and at most eight empty cells, full-width alpha-beta replaces MCTS. Only a completed proof marks `IsProven`; interruption retains the previously legal heuristic fallback.
- **Rewards:** A winner receives 1, other players 0; an unresolved draw distributes 1 equally among the remaining active players. This is a search utility convention, not an added game scoring rule.
- **Search ownership:** Search clones input and settings. It never invokes game callbacks or mutates the live game, and it does not use Unity objects or Unity's global random state.
- **Unity adapter:** `AISearchCore.BeginSearch` provides the resumable path. `DetermineMove` remains a bounded synchronous compatibility API and can block its caller until its budget ends; the game loop does not use that synchronous path for this core.
- **Scheduling:** `AIThinkerLoop` yields between short work slices, cancels on reset/disable/turn changes, and rechecks game identity, state version, player, and human/CPU status before applying a result.
- **Diagnostics:** `LastSearchWorkUnits`, `LastSearchSimulations`, `LastSearchStopReason`, and `LastSearchMaxSliceMilliseconds` are available on the loop for profiling completed searches.

The engine targets the current 3–20 per-axis, 2–6-player game. It also supports rectangular inputs and other connection lengths in the model, but no larger-board mobile support claim is made. Seeded fixed-work replay is tested across different frame-slice sizes in the same runtime; cross-runtime RNG identity is not asserted.

The tree cap bounds retained nodes and their candidate lists; exact search has bounded recursion depth but creates temporary snapshots. This version does not yet have a pooled zero-allocation node/state allocator, a transposition table, tree reuse between turns, or a general iterative-deepening alpha-beta engine for large positions.

## Supporting changes

The data-structures assembly now has no runtime NUnit or Visual Scripting dependency: unused imports were removed, and the nonempty `CellsConnection` invariant uses `ArgumentException`. This makes the production rules core buildable outside Unity without changing legal-play rules.

`GameConductor` has a reset notification and state version, and rejects terminal/occupied placements at the UI mutation boundary. `TurnOrderHolder` avoids duplicate subscriptions when resetting its turn UI and retains names for every seat rather than sizing them only to surviving players.

## Run checks

From the repository root, with the .NET 8 SDK:

```sh
dotnet build Tools/HeadlessTests/CoreCompatibility.csproj -c Release
dotnet run --project Tools/HeadlessTests/HeadlessTests.csproj -c Release -- --workers=0
```

The first command builds the actual rules/search sources as a .NET Standard 2.1 library, with warnings treated as errors. The second runs the original coordinate/scoring tests, new rules/search tests, and console-only adapter/control-flow smoke tests.

Run the exploratory experiments separately:

```sh
dotnet run --project Tools/HeadlessTests/HeadlessTests.csproj -c Release -- \
  --workers=0 --test=BaselineTests.PairedLegacyLeague,BaselineTests.MultiplayerLeague,BaselineTests.DifficultyLeague

dotnet run --project Tools/HeadlessTests/HeadlessTests.csproj -c Release -- \
  --workers=0 --test=TicTacCOSTCO.Tests.CascadeSearchTests.BenchmarkPresetsAndBoardSizes
```

`Assets/Tests/CascadeSearchTests.cs` is also in the Unity editor test assembly. The console-only shims under `Tools/HeadlessTests` compile actual legacy AI and adapter/turn-loop bodies without the Unity runtime; they are not included in Unity builds and do not emulate Unity serialization, frame scheduling, rendering, or its RNG.

## Validation scope and remaining release gates

The regression suite covers hypothetical scoring for all players, complete transitions over seeded 3×3 through 20×20 games, consumed directions, multi-axis scoring, full-board draw versus winner precedence, eliminated-player skipping, legal fallback, input isolation, work/node limits, cancellation, deterministic frame slicing, and exact small-position agreement with an independent exhaustive oracle using the original `GameState`.

Headless control-flow checks cover reset, version changes, human toggles, replacement game objects, disable/unsubscribe, consecutive CPU turns, and legacy-core compatibility. These are useful smoke checks, not a substitute for Unity Play Mode tests.

Before shipping:

- Run Unity compilation and Edit Mode tests, then Play Mode checks through the real title/game scenes.
- Exercise resize/reset, repeated human/CPU toggles, scene unload, pause/resume, and consecutive CPU turns on device.
- Profile sustained Android release/IL2CPP play on representative low-, mid-, and high-tier devices: frame slices, total latency, allocations, deadline-hit frequency, and thermal behavior.
- Expand held-out leagues across board sizes, starts, seats, and opponent mixtures before calling the presets calibrated skill levels.
- Treat any exhausted deadline as reduced delivered search, not completion of the requested logical-work tier.

The implementation is a tested first version, not a certified mobile release. No pplx-reflect execution, model inference, cloud service, or runtime network dependency is part of the CPU.
