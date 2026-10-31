### Added

- Reader: the transcripts are read line by line, with the same safety as the other files (allow-list, links refused, full sharing), so that a transcript of hundreds of megabytes is never held in memory; a line longer than the limit is skipped and counted.
- Reader: the runs of the sub-agents written in the window (`projects/<project>/<session>/subagents/agent-<id>.jsonl`), each with its agent type (from its description `agent-<id>.meta.json`), its session, its project, its first and last records, its end and its real model. A run without description is left out and reported; each fact of a line is read on its own, and the lines that cannot be read are reported once per file; a time must be a full ISO date with its offset, and a record dated more than five minutes after now is not read; the memory folders are never walked. A run whose files have not changed is taken from memory instead of being read again; nothing is written on the disk.
- Reader: the periods of work of an agent type come from its runs, and an agent seen only in runs (a built-in agent, an agent of a plugin) gets a desk.

### Changed

- Reader: the allow-list also accepts the descriptions of the sub-agent runs, at their exact place (`projects/<project>/<session>/subagents/agent-<id>.meta.json`), outside the memory folders.
- Decision 4 of the model of Claude Code: a run without its description is left out and reported.
