### Fixed

- Doxygen documentation: the C# records of the model and the internal types now have their page. Doxygen 1.18 does not know the `record` keyword, so the C# files are read through a filter (`tools/doxygen/Convert-RecordsForDoxygen.ps1`) that presents them as classes and structs, without changing the sources; the examples of paths in the comments of `Project` use the slash, which Doxygen does not read as a command.
