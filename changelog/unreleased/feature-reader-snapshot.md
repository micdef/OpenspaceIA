### Added

- Reader: `ClaudeActivityReader`, the adapter of the read port of the core. Each snapshot gathers the agent definitions, the open sessions with the reason of a wait, the sub-agent runs and the periods of the chief over the window of history, read outside the thread of the caller; a session whose status is not written yet is left out until the next snapshot. The problems of every part come with the snapshot.
- Core: a problem met while the activity was read (`ReadingProblem`), and the snapshot now holds the periods of the chief and the problems of the reading.

### Changed

- Core: the periods of an agent type from its runs, and the agent types seen only in runs, are computed in the core (`RunActivity`, moved from the Reader).
