# 0005. Instruction parsing pipeline

`InstructionParser.Parse` turns one line of terminal input into a typed
`Instruction`. Nothing downstream ever sees the raw string again.

```
/spare-money help --argumentOne hello world --argumentTwo 1
   └ name      └ sub-name  └ arguments
```

It runs three stages: **index** the four token regions without allocating,
**extract** them into strings and split arguments on `--` then on the first
space, then **type** each value.

## Type comes from the value, not the command

This is the surprise. No command declares that `--dueDate` is a
`DateOnly`. Each raw value is offered to the registered builders in order,
and the first to claim it wins:

| Order | Builder | Claims a value that... |
|---|---|---|
| 1 | `DirectoryInfo` | is a rooted path, or starts with `.` |
| 2 | `Guid` | parses as a GUID |
| 3 | `String` | has a letter, and isn't `true`/`false` |
| 4 | `Int` | parses as a whole number |
| 5 | `Decimal` | parses as a decimal |
| 6 | `DateOnly` | parses as a date |
| 7 | `Bool` | always |

The "has a letter" test on `String` is what keeps `--count 5` an `int`.
Conversely `--name 42` is an `int`, whatever the argument is called. A bare
`--verbose` has no value, so it falls through to `Bool` and becomes `true`.

**Order is the whole contract.** Move a builder and every value once
claimed by the one ahead of it silently changes type.

## Gaps

- No quoting or escaping, so `--` cannot appear inside a value.
  [#39](https://github.com/KitCli/KitCli/issues/39)
- `Int`, `Decimal`, and `DateOnly` parse with the current thread culture,
  not invariant. [#22](https://github.com/KitCli/KitCli/issues/22)
- `Bool` matching everything makes the builder extension point
  unreachable from consuming code.
  [#9](https://github.com/KitCli/KitCli/issues/9)
- The indexer honours a configured argument prefix; the extractor
  hard-codes `--`. Configuring a different one fails halfway.

## See also

[0001-command-registration.md](0001-command-registration.md) ·
[0008-artefacts.md](0008-artefacts.md) ·
[../user-guides/0005-reading-command-arguments.md](../user-guides/0005-reading-command-arguments.md)
