# Commit Messages

This project uses **Conventional Commits** (https://www.conventionalcommits.org).

## Format

```
<type>(<optional scope>): <short summary>

<optional body>
```

- **Subject line**: lowercase, imperative mood, no trailing period, max 72 characters.
- **Body** (when needed): wrap at 72 characters. Explain *what* and *why*, not *how*.

## Types

| Type | When to use |
|---|---|
| `feat` | A new feature or user-facing behaviour |
| `fix` | A bug fix |
| `docs` | Documentation only (CLAUDE.md, README, code comments) |
| `style` | Formatting, whitespace, CSS — no logic change |
| `refactor` | Code change that neither fixes a bug nor adds a feature |
| `perf` | Performance improvement |
| `test` | Adding or updating tests |
| `build` | Build system, CI, or dependency changes (.csproj, yml, props) |
| `chore` | Anything else that doesn't modify src or test files |

## Scopes

Scope is optional but encouraged. Use the area of the codebase affected:

- `blog` — blog posts, blog listing page, BlogService
- `course` — training/course pages and components
- `layout` — MainLayout, NavMenu, shared chrome
- `components` — reusable Razor components
- `ci` — GitHub Actions workflows
- `deps` — dependency updates

## Examples

```
feat(blog): add post on source generators

docs: add conventional commits section to CLAUDE.md

fix(blog): correct sidebar anchor for C# headings

refactor(components): move BlogPost page out of Blogs subdirectory

build(deps): bump Markdig to 0.38.0

style(layout): align nav menu spacing on mobile
```

## Breaking changes

Append `!` after the type/scope and include a `BREAKING CHANGE:` footer:

```
refactor(blog)!: switch frontmatter parser from YamlDotNet to tomlyn

BREAKING CHANGE: frontmatter format changed from YAML to TOML.
All existing .md files must be updated.
```

