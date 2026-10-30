# Spike: the "waiting for you" signal

Version 0.2.0 (`roadmap.md`). Question: can the application tell, from the files of Claude Code only, that an agent
**waits for the user** (a question, a permission), so that the office can show it at the coffee machine with a "!" bubble?

| Item | Value |
|------|-------|
| Date | 2026-10-30 |
| Claude Code | 2.1.309 (desktop session, `kind: interactive`); CLI 2.1.312 installed |
| System | Windows 11 |
| Method | Observation of a real session, then a live measurement (below). No transcript text is copied here: the examples are invented. |

## Result

| Situation | Detected? | How |
|-----------|-----------|-----|
| **Claude asks the user a question** | **Yes, verified** | `status` of `sessions/<pid>.json` becomes **`waiting`** for as long as the question is open, then goes back to `busy` |
| Claude waits for a **permission** | **Yes, verified** (second measurement) | `status` becomes **`waiting`** while the permission prompt is shown; the tool call stays open in the transcript |
| A permission was **refused** by the user | Yes, after the fact | the tool result carries `toolDenialKind: "user-rejected"` |
| A question was **answered** by the user | Yes, after the fact | the tool result carries `permissionDecision.source: "user_temporary"` |

## 1. The status of a session

Each running session has a file `~/.claude/sessions/<pid>.json`. Its `status` field takes **three** values:

| Value | Meaning (observed) |
|-------|--------------------|
| `busy` | Claude is working (thinking or running a tool) |
| `waiting` | Claude waits for the user: an open question, observed below |
| `idle` | The turn is over (measured in the second measurement below) |

The file also holds `statusUpdatedAt`, the time of the last change of `status`.

## 2. Live measurement of a question

A throwaway script (not kept in the repository) read, every second for 200 seconds, the `status` of the session and the
last records of its transcript (record type and tool name only). During that time, the session asked the user a test
question with the `AskUserQuestion` tool, and the user waited about 20 seconds before answering.

| Time | `status` | Last record of the transcript | Open tool call |
|------|----------|-------------------------------|----------------|
| 14:04:27 | `busy` | user | — |
| 14:04:31 | **`waiting`** | user | — |
| 14:04:33 | **`waiting`** | assistant, `AskUserQuestion` | `AskUserQuestion` |
| 14:05:40 | `busy` | user (the answer) | — |

What it shows:

- `status` switches to `waiting` **about two seconds before** the question appears in the transcript, and stays `waiting`
  until the answer: the session file alone is enough, and it is the earliest signal.
- In the transcript, an open question is an `assistant` record with a `tool_use` of `AskUserQuestion` whose matching
  `tool_result` has not been written yet. This confirms the status, but it comes later.

## 3. Permissions

Every tool result of the transcript (a `user` record) carries a `permissionDecision` object:

```json
{ "decision": "accept", "source": "config", "reasonType": "classifier", "ruleSource": "" }
```

Values seen in one real session (about 1 290 tool results):

| `source` | `reasonType` | Meaning |
|----------|--------------|---------|
| `config` | `classifier`, `rule`, `mode`, `other` | accepted without asking the user |
| `user_permanent` | `rule` | accepted by a rule that the user saved |
| `user_temporary` | — | **decided by the user** (here: all of them are answers to `AskUserQuestion`) |
| `config` | `safetyCheck` | refused by a safety check (`decision: "reject"`) |

A refused tool call also carries `toolDenialKind`: `"user-rejected"` (refused by the user) or `"permission-rule"` (refused by a rule).

The decision records above are written only **after** the user decides; they do not show a pending prompt. In the first
session (automatic mode), no prompt could be triggered: a call to a new web domain was accepted by the classifier and the
status stayed `busy`. Hence the second measurement.

### Second measurement: a permission prompt

The author opened a second session in the normal mode (his rules allow every shell command and the files of the project
and of `~/.claude`, so the session was asked to read a file outside its folder) and waited before answering. The status
of that session was recorded every second.

| Time | What happens | `status` | Transcript |
|------|--------------|----------|------------|
| 14:14:43 | The session asks the user a question | **`waiting`** | `tool_use` `AskUserQuestion`, open |
| 14:15:57 | The user answers | `busy` | `tool_result`, `source: "user_temporary"`, `grantScope: "once"` |
| 14:16:00 | The session wants to read a file outside its folder | `busy` | `tool_use` `Read`, open |
| 14:16:03 | **The permission prompt waits for the user** | **`waiting`** | still open |
| 14:17:02 | The user accepts | `busy` (14:17:04) | `tool_result`, `source: "user_temporary"`, `grantScope: "once"` |
| 14:17:11 | End of the turn | **`idle`** | — |

What it shows:

- **`waiting` covers both cases**: a question and a permission prompt.
- **The reason is the tool call left open** while the status is `waiting`: `AskUserQuestion` is a question, any other tool
  is a permission prompt.
- For a permission, the tool call reaches the transcript **before** the status changes (about 3 seconds); for a question,
  the order was not the same in the two measurements (the status first, then the record, or both together). The
  application must therefore join the two without relying on their order.

## 4. Consequences for OpenSpaceIA

- **The waiting signal is the `status` field**, read in the session file. It is cheap (one small file per session). The
  transcript tells *why* Claude waits: the tool call left open.
- **No fallback is needed**: questions and permission prompts are both seen as `waiting`.
- The decisions that follow are written in [claude-model-decisions.md](claude-model-decisions.md).
