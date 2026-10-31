### Added

- Reader: the open sessions of Claude Code (`sessions/*.json`). Each session file gives its process, its project, its start and its status; the key files next to it are never opened. A session whose process is gone, or whose process ID now belongs to a process started at another time, is closed and left out; one whose process cannot be checked is kept and reported. The process is checked on Windows (start time of the process) and on Linux (`/proc/<pid>/stat`), as measured on both systems.

### Changed

- Roadmap: a line that cannot be read is reported in the result of the reader, and the host writes the logs; `feature/reader-activity` is split into `feature/reader-sessions` and `feature/reader-runs`.
- Spike on the files of Claude Code: what `procStart` holds on Windows and on Linux, the status written about one second after the file, and the file deleted when the session ends.
