### Added

- Reader: a transcript can be read only where it grew: the complete lines after a position are read, with the same safety as the other files, and a line that Claude Code is still writing is left for the next reading.
- Reader: the reason why a chief waits for the user. For each session whose status is waiting, its transcript is read from where the previous reading stopped, and the tool call left open gives the reason: a question for `AskUserQuestion`, a permission for any other tool, and a question when no call is open yet (the status of a question can come before its record). A waiting session always gets a reason; a missing or unreadable transcript is reported.

### Changed

- Spike of the waiting signal: the time between a tool call and its result, measured by source of the decision on a week of transcripts, and the definition of a period of waiting of the chief.
- Roadmap: `feature/reader-chief-periods` comes after `feature/reader-waiting-signal`.
