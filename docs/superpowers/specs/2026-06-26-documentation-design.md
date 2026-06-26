# Documentation Redesign — Design Spec
Date: 2026-06-26

## Goal

Split the monolithic README into per-module documentation pages published as a Hugo website, while keeping the README as a navigable entry point that mirrors the Hugo home page.

## Constraints

- No commits — user will commit manually.
- No new root-level markdown files. Per-module docs live exclusively in `docs/content/`.
- README and `docs/content/_index.md` must stay in sync (same content, different format headers).
- The Hugo site base URL is `https://artur-rios.github.io/dotnet-util/`.

## File Structure

```
README.md                              ← updated; links to Hugo site URLs
docs/content/
  _index.md                            ← mirrors README; title-only front matter
  collections.md
  flow-control.md
  hashing.md
  http.md
  io.md
  math.md
  random.md
  regular-expressions.md
```

## README / _index.md Content (shared)

Both files contain:
1. Title and one-line description of the library
2. Installation section (NuGet + local project reference)
3. Quickstart section — one brief code snippet per module (all 8 modules: Collections, FlowControl, Hashing, Http, IO, Math, Random, RegularExpressions)
4. Documentation section — bulleted list of links to each Hugo module page
5. Contributing section
6. Legal / license section

`README.md` uses plain Markdown links to the Hugo site URLs.
`docs/content/_index.md` uses the same Markdown body with front matter:
```toml
+++
title = 'Dotnet Util'
+++
```

## Module Pages (`docs/content/<slug>.md`)

### Front matter format

```toml
+++
title          = "<Module Name>"
show_nav       = true
nav_back_label = "<Previous Page Label>"
nav_back_url   = "/dotnet-util/<previous-slug>"
nav_next_label = "<Next Page Label>"
nav_next_url   = "/dotnet-util/<next-slug>"
+++
```

The last page (`regular-expressions.md`) omits `nav_next_label` and `nav_next_url`.

### Navigation chain (alphabetical by folder name)

| Page | Back | Next |
|------|------|------|
| collections | Home (`/dotnet-util`) | Flow Control |
| flow-control | Collections | Hashing |
| hashing | Flow Control | Http |
| http | Hashing | IO |
| io | Http | Math |
| math | IO | Random |
| random | Math | Regular Expressions |
| regular-expressions | Random | — |

### Page body structure (per module)

Each page contains:
1. **Features** — bulleted list of classes/types and their purpose
2. **Class diagram** — Mermaid `classDiagram` block
3. **Usage examples** — one or more `csharp` code blocks demonstrating real usage

## Modules to Document

| Folder | Slug | Classes |
|--------|------|---------|
| `src/Collections` | `collections` | `AnsiColors`, `Characters` |
| `src/FlowControl` | `flow-control` | `Condition`, `ConditionFailedException`, `Retry`, `JitteredWaiter`, `MaxRetriesReachedException` |
| `src/Hashing` | `hashing` | `Hash`, `HashConfiguration` |
| `src/Http` | `http` | `HttpGateway`, `HttpOutput<T>`, `HttpExtensions`, `HttpStatusCodes` |
| `src/IO` | `io` | `FileReader`, `FileReaderAsync` |
| `src/Math` | `math` | `PrimeUtils`, `PrimeGenerator<T>` |
| `src/Random` | `random` | `CustomRandom`, `RandomStringOptions` |
| `src/RegularExpressions` | `regular-expressions` | `RegexCollection`, `RegexExtensions` |

## Out of Scope

- Hugo theme changes
- CI/CD pipeline changes
- New source code
- Committing or pushing changes
