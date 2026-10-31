### Added

- Reader: the agent definitions of the user (`agents/*.md` of Claude Code) become the agent types of the core: name, description and model, read from the front matter by a small parser of our own (the simple forms of YAML only, no package of a third party). The reading is tolerant: a definition that cannot be read or parsed, a value in a form that is not read, a second definition of the same name and a subfolder (not read yet) are each reported as a problem with their file, and never stop the reading of the others.

### Fixed

- Spike on the files of Claude Code: an agent is identified by the `name` of its front matter, not by its file name, and Claude Code also reads the subfolders of `agents/` (Claude Code documentation).
