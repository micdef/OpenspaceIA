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
| **Waiting for you** | `status` is `waiting`; the reason is the tool call left open: `AskUserQuestion` or `ExitPlanMode` (the approval of a plan) → a question, any other tool → a permission, no open call → a question (decisions of Michaël, 2026-10-31) | — (sub-agent runs do not wait for the user: they run in the background) |
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
- A **period of waiting** exists only for the chief: the time while it waits for you.
- The rules receive the periods of an agent, already cut, with their project; they do not know where the periods come from.
  The reader (0.4.0) builds them from the transcripts, with no storage of our own (decision 6 of
  [claude-model-decisions.md](claude-model-decisions.md)). Validated by Michaël Defraene on 2026-10-30.
- Periods that overlap (runs of the same agent type in parallel) are merged: a time is never counted twice.
- "Now" is a **parameter** of every rule: the core never reads the clock itself, so that every rule is tested with a fixed time.
- All times are compared in UTC; only the screens show local time.

## 6. How the thresholds are read

Validated by Michaël Defraene on 2026-10-30.

| Rule | Reading |
|------|---------|
| Overloaded: projects | "at least 2": 2 projects is enough |
| Overloaded: break | "less than 10 minutes": a break of exactly 10 minutes ends "never stops" |
| Overloaded: what is a break | Any time of the window without a period of work: between two periods, **and also** from the start of the window to the first period, and from the last period to now. A period of waiting is a break |
| Under-used | "at least 90 % idle": exactly 90 % is under-used. Idle is the time of the window without a period of work **or** of waiting |
| Both | Only the part of a period inside the window counts; a period that started before the window is cut at its start |
| Both | A period counts only when its part inside the window lasts more than zero: a period that only touches the window (it ends exactly at its start, or starts exactly now) or that lasts zero is ignored, so it adds no project |

## 7. Settings

| Setting | Default | Used by |
|---------|---------|---------|
| Window of "overloaded" | 2 hours | §3 |
| Minimum number of projects | 2 | §3 |
| Break that ends "never stops" | 10 minutes | §3 |
| Window of "under-used" | 7 days | §4 |
| Idle share for "under-used" | 90 % | §4 |

The settings are read from the configuration of the application; their file and their screen come with the desktop
application (0.5.0). Until then, the core receives them as an input of its rules.
