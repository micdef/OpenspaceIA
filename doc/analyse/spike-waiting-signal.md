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
| Claude waits for a **permission** | **Not verified** | No permission prompt could be triggered in the session used (see below) |
| A permission was **refused** by the user | Yes, after the fact | the tool result carries `toolDenialKind: "user-rejected"` |
| A question was **answered** by the user | Yes, after the fact | the tool result carries `permissionDecision.source: "user_temporary"` |

## 1. The status of a session

Each running session has a file `~/.claude/sessions/<pid>.json`. Its `status` field takes **three** values:

| Value | Meaning (observed) |
|-------|--------------------|
| `busy` | Claude is working (thinking or running a tool) |
| `waiting` | Claude waits for the user: an open question, observed below |
| `idle` | The turn is over (observed on sessions that had finished their turn; not measured during this spike) |

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

**Not verified:** the `status` of a session **while a permission prompt is shown**. The session of this spike runs in
automatic mode: a call that could need a permission (a web page on a new domain) was accepted by the classifier, with no
prompt, and the status stayed `busy`. The decision records above are written only **after** the user decides; they do not
show a pending prompt. Expected (not verified): `waiting`, as for a question.

## 4. Consequences for OpenSpaceIA

- **The waiting signal is the `status` field**, read in the session file. It is cheap (one small file per session) and it is
  the earliest signal. The transcript (open `AskUserQuestion`) confirms it and tells *why* Claude waits.
- **No fallback is needed for questions.** For permissions, a second measurement is planned: the author opens a session in
  the default mode, asks for a command that needs his approval, and waits about 20 seconds before answering, while the
  status of that session is recorded.
- The rest of the decisions (the states that can be detected, what happens if a permission prompt is not seen as
  `waiting`) are taken in `claude-model-decisions` (0.2.0).
