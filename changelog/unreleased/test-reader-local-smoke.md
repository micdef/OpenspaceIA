### Added

- Tests: a local smoke test that reads the real folder of Claude Code of the user and writes the state of the chief and of each agent type, with the times and the problems. It is explicit: a normal run of the tests and the CI never run it, so the files of the user are never read without asking; `test/README.md` says how to run it.
