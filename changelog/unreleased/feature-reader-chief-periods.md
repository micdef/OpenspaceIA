### Added

- Reader: the periods of work and of waiting of the chief, read in the transcripts of its sessions written in the window, only where they grew. A turn goes from a message of the user to a final answer, or to an interruption by the user; a message typed while Claude works continues the turn; a local command or a summary of a compaction does not start a turn; a silence of more than 30 minutes without a tool call in progress ends the turn at its last record. A period found in two transcripts (a resumed session) is kept once. A wait is the time from a tool call to its result when the user decided. The periods of work are the turns minus their waits. A record dated more than five minutes after now is not read; the transcripts of the sub-agents and the memory folders are never read as a chief.

### Changed

- Reader: the following of a transcript only where it grew, and the reading of the time of a record, are shared by the readers of the reason of a wait, of the runs and of the periods of the chief.
- State definitions: how the turns and the waits of the chief are read in the transcripts.
