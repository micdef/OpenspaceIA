# Source projects

One folder per project. Each project is created by the version of the roadmap that first needs it.

| Project | Module | Contents |
|---------|--------|----------|
| `OpenSpaceIA.Core` | Core | The immutable model (`Model/`) and the read port (`Ports/Read/`); no dependency |

## Planned modules

The architecture is a modular monolith, with a light hexagonal architecture (ports and adapters) inside the modules. Each project is named `OpenSpaceIA.<Module>`, like `OpenSpaceIA.Core`.

| Module | Role |
|--------|------|
| Core | The model of the agents, sessions and states, and the rules (working, overloaded, waiting for you, idle). It knows neither the user interface nor the format of the files. Read ports and write ports are separate. |
| `~/.claude` reader | Adapter that reads the files of Claude Code. Only this module changes when the format of `~/.claude` changes. |
| Agent writer | Adapter that creates agents and teams and assigns them, on the local machine only. |
| Collector | Worker service on each machine: reads `~/.claude` through the reader and sends the activity to the hub. |
| Hub | SignalR server in the home lab: gathers the machines and only exposes reading. |
| Desktop | Avalonia application, Windows and Linux: reads through the hub, or directly through the reader when the hub is not reachable; the only part that writes. |
| Web | Later: read only, through the hub. |

## Rules

- The web never references the write side: reading only, guaranteed by the structure and checked by an architecture test.
- The hub only exposes reading.
