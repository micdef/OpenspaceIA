# Decisions: from the files of Claude Code to the office

Version 0.2.0 (`roadmap.md`), last part of the spike. It turns the facts of
[spike-claude-files.md](spike-claude-files.md) and [spike-waiting-signal.md](spike-waiting-signal.md) into the rules that
the core (0.3.0) and the reader (0.4.0) will follow.

| Item | Value |
|------|-------|
| Status | Validated by Michaël Defraene on 2026-10-30 |
| Claude Code | 2.1.309 (the format can change with any version: only the reader module knows it) |

## Decisions

| # | Topic | Decision | Based on |
|---|-------|----------|----------|
| 1 | **States of a session** (the chief) | `status` of `sessions/<pid>.json`: `busy` → **working**; `waiting` → **waiting for you** (at the coffee machine, with the "!" bubble); `idle` → **idle** | Measured for a question and for a permission prompt |
| 2 | **Why it waits** | The tool call left open in the transcript while the status is `waiting`: `AskUserQuestion` → "question"; any other tool → "permission". The status and the record are joined without relying on their order | Measured: the order differs between a question and a permission |
| 3 | **A closed session** | The session file is gone, **or** its process is dead, **or** the start time of the process differs from `procStart` (a reused process ID) | `procStart` equals the start of the process, 0 ms difference |
| 4 | **States of a sub-agent run** | Running while `finished` is missing from its `meta.json` and its transcript still grows; done when `finished` is `true` or when its last tool call is `SubagentHandback`; without a `meta.json`, the transcript alone decides | 49 of 138 meta files without `finished`; 56 transcripts without a meta file |
| 5 | **Overloaded** | Computed from the history: the agent chains several projects without a break. The threshold and the time window are fixed in 0.3.0 (`docs/state-definitions`), configurable | Nothing in the files says it directly |
| 6 | **History** | Computed again from the transcripts, with **no storage of our own** | Transcripts are kept 30 days by default (`cleanupPeriodDays`), and the sessions of Claude Desktop are exempt from this limit by default (Claude Code documentation, "Data usage"); the history needs 7 days |
| 7 | **The real model** | `message.model` of the transcript. The `model` of a `meta.json` is only the model asked at launch, and is often missing | Observed |
| 8 | **Task list** | Rebuilt by replaying the `update_plan` calls in order. The tools used by a command-line session are checked before the reader (0.4.0); a session without a known tool shows "no task list" | Only `update_plan` was seen |
| 9 | **A team** | Claude Code has no idea of a team: a team is **a file of our own, outside `~/.claude`**. The assignment to a project uses the path of the project (`cwd`). To be confirmed by the threat analysis of writing (0.12.0) | No team found in `~/.claude` |
| 10 | **Context of a session** | Shown as a percentage: the tokens of the last request (`input_tokens` + `cache_read_input_tokens` + `cache_creation_input_tokens` of `message.usage`) divided by the context size of the model (200 000 by default, 1 000 000 with an extended context) | Observed in the transcripts; sizes from the Claude Code documentation ("Customize your status line") |

## Not in this version

**The usage limits of the plan** (5-hour window and weekly limit, as a percentage left) are wanted by the author, who has a
Max plan. They are **not in the files** of `~/.claude`: Claude Code gives them to a status line command
(`rate_limits.five_hour` and `rate_limits.seven_day`, `used_percentage` and `resets_at`, Pro and Max plans only), and the
Claude Desktop application shows them. Where to read them from is not verified (does a Desktop session run a status line?
writing a status line in `settings.json` would also widen what the application writes). This feature gets its own place in
the roadmap, starting with a spike.

## What the reader must never do

- Read `.credentials.json`, the `sessions/*.key` files, `backups/`, `telemetry/`, `uploads/` or `~/.claude.json`.
- Show or keep the text of a transcript other than what a screen needs (task card, tool name), always as plain text, cut
  to a maximum length.
- Write anything in `~/.claude` (writing agents comes with 0.12.0, after its own threat analysis).
