### Added

- Architecture rules (`test/OpenSpaceIA.Architecture.Tests`): the Core depends on nothing but the .NET base library. Its project file has no project or package reference, and every type its code uses (method bodies included, read by ArchUnitNET) lives in `System.*` or in the Core; a control proves that the external types are really seen.
