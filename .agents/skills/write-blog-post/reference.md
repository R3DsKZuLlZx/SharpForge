---
name: write-blog-post-reference
description: Frontmatter schema, allowed categories, anchor rules and body conventions for SharpForge blog posts.
---

# Blog post reference

The specification for a post in `src/SharpForge.Client/Content/Blog/<slug>.md`.
Read this when authoring or reviewing a post. Process and quality bar live in
[`SKILL.md`](./SKILL.md); repo-wide conventions live in
[`AGENTS.md`](../../../AGENTS.md).

---

## File naming

- Lowercase kebab-case slug: `my-new-post-topic.md`.
- The slug becomes the URL: `/blog/my-new-post-topic`.
- Concise but descriptive. Slugs must be unique and are never reused.

---

## Frontmatter format

Every file starts with a YAML frontmatter block delimited by `---`. All fields
are required.

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
| `category` | string | Must be one of the categories below. |
| `date` | string | Format: `"Month Day, Year"` e.g. `"March 15, 2026"`. |
| `readTime` | string | Format: `"X min read"` e.g. `"10 min read"`. Estimate ~200 words/min. |
| `excerpt` | string | 1–2 sentences, 40–300 characters. Appears on the listing page and post header. |
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

Prefer reusing an existing category. Adding a new one requires updating **three**
places or CI fails: this list, `FrontmatterTests.AllowedCategories`, and the
category dropdown in `.github/ISSUE_TEMPLATE/blog-post.yml`.

### Sidebar href values

The `href` must be a `#`-prefixed anchor matching the **auto-generated id**
Markdig creates from the heading text via `UseAutoIdentifiers`:

1. Lowercase the heading text.
2. Replace runs of whitespace with a single `-`.
3. **Strip** characters that aren't letters, digits, `-`, `_` or `.` — they are
   removed, *not* replaced with a hyphen.
4. Collapse the result so there are never two consecutive hyphens.

> **Dots are preserved.** This is the rule most often got wrong: `.NET` becomes
> `.net`, not `net`. Likewise `&`, `/`, `(`, `)` and `<`, `>` vanish entirely
> rather than becoming hyphens.

| Heading | Auto-ID |
|---|---|
| `## ASP.NET Core 10` | `#asp.net-core-10` |
| `## OpenAPI & Swagger` | `#openapi-swagger` |
| `## Span<T> and Memory<T>` | `#spant-and-memoryt` |

**Do not hand-compute these.** Write your best guess and run the tests — they
resolve every `href` against the ids Markdig actually generates and print the
full list on failure, so fixing an anchor is copy-paste.

---

## Markdown body conventions

### Structure

1. **Opening paragraph** — immediately after the frontmatter. One to three
   sentences introducing the topic. No heading before it, and no `# ` H1
   anywhere (the title is rendered from frontmatter).
2. **`## ` sections** — the main body. These are the top-level sections and
   should correspond to the sidebar entries.
3. **`### ` subsections** — for subsections within an H2. Deeper nesting
   (`####`) is rare and should be avoided.
4. **Closing section** — end with `## Conclusion`, `## Summary` or
   `## Best Practices`: key takeaways or a brief closing paragraph.

### Code blocks

- Always fence with a language identifier: ` ```csharp `, ` ```yaml `,
  ` ```bash `, ` ```xml `, ` ```dockerfile `, ` ```protobuf `, etc.
- For directory trees, ASCII diagrams and console output use ` ```text `.
  Never leave a fence unlabelled.
- Real-world patterns, not toy examples. Avoid `Foo`, `Bar`, `MyClass`.
- Use `// ❌ BAD:` and `// ✅ GOOD:` annotations when contrasting anti-patterns
  with recommended patterns.
- Use `<` and `>` directly (Markdig handles escaping). Never `&lt;` / `&gt;`
  inside a fence.

### Text formatting

- **Bold** for key terms on first introduction.
- `inline code` for type names, method names, package names, file paths and CLI
  commands mentioned in prose.
- Standard Markdown lists (`-` unordered, `1.` ordered).
- Avoid raw HTML — the Markdig pipeline handles standard Markdown.
- Short paragraphs (2–4 sentences). Posts are scanned more than read linearly.

### Tone and length

- Professional but approachable. Direct and practical, not academic.
- Target **100–300 lines** of Markdown (roughly 800–2500 words).
- Working code and concrete guidance. Minimise theory preambles.

---

## How the listing page works

You do **not** need to touch `Blog.razor` or `Blog.razor.cs` when adding a post.
Everything is derived from parsed frontmatter:

- **Ordering** — sorted by `date` descending by `MarkdownService`.
- **Pagination** — 6 posts per page (`PageSize`), from the post count.
- **Category filter** — built with `_regularPosts.GroupBy(p => p.Category)`, so
  a new `category` value appears in the filter automatically.
- **Featured post** — the post whose `category` is `Featured` is pulled out via
  `MarkdownService.GetFeaturedPost()` and excluded from the regular list.

> **Only one** post may use `category: "Featured"` at a time.
> `GetFeaturedPost()` resolves a single post, so promoting a new post to
> Featured means demoting the current one in the same change.

---

## What validation enforces

```bash
dotnet test tests/SharpForge.Content.Tests/SharpForge.Content.Tests.csproj
```

| Area | Checks |
|---|---|
| Frontmatter | All required fields present; parseable YAML; `date` matches `MMMM d, yyyy`; `readTime` matches `X min read`; `category` in the allowed list; 3–5 tags; excerpt 40–300 chars |
| Slugs | Lowercase kebab-case; unique across all posts |
| Featured | **Exactly one** post has `category: "Featured"` |
| Sidebar | Every `href` starts with `#` and resolves to a heading id **Markdig actually generates**; no duplicates; every entry has `text`; entry count consistent with the `## ` sections |
| Body | Balanced code fences; every fence has a language; no HTML entities inside fences; opens with a paragraph; no `# ` H1; closes with a wrap-up section; `readTime` consistent with prose + code length |

Because the sidebar test builds the *same* Markdig pipeline as
`MarkdownService`, it is authoritative — it never guesses at the slug algorithm.

---

## Quick-start template

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

