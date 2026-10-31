### Changed

- Reader: the agent definitions in the subfolders of `agents/` are read too, as Claude Code reads them; the allow-list accepts `agents/.../<name>.md`. A subfolder that is a junction or a symbolic link is not followed, and it is reported.
