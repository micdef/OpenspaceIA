### Changed

- Roadmap: network tests use a range of ports reserved for the tests (defined in 1.1.0), so that test runs can work in parallel, instead of port 0; a new principle says that the IP address and the port of every application can always be changed in its configuration.
