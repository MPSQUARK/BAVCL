# Migrations

One file per version step. Read every file between the tag you are on and the tag you are moving to.

Each change is a title, what broke, why, and what to call instead. Behaviour that did not change is in [the spec](../BAVCLSpecification.md) and [Features](../Features.md).

The next release adds `{previous}-to-{next}.md` and one row here. Do not add it to an older file.

| Step | File |
|------|------|
| Pre-0.9.0 → 0.9.0 | [to-0.9.0.md](to-0.9.0.md) |
| 0.9.0 → 0.9.1 | [0.9.0-to-0.9.1.md](0.9.0-to-0.9.1.md) |
| 0.9.1 → 0.9.2 | [0.9.1-to-0.9.2.md](0.9.1-to-0.9.2.md) |
| 0.9.2 → 0.9.3 | [0.9.2-to-0.9.3.md](0.9.2-to-0.9.3.md) |
| 0.9.3 → 0.9.4 | [0.9.3-to-0.9.4.md](0.9.3-to-0.9.4.md) |
| 0.9.4 → 0.9.5 | [0.9.4-to-0.9.5.md](0.9.4-to-0.9.5.md) |

`0.9.0` is the first tag in this series. `0.9.5` is not tagged yet.
