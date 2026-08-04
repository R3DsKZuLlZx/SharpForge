# AGENTS.md — SharpForge Project Instructions

> This is the **single source of truth** for AI agents working in this repo,
> following the [agents.md](https://agents.md) convention. Tool-specific files
> (`CLAUDE.md`, `.github/copilot-instructions.md`) are thin pointers to this
> file — do not duplicate guidance into them.

## Project overview

SharpForge is a Blazor WebAssembly site. Blog posts are authored as Markdown files with YAML frontmatter. At build time they are embedded as assembly resources and rendered at runtime by `MarkdownService` (`src/SharpForge.Client/Services/MarkdownService.cs`), which parses frontmatter with **YamlDotNet** and converts the body to HTML with **Markdig** (using auto-identifiers, auto-links, pipe tables, emphasis extras, and task lists).

---

## Commit messages

This project uses **Conventional Commits** (https://www.conventionalcommits.org).

### Format

```
<type>(<optional scope>): <short summary>

<optional body>
```

- **Subject line**: lowercase, imperative mood, no trailing period, max 72 characters.
- **Body** (when needed): wrap at 72 characters. Explain *what* and *why*, not *how*.

### Types

| Type | When to use |
|---|---|
| `feat` | A new feature or user-facing behaviour |
| `fix` | A bug fix |
| `docs` | Documentation only (AGENTS.md, README, code comments) |
| `style` | Formatting, whitespace, CSS — no logic change |
| `refactor` | Code change that neither fixes a bug nor adds a feature |
| `perf` | Performance improvement |
| `test` | Adding or updating tests |
| `build` | Build system, CI, or dependency changes (.csproj, yml, props) |
| `chore` | Anything else that doesn't modify src or test files |

### Scopes

Scope is optional but encouraged. Use the area of the codebase affected:

- `blog` — blog posts, blog listing page, `MarkdownService`
- `course` — training/course pages and components
- `layout` — MainLayout, NavMenu, shared chrome
- `components` — reusable Razor components
- `ci` — GitHub Actions workflows
- `deps` — dependency updates

### Examples

```
feat(blog): add post on source generators

docs: add conventional commits section to AGENTS.md

fix(blog): correct sidebar anchor for C# headings

refactor(components): move BlogPost page out of Blogs subdirectory

build(deps): bump Markdig to 0.38.0

style(layout): align nav menu spacing on mobile
```

### Breaking changes

Append `!` after the type/scope and include a `BREAKING CHANGE:` footer:

```
refactor(blog)!: switch frontmatter parser from YamlDotNet to tomlyn

BREAKING CHANGE: frontmatter format changed from YAML to TOML.
All existing .md files must be updated.
```

---

## Adding a new blog post — checklist

Adding a post requires exactly **one** change:

1. **Create the Markdown file** at `src/SharpForge.Client/Content/Blog/<slug>.md`.

That's it. `MarkdownService` discovers posts at runtime by reflecting over the
assembly's embedded resources, so there is **no list to update anywhere**. The
`.csproj` already includes `Content\Blog\*.md` as embedded resources, so no
project file changes are needed either.

Posts are sorted by `date` descending automatically — ordering is derived from
frontmatter, not from file order or any hand-maintained list.

---

## File naming

- Use lowercase kebab-case for the slug: `my-new-post-topic.md`.
- The slug becomes the URL: `/blog/my-new-post-topic`.
- Keep slugs concise but descriptive.

---

## Frontmatter format

Every file starts with a YAML frontmatter block delimited by `---`. All fields are required.

```yaml
---
title: "Human-Readable Title"
category: "Category Name"
date: "Month Day, Year"
readTime: "X min read"
excerpt: "One or two sentence summary used on the listing page and in the post header."
tags: ["Tag1", "Tag2", "Tag3"]
sidebar:
  - href: "#heading-slug"
    text: "Short Label"
  - href: "#another-heading-slug"
    text: "Another Label"
---
```

### Field details

| Field | Type | Notes |
|---|---|---|
| `title` | string | Wrap in double quotes. Title-case. |
| `category` | string | Must be one of the existing categories (see below). |
| `date` | string | Format: `"Month Day, Year"` e.g. `"March 15, 2026"`. |
| `readTime` | string | Format: `"X min read"` e.g. `"10 min read"`. Estimate ~200 words/min. |
| `excerpt` | string | 1–2 sentences. This appears on the listing page and the post header. |
| `tags` | string[] | YAML flow sequence `["A", "B", "C"]`. 3–5 tags. |
| `sidebar` | object[] | Each entry has `href` (auto-id anchor) and `text` (short label). Typically 5–7 entries mapping to the main `## ` headings. |

### Existing categories

Use one of these to keep the category filter on the listing page consistent:

- `AI`
- `Architecture`
- `ASP.NET Core`
- `Best Practices`
- `Blazor`
- `C#`
- `DevOps`
- `Entity Framework`
- `Featured` (reserved for the single featured/pinned post)
- `Performance`
- `Testing`

If a new category is genuinely needed, add it — but prefer reusing existing ones.

### Sidebar href values

The `href` must be a `#`-prefixed anchor that matches the **auto-generated id** Markdig creates from the heading text via `UseAutoIdentifiers`. The rules are:

1. Lowercase the heading text.
2. Replace runs of whitespace with a single `-`.
3. **Strip** characters that aren't letters, digits, `-`, `_` or `.` — they are removed, *not* replaced with a hyphen.
4. Collapse the result so there are never two consecutive hyphens.

> **Dots are preserved.** This is the rule most often got wrong: `.NET` becomes
> `.net`, not `net`. Likewise `&`, `/`, `(`, `)` and `<`, `>` vanish entirely
> rather than becoming hyphens.

| Heading | Auto-ID |
|---|---|
| `## Getting Started` | `#getting-started` |
| `## C# 13 Language Features` | `#c-13-language-features` |
| `## ASP.NET Core 10` | `#asp.net-core-10` |
| `## System.Text.Json Basics` | `#system.text.json-basics` |
| `## The .NET Memory Model` | `#the.net-memory-model` |
| `## OpenAPI & Swagger` | `#openapi-swagger` |
| `## OnInitialized & OnInitializedAsync` | `#oninitialized-oninitializedasync` |
| `## Large Object Heap (LOH)` | `#large-object-heap-loh` |
| `## Span<T> and Memory<T>` | `#spant-and-memoryt` |
| `## Side-by-side: when to use which` | `#side-by-side-when-to-use-which` |

Do not hand-compute these if you can avoid it — run the content tests
(see [Validation](#validation)), which resolve every `href` against the ids
Markdig actually generates and print the available ids on failure.

---

## Markdown body conventions

### Structure

1. **Opening paragraph** — immediately after the frontmatter. One to three sentences introducing the topic. No heading before it.
2. **`## ` sections** — the main body is organized with `## ` (H2) headings. These are the top-level sections and should correspond to the sidebar entries.
3. **`### ` subsections** — use H3 for subsections within an H2 section. Deeper nesting (`####`) is rare and should be avoided.
4. **Closing section** — end with a `## Conclusion`, `## Summary`, or `## Best Practices` section. A short list of key takeaways or a brief closing paragraph.

### Code blocks

- Always use fenced code blocks with a language identifier: ` ```csharp `, ` ```yaml `, ` ```bash `, ` ```xml `, ` ```dockerfile `, ` ```protobuf `, etc.
- For directory trees, ASCII diagrams and plain console output, use ` ```text `. Never leave a fence unlabelled.
- Keep code examples focused and practical — real-world patterns, not toy examples.
- Use `// ❌ BAD:` and `// ✅ GOOD:` comment annotations when showing anti-patterns vs recommended patterns.
- Use `<` and `>` directly in code blocks (Markdig handles escaping). Do **not** use HTML entities like `&lt;` or `&gt;` inside fenced code blocks.

### Text formatting

- Use **bold** for emphasis on key terms when first introduced.
- Use `inline code` for type names, method names, package names, file paths, and CLI commands mentioned in prose.
- Use standard Markdown lists (`-` for unordered, `1.` for ordered).
- Avoid raw HTML. The Markdig pipeline handles standard Markdown; raw HTML is unnecessary.
- Keep paragraphs short (2–4 sentences). Blog posts are scanned more than read linearly.

### Tone and length

- Professional but approachable. Direct and practical, not academic.
- Target length: **100–300 lines** of Markdown (roughly 800–2500 words).
- Focus on working code and concrete guidance. Minimize theory preambles.

---

## How the listing page works

You do **not** need to touch `Blog.razor` or `Blog.razor.cs` when adding a post.
`Blog.razor.cs` derives everything from the parsed frontmatter of the discovered
Markdown files:

- **Ordering** — posts are sorted by `date` descending by `MarkdownService`.
- **Pagination** — 6 posts per page (`PageSize`), calculated from the post count.
- **Category filter** — the sidebar category list and counts are built with
  `_regularPosts.GroupBy(p => p.Category)`. Adding a post with a new `category`
  value makes that category appear in the filter automatically.
- **Featured post** — the single post whose `category` is `Featured` is pulled
  out via `MarkdownService.GetFeaturedPost()` and excluded from the regular list.

> **Important:** only **one** post may use `category: "Featured"` at a time.
> `GetFeaturedPost()` resolves a single post; if you promote a new post to
> Featured, demote the current one to a real category in the same change.

---

## Topic queue

Blog topics are tracked as **GitHub issues**, not as a file in the repo. This
keeps capture friction low — an idea can be filed from a phone in seconds — and
gives a single queue shared by manual ideas and the automated topic scout.

Open one with the **Blog post topic** issue form
(`.github/ISSUE_TEMPLATE/blog-post.yml`), which captures topic, **angle**,
category, post type and sources.

### Labels

| Label | Meaning |
|---|---|
| `blog post` | Any blog topic issue |
| `topic:proposed` | Awaiting triage — **not** ready to draft |
| `topic:approved` | Triaged and queued for drafting |
| `topic:drafting` | A draft PR is open for this topic |
| `topic:rejected` | Declined — the scout must never re-propose this |
| `topic:published` | Written and shipped |
| `source:manual` | Captured by a human |
| `source:scout` | Surfaced by the automated topic scout |

### Lifecycle

```text
scout / you                you                    agent                 you
  ─────────►  proposed  ─────────►  approved  ─────────►  drafting  ─────────►  published
                                                   │                      │
                                          draft PR opened          PR reviewed + merged
```

Labelling an issue `topic:approved` triggers
`.github/workflows/draft-blog-post.yml`, which runs the `write-blog-post` skill
and opens a **draft** pull request. Two human gates remain deliberately in
place: you decide what gets approved, and you review the PR before it merges.

### Rules

- Only draft from `topic:approved`. Ask before drafting a `topic:proposed`.
- On publishing, move the issue to `topic:published` and reference it in the
  commit — this is what stops duplicate topics being suggested later.
- **Never delete or close-without-label a rejected topic.** `topic:rejected` is
  the memory that prevents it resurfacing.

```bash
gh issue list --label "blog post" --label "topic:approved"   # ready to write
gh issue view <number>                                        # read a brief
```

> The category dropdown in the issue form must stay in sync with the allowed
> categories below — a test enforces this.

### Topic scout

`tools/SharpForge.TopicScout` polls .NET content feeds and classifies each link
against the published back catalogue:

```bash
# report only
dotnet run --project tools/SharpForge.TopicScout -- --days 21
dotnet run --project tools/SharpForge.TopicScout -- --days 21 --show-noise

# file the strongest candidates as topic:proposed issues
dotnet run --project tools/SharpForge.TopicScout -- --create-issues --dry-run
dotnet run --project tools/SharpForge.TopicScout -- --create-issues
```

| Flag | Default | Purpose |
|---|---|---|
| `--days` | 21 | How much feed history to consider |
| `--create-issues` | off | File surviving candidates as `topic:proposed` |
| `--dry-run` | off | Print what would be filed, change nothing |
| `--min-score` | 2.0 | Score floor — weak matches are never filed |
| `--max-issues` | 5 | Hard cap per run |
| `--show-noise` | off | Show what was filtered, and why |

| Classification | Meaning |
|---|---|
| **DECAY** | Overlaps an existing post — that post may now be out of date |
| **GAP** | Nothing in the back catalogue covers this |
| **Noise** | Vendor product marketing, release candidates, calls for speakers |
| **Duplicate** | Already present in the topic queue (read via `gh`) |

Sources implement `ITopicSource` and may be deterministic feeds or agentic.
The deterministic layer does the cheap, reliable work — noise filtering,
dedupe, gap/decay classification — so only a short list needs human or agent
judgement.

> **Feed health is reported explicitly.** A source that has not published in
> 60 days is flagged `STALE`. This matters: a dormant feed and a healthy feed
> that found nothing produce identical output otherwise. The Morning Brew is
> currently dormant (last issue August 2024).

**Everything the scout files is `topic:proposed`, never `topic:approved`** — so
it can never cause a post to be drafted without a human triaging it first.
DECAY candidates are filed ahead of GAP candidates, because a post that has
gone stale is more urgent than one that was never written.

#### Schedule

`.github/workflows/topic-scout.yml` runs it **every Monday at 08:00 UTC**
(`--days 10 --min-score 3 --max-issues 5`) and writes the report to the job
summary. It can also be triggered manually via `workflow_dispatch`, including a
`dry_run` option.

Re-running is safe: the scout reads every existing `blog post` issue — open,
closed and rejected — and skips anything already queued.

---

## Agent skills

Reusable agent workflows live in `.agents/skills/<skill-name>/SKILL.md`. That
is the canonical, tool-neutral location. Tool-specific entry points are **thin
wrappers** that point at it:

| Path | Purpose |
|---|---|
| `.agents/skills/<name>/SKILL.md` | Canonical instructions — edit here |
| `.claude/skills/<name>/SKILL.md` | Claude Code entry point (wrapper) |
| `.github/prompts/<name>.prompt.md` | Copilot entry point (wrapper) |

Available skills:

- **`write-blog-post`** — drafts a post from a topic brief, matches the site's
  voice by reading existing posts in the same category, and iterates until the
  content validation suite passes.

When adding a skill, write the real content once in `.agents/skills/` and add
wrappers for each tool. Never duplicate the instructions.

### Automated drafting

`.github/workflows/draft-blog-post.yml` runs the `write-blog-post` skill when an
issue is labelled `topic:approved`, then opens a draft PR.

**Setup required before it will run:**

1. Add an `ANTHROPIC_API_KEY` repository secret
   (Settings → Secrets and variables → Actions). The workflow fails fast with a
   clear message if it is missing.
2. Enable Settings → Actions → General → **Allow GitHub Actions to create and
   approve pull requests**. Without it, `gh pr create` is rejected.

**Guardrails built in:**

- Refuses to run unless the issue carries `topic:approved`.
- Fails if the agent adds anything other than exactly one file under
  `Content/Blog/` — no drive-by edits to the rest of the repo.
- Always opens a **draft** PR, never auto-merges.
- Runs the validation suite and states the result prominently in the PR body.
- `concurrency` prevents two drafts racing on the same directory.

The agent invocation is isolated in a single clearly-marked step so it can be
swapped for a different provider without touching the surrounding plumbing.

> The validation suite proves a post is *well-formed*, not that it is *correct*.
> The PR review is where accuracy is checked — that gate is the point.

---

## Validation

Every post is validated by an automated test suite in
`tests/SharpForge.Content.Tests`. Run it after adding or editing any post:

```bash
dotnet test tests/SharpForge.Content.Tests/SharpForge.Content.Tests.csproj
```

The suite reads the Markdown files straight off disk and produces one test case
per post, so a failure names the exact file and problem. It enforces:

| Area | Checks |
|---|---|
| Frontmatter | All required fields present; parseable YAML; `date` matches `MMMM d, yyyy`; `readTime` matches `X min read`; `category` is in the allowed list; 3–5 tags; excerpt 40–300 chars |
| Slugs | Lowercase kebab-case; unique across all posts |
| Featured | **Exactly one** post has `category: "Featured"` |
| Sidebar | Every `href` starts with `#` and resolves to a heading id **Markdig actually generates**; no duplicates; every entry has `text`; entry count consistent with the `## ` sections |
| Body | Balanced code fences; every fence has a language; no HTML entities inside fences; opens with a paragraph; no `# ` H1; closes with a wrap-up section; `readTime` consistent with prose + code length |

Because the sidebar test builds the *same* Markdig pipeline as
`MarkdownService`, it is authoritative — it never guesses at the slug algorithm.
When it fails it prints every available heading id, so fixing an anchor is a
copy-paste.

> **Adding a new category** requires updating two places: the list above and
> `FrontmatterTests.AllowedCategories`.

---

## Quick-start template

Copy this to start a new post:

````markdown
---
title: "Your Post Title Here"
category: "ASP.NET Core"
date: "April 1, 2026"
readTime: "10 min read"
excerpt: "A brief summary of what the reader will learn."
tags: ["Tag1", "Tag2", "Tag3"]
sidebar:
  - href: "#first-section"
    text: "First Section"
  - href: "#second-section"
    text: "Second Section"
  - href: "#third-section"
    text: "Third Section"
  - href: "#best-practices"
    text: "Best Practices"
---

Opening paragraph introducing the topic and what the reader will learn.

## First Section

Content here.

```csharp
// code example
```

## Second Section

Content here.

## Third Section

Content here.

## Best Practices

- Key takeaway one
- Key takeaway two
- Key takeaway three
````
