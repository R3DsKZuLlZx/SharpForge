# AGENTS.md — SharpForge Project Instructions

> Entry point for AI agents, following the [agents.md](https://agents.md)
> convention. This file is prepended to **every** agent request, so it stays
> small and routes to detail that loads only when it is actually needed.
> Put new guidance in the linked documents, not here — a test enforces the cap.

## Project overview

SharpForge is a Blazor WebAssembly site. Blog posts are Markdown files with YAML
frontmatter in `src/SharpForge.Client/Content/Blog/`, embedded as assembly
resources at build time and rendered at runtime by `MarkdownService`
(`src/SharpForge.Client/Services/MarkdownService.cs`) — YamlDotNet parses the
frontmatter, Markdig converts the body (auto-identifiers, auto-links, pipe
tables, emphasis extras, task lists).

## Where to look

| If you're… | Read |
|---|---|
| Writing or drafting a blog post | `.agents/skills/write-blog-post/SKILL.md` |
| Looking up frontmatter, categories, anchors or body conventions | `.agents/skills/write-blog-post/reference.md` |
| Working the topic queue or the topic scout | `docs/topic-queue.md` |
| Wiring up skills, workflows or CI | `docs/automation.md` |

Tool-specific files (`CLAUDE.md`, `.github/copilot-instructions.md`,
`.github/instructions/*.instructions.md`, `.claude/skills/`, `.github/prompts/`)
are **thin pointers**. Never duplicate guidance into them.

## Published posts are immutable

Never edit, rewrite or "refresh" a post in `Content/Blog/`. Each post is a dated
record of what was true when it was written. If something fundamental changed —
a breaking API, a new major version, advice that is now wrong — write a
brand-new post that supersedes it. If the change is smaller than that, write
nothing and say so plainly. There is no third outcome. Typo fixes and
broken-link repairs are the one exception, and must not change the substance of
a post. Full rule: `.agents/skills/write-blog-post/SKILL.md`.

## Adding a blog post

Exactly **one** change: create `src/SharpForge.Client/Content/Blog/<slug>.md`,
lowercase kebab-case — the slug becomes the URL `/blog/<slug>`. Posts are
discovered by reflection over embedded resources and sorted by `date`
descending, so there is no list, no `.csproj` entry and no listing page to
update. Schema and conventions live in
`.agents/skills/write-blog-post/reference.md`.

## Validation

Run after any change to content, docs or tests:

```bash
dotnet test tests/SharpForge.Content.Tests/SharpForge.Content.Tests.csproj
```

The suite reads Markdown straight off disk, one test case per post, so failures
name the exact file and problem. It is authoritative on sidebar anchors: it
builds the same Markdig pipeline as `MarkdownService` and prints every available
heading id on failure. **Never hand-compute an anchor slug** — write your best
guess and let the tests correct it.

## Commit messages

[Conventional Commits](https://www.conventionalcommits.org) —
`<type>(<optional scope>): <short summary>`

- **Subject**: lowercase, imperative mood, no trailing period, max 72 chars.
- **Body** (when needed): wrap at 72 chars. Explain *what* and *why*, not *how*.
- **Breaking**: append `!` after the type/scope and add a `BREAKING CHANGE:` footer.

| Type | When to use |
|---|---|
| `feat` | A new feature or user-facing behaviour |
| `fix` | A bug fix |
| `docs` | Documentation only |
| `style` | Formatting, whitespace, CSS — no logic change |
| `refactor` | Neither fixes a bug nor adds a feature |
| `perf` | Performance improvement |
| `test` | Adding or updating tests |
| `build` | Build system, CI or dependency changes (.csproj, yml, props) |
| `chore` | Anything else that doesn't modify src or test files |

Scopes are optional but encouraged — use the area affected: `blog`, `course`,
`layout`, `components`, `ci`, `deps`.

```text
feat(blog): add post on source generators
fix(blog): correct sidebar anchor for C# headings
refactor(blog)!: switch frontmatter parser from YamlDotNet to tomlyn
```

