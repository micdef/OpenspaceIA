### Changed

- Core: "under-used" is calibrated on a real week. An agent is now under-used when it was idle at least 95 % of the active time of the chief (its work and its waiting) over 7 days, instead of 90 % of the whole 7 days, which flagged every agent; it does not apply to the chief, and no agent is under-used when the chief was never active. The thresholds of "overloaded" are confirmed (2 hours, 2 projects, a break under 10 minutes). `doc/analyse/state-definitions.md` gives the measures behind the choice.
