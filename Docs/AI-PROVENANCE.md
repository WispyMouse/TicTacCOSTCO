# Provenance of the budgeted cascade CPU contribution

The new CPU implementation, integration changes, tests, and accompanying documentation in this contribution were **chiefly generated, revised, and tested by Perplexity Computer, an AI assistant**, on September 25, 2026. The session identified its orchestrator model as **GPT 6 Astra**.

The GitHub account `rmc3` requested the work, provided goals and constraints, and authorized its submission. **This contribution is not presented as code personally written by `rmc3`.** Human direction and permission to submit should not be interpreted as an assertion of independent human code review.

## Scope and existing work

This contribution builds on the existing TicTacCOSTCO game at [upstream commit 475c0b61c797f277ef993360b39c5c9d9bee271a](https://github.com/WispyMouse/TicTacCOSTCO/commit/475c0b61c797f277ef993360b39c5c9d9bee271a). The game, its original rules implementation, existing AI algorithms, and pre-existing tests are upstream work, not work originated by the assistant or by the submitting account.

The assistant chiefly authored:

- `Assets/Scripts/AIs/Search/`: the detached state, search settings, MCTS, and small-position exact solver.
- `Assets/Scripts/AIs/AISearchCore.cs` and the new search asset/metadata.
- `Assets/Tests/CascadeSearchTests.cs` and the headless harness under `Tools/HeadlessTests/`.
- The CPU documentation and validation report under `Docs/`.
- The changes to the existing AI turn loop, game/turn integration, scene asset selection, assembly references, and runtime dependency cleanup shown in the contribution's diff.

Headers on newly authored C# files point to this record. Existing upstream files retain their history; modifying those files does not imply authorship of their unchanged contents.

## Git and submission attribution

The implementation commit uses the Git author name `Perplexity Computer`, with an empty author email rather than an invented bot address or a human author's address. The authenticated submitting account remains the Git committer and GitHub pull-request submitter because it is the account through which the assistant was authorized to publish.

The committer and pull-request submitter identify the submission channel, not the chief source of the code. No verified GitHub bot identity, provider endorsement, signature, copyright ownership, or new license grant is claimed by this attribution.

## How the work was produced

The assistant inspected the upstream rules and existing CPU implementations, implemented the changes directly, ran command-line builds and tests, and prepared the patch and reports. No part of this implementation was delegated through pplx-reflect; the game CPU itself does not use an LLM or a runtime cloud service.

The tests, test harness, benchmark setup, and explanatory reports were also chiefly authored by the assistant. Their execution is recorded verification evidence, but neither their authorship nor a passing result should be described as an independent human audit.

## Verification boundaries

The reported checks comprise a .NET Standard 2.1 compatibility build, 60 passing headless regression tests, four separately executed exploratory experiments, and a clean-patch application followed by another passing regression run. The validation report describes the specific results and their limitations.

Actual Unity editor import/compilation, Unity Play Mode execution, Android builds, device profiling, and sustained mobile performance testing have **not** been performed for this contribution. Some console integration checks use explicitly labeled Unity API shims; those do not reproduce or verify the actual Unity engine.

Review and device validation remain necessary before treating this as a release-ready implementation. See `Docs/CPU.md` for usage and remaining gates, and `Docs/CPU-validation-20260925.md` for the initial test and exploratory results.
