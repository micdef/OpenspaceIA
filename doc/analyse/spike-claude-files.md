# Spike: the files of Claude Code

Version 0.2.0 (`roadmap.md`). Question: what does `~/.claude` hold that OpenSpaceIA can read (read only) to show who
works on what, and how big is it?

| Item | Value |
|------|-------|
| Date | 2026-10-30 |
| Claude Code | 2.1.309 (desktop sessions); CLI 2.1.312 installed |
| System | Windows 11 |
| Method | Inventory of the **structure** of a real `~/.claude` by Probe (`Explore`), key facts checked again by the main developer. **No content is copied**: names, keys, value types and small closed sets of values only; the examples are invented. |

The format is internal to Claude Code and not documented: everything below can change with a new version. It is the reason
why one reader module reads it and nothing else does (`roadmap.md`, architecture summary).

## 1. What the application reads, and what it never reads

| Path | Use for OpenSpaceIA |
|------|---------------------|
| `sessions/<pid>.json` | **Who is running, and in which state** (section 2) |
| `projects/<encoded cwd>/` | Transcripts of the main sessions and of the sub-agents (section 3) |
| `agents/*.md` | Definitions of the user agents (section 4) |
| `projects/<encoded cwd>/memory/` | Not needed: never read |

**Never read**, not even listed by content: `.credentials.json`, every `sessions/<pid>.<hex>.key` file, `backups/`,
`telemetry/`, `uploads/`, and `~/.claude.json` (outside the folder). They hold secrets, account data or user files, and
the screens need none of them.

## 2. `sessions/`: the live sessions

One `<pid>.json` per running Claude Code process (the file name is its process ID), next to a `<pid>.<64 hex>.key` file
(never read).

| Key | Type | Use |
|-----|------|-----|
| `pid` | number | Process ID, same as the file name |
| `sessionId` | UUID | Name of the transcript `projects/<encoded cwd>/<sessionId>.jsonl` |
| `cwd` | absolute path | The project the session works on |
| `procStart` | string of digits | **Windows FILETIME of the process start**: equal to the start time of the process (0 ms difference on 4 sessions). It tells a live session from a dead one whose PID was reused. On Linux it is a different value (field 22 of `/proc/<pid>/stat`, checked on the first repository; to check again). |
| `startedAt`, `updatedAt`, `statusUpdatedAt` | number, epoch ms | When the session started, was last written, last changed its status |
| `status` | string | **`busy`, `waiting` or `idle`** (`waiting`: see `spike-waiting-signal.md`) |
| `name`, `nameSince`, `nameSource` | string, number, string (optional) | Display name of the session |
| `version`, `kind`, `entrypoint` | strings | Claude Code version; `interactive`; `claude-desktop` (other values not seen) |
| `peerProtocol`, `peerFeatures`, `hostSessionId`, `pidDomain`, `messagingSocketPath`, `bridgeSessionId` | various | Internal to Claude Code; not needed |

**Not verified:** whether the file is deleted when a session ends normally, and what is left after a crash. The reader
must therefore treat a file whose process is gone (or whose `procStart` differs) as a closed session.

## 3. `projects/`: the transcripts

### Folder names

A project folder is named after the `cwd` of the session, with `:` and `\` replaced by `-`
(`C:\Work\Demo` → `C--Work-Demo`). The encoding cannot be reversed (a `-` in a folder name looks the same), so the real path
is always read from the `cwd` field, never decoded from the folder name. The rule for spaces, dots and other characters is
not verified.

### Inside a project folder

| Path | Content |
|------|---------|
| `<sessionId>.jsonl` | Transcript of a main session (the chief) |
| `<sessionId>/subagents/agent-<agentId>.jsonl` | Transcript of one sub-agent run (`agentId`: 17 hexadecimal characters) |
| `<sessionId>/subagents/agent-<agentId>.meta.json` | Description of that run (below) |
| `<sessionId>/subagents/agent-<agentId>.prefix.json` | Prompt cache diagnostics: not needed |
| `<sessionId>/tool-results/` | Large tool outputs moved out of the transcript: not needed |
| `<sessionId>/custom-title.json` | `{ "customTitle": … }`: the title of the session |

### Records of a transcript (JSON Lines, one record per line)

| `type` | Use for OpenSpaceIA |
|--------|---------------------|
| `assistant` | What Claude does: `message.content[]` holds `text`, `thinking` and **`tool_use`** items (`id`, `name`, `input`); `message.model`; `stop_reason` (`tool_use`, `end_turn`) |
| `user` | A prompt (string content) or **`tool_result`** items (`tool_use_id`, `is_error`); `permissionDecision` and `toolDenialKind` (see `spike-waiting-signal.md`) |
| `attachment`, `system`, `queue-operation`, `custom-title`, `agent-name`, `last-prompt`, `mode`, `pr-link`, `cost-state` | Context records: only `custom-title` (last one wins) may be useful |
| `api-request`, `api-request-blob`, `api-request-shape`, `file-history-snapshot`, `file-history-delta`, `bridge-session`, `atis-latch` | Internal: skipped |

Every conversation record (`user`, `assistant`, `attachment`, `system`) has `uuid`, `parentUuid`, `sessionId`, `timestamp`
(ISO 8601), `cwd`, `gitBranch`, `version` and `isSidechain`.

```json
{ "type": "assistant", "uuid": "…", "timestamp": "2026-10-30T09:00:00.000Z", "isSidechain": false,
  "message": { "model": "claude-…", "content": [ { "type": "tool_use", "id": "toolu_…", "name": "Read", "input": { } } ] } }
```

### Sub-agents

- A sub-agent transcript has the same records, plus `agentId`, and `isSidechain: true`. Its first record is the task given
  to the agent; it ends with a `SubagentHandback` tool call.
- The main session launches it with an `Agent` tool call; the `meta.json` links back to it with `toolUseId`.

| Key of `agent-<agentId>.meta.json` | Presence | Use |
|------------------------------------|----------|-----|
| `agentType` | always | The agent (for example `csharp-reviewer`, `Explore`, `general-purpose`): **its desk in the office** |
| `description` | always | The short task: **the card above the agent** (plain text, cut, never rendered) |
| `toolUseId` | always | Link to the `Agent` call of the main session |
| `spawnDepth`, `parentAgentId` | always, only at depth 2 | Agent launched by another agent |
| `finished` | **missing in 49 of 138 files** | `true` when the run is over; missing while it runs (and maybe in older files) |
| `model` | sometimes | The model asked at launch; when missing, the real model is read in the transcript (`message.model`) |
| `worktreePath`, `worktreeBranch` | worktree runs only | Not needed |

56 sub-agent transcripts have no `meta.json` (older version or killed run: not verified). The reader must accept a
transcript without its description.

## 4. `agents/`: the agent definitions

One `<name>.md` per user agent, with a front matter of exactly four keys: `name`, `description`, `tools`
(comma-separated) and `model` (`sonnet`, `opus`…). Built-in agents (`general-purpose`, `Explore`) have no file: the
application must know them without one. Files such as `*.md.bak-<date>` are not definitions.

Corrected in 0.4.0, from the Claude Code documentation ("Create custom subagents"): the agent is identified by its `name`,
and the file name does not have to match it (here, they are the same). Only `name` and `description` are required; a
file without `name` is skipped. A name has at most 256 characters and no colon. Claude Code also reads the subfolders of
`agents/`; when two files give the same name, it keeps one of them in an order it does not document.

## 5. Task lists

- The sessions of the Claude desktop application record their task list with the `mcp__ccd_turn__update_plan` tool:
  `{ items: [ { content, status } ] }` where `status` is `pending`, `in_progress` or `completed`, then `{ advance: true }`
  or `{ done: [n], start: n }`. The current list is rebuilt by replaying these calls in order.
- `TodoWrite` and `TaskCreate` were not found in this folder. **Not verified:** what a session started from the command
  line uses (probably one of them); the reader must support the tools it actually meets, and show "no task list" otherwise.

## 6. Sizes: incremental reading is needed

| Measure | Value |
|---------|-------|
| `.jsonl` files | 220 (26 main sessions, about 190 sub-agent runs) |
| Total size | about 630 MB |
| Largest transcript | about 305 MB (one main session) |
| Longest line | about 900 000 characters |

Consequences for the reader (0.4.0):

- **Read each transcript from the last position read**: the files only grow. Keep the position per file, read complete
  lines only (the last line may be cut), and start again from zero if a file becomes shorter.
- **Skip the internal records before parsing them**: they start with `{"type":"api-request` or `{"type":"file-history`,
  and they make up a large part of a transcript (about 40 % of a 42 MB file).
- **Limit the length of a line** that is parsed fully; a longer one is skipped and reported in the format health.

## 7. Open questions

| Question | Where it is decided |
|----------|---------------------|
| Is a session file deleted at the end of a session, or after a crash? | `claude-model-decisions` (rule: a file whose process is gone is a closed session) |
| ~~Does a permission prompt set `status: waiting`?~~ | **Yes**, measured: see `spike-waiting-signal.md` |
| Which task list tool does a command-line session use? | Checked with a command-line session before the reader (0.4.0) |
| ~~Does Claude Code delete old transcripts (history of 7 days)?~~ | After 30 days by default (`cleanupPeriodDays`); the sessions of Claude Desktop are exempt from this limit by default: see `claude-model-decisions.md` |
| The exact encoding of folder names | Not needed: the path is read from `cwd` |
