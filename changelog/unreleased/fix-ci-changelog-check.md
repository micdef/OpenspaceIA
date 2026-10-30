### Fixed

- CI: the changelog check now applies only to pull requests into a release branch, so that the pull requests that move a version up into `dev` and `main`, which carry the finalized changelog, are no longer refused.
