### Added

- State rules of the Core (`src/OpenSpaceIA.Core/States`): the state of the moment of the chief and of an agent type (working, waiting for you, idle), "overloaded" and "under-used", computed from the periods of an agent with "now" and the settings as parameters, never the clock.
- Periods of activity in the model (`ActivityPeriod`, of work or of waiting), and the settings of the rules with their defaults (`StateSettings`).
- State definitions: the periods given to the rules, the merge of the periods that overlap, and how the thresholds are read (the start and the end of the window count as breaks; a period counts only for a part of more than zero inside the window).
