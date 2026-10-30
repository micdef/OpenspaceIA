# State definitions

Version 0.3.0 (`roadmap.md`). The rules that the core computes, written before the code so that the tests (written first,
by Kenobi) prove exactly these rules. They build on [claude-model-decisions.md](claude-model-decisions.md).

| Item | Value |
|------|-------|
| Status | Validated by Michaël Defraene on 2026-10-30 |
| Values | Every number below is a **default**: the application lets the user change it |

## 1. Who is "an agent"

- An agent is an **agent type**: BB-8, C-3PO, Explore… Each run of a sub-agent is one run of its agent type; the office
  shows one character per agent type, not one per run.
- **The chief**, the main session of Claude Code, is an agent like the others. Each open main session is one chief.

## 2. The state at a given moment

| State | The chief (a main session) | Another agent |
|-------|----------------------------|---------------|
| **Working** | `status` is `busy` | At least one of its runs is in progress |
| **Waiting for you** | `status` is `waiting`; the reason is the tool call left open: `AskUserQuestion` → a question, any other tool → a permission | — (sub-agent runs do not wait for the user: they run in the background) |
| **Idle** | `status` is `idle` | No run in progress |

A session that is closed (its file is gone, its process is dead, or its process start differs from `procStart`) is not
shown at all.

## 3. Overloaded

The author's definition: *the agent chains several projects and never stops*. An agent is **overloaded** when all three
conditions are true over a sliding window:

| Condition | Default |
|-----------|---------|
| The window looked at | the last **2 hours** |
| It worked on at least | **2 different projects** |
| Its longest break (time between two periods of work) | **less than 10 minutes** |

Why three conditions:

- An agent that works two hours in a row on **one** project is busy, but not overloaded: it is not spread over projects.
- An agent that works on two projects with an hour of break between them is not overloaded: it stops.

"Overloaded" is shown **on top of** the state of the moment: an overloaded agent at its desk has sweat and paper stacks;
an overloaded agent who waits for you is still at the coffee machine.

## 4. Under-used

The author's definition: *always playing or at the coffee machine*. An agent is **under-used** when it was **idle at
least 90 % of the time over the last 7 days**.

- Only **idle** counts. Waiting for you is not under-use: it means the user slows the agent down, which the History screen
  shows apart ("you slowed them down").
- An agent that was never called in the 7 days is under-used.

## 5. Periods and time

- A **period of work** of the chief is the time while its `status` is `busy`; of another agent, the time between the first
  and the last record of each run.
- "Now" is a **parameter** of every rule: the core never reads the clock itself, so that every rule is tested with a fixed time.
- All times are compared in UTC; only the screens show local time.

## 6. Settings

| Setting | Default | Used by |
|---------|---------|---------|
| Window of "overloaded" | 2 hours | §3 |
| Minimum number of projects | 2 | §3 |
| Break that ends "never stops" | 10 minutes | §3 |
| Window of "under-used" | 7 days | §4 |
| Idle share for "under-used" | 90 % | §4 |

The settings are read from the configuration of the application; their file and their screen come with the desktop
application (0.5.0). Until then, the core receives them as an input of its rules.
