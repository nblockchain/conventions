# Documentation Overview

This directory contains our org's development conventions, split across three documents:

- **[WorkflowGuidelines.md](WorkflowGuidelines.md)** – How we collaborate day‑to‑day: Pull Request structure, commit hygiene, pushing strategy, merge options, Git usage (rebasing, cherry‑picking, `pull --rebase`), commit‑message format, script language policy, and naming conventions for scripts, CI jobs, constants, projects, and APIs.

- **[CodingStyleGuidelines.md](CodingStyleGuidelines.md)** – Language‑agnostic coding style rules that apply across all the languages we use. Covers verbosity preferences, import grouping, `this.` usage, serialization choices, indentation & brace style, comment best practices, and the avoidance of common anti‑patterns (e.g. magic numbers, DRY violations, primitive obsession, exception mishandling, type‑system neglect, obscure operators, and more).

- **[FSharpStyleGuide.md](FSharpStyleGuide.md)** – An F#‑specific companion to the coding style guidelines above. Because F# is our preferred programming language, this document goes deeper into conventions that only make sense in F# (casing of F# types, curried vs. tupled arguments, `async{}` vs. `task{}`, Option handling, FSharpLint rules, etc.). Many of the general principles in **CodingStyleGuidelines.md** still apply when writing F#, but **FSharpStyleGuide.md** contains the additional, F#‑only details.
