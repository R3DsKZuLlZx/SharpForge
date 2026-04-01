# CLAUDE.md — SharpForge Project Instructions

## Commit messages

See `.github/copilot-instructions.md` for Conventional Commits rules. Follow them for all commits.

## Project overview

SharpForge is a Blazor WebAssembly site. Blog posts are authored as Markdown files with YAML frontmatter. At build time they are embedded as assembly resources and rendered at runtime by a `BlogService` that parses frontmatter with **YamlDotNet** and converts the body to HTML with **Markdig** (using auto-identifiers, auto-links, pipe tables, emphasis extras, and task lists).

## Adding a new blog post — checklist

Every new blog post requires **two** changes:

1. **Create the Markdown file** at `src/SharpForge.Client/Content/Blog/<slug>.md`.
2. **Add an entry** to the `AllPosts` list in `src/SharpForge.Client/Pages/Blog.razor` so the post appears on the blog listing page. Insert the entry in **reverse-chronological order** (newest first).

The `.csproj` already includes `Content\Blog\*.md` as embedded resources, so no project file changes are needed.

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

The `href` must be a `#`-prefixed anchor that matches the **auto-generated id** Markdig creates from the heading text. Markdig's `UseAutoIdentifiers` lowercases the heading, replaces spaces with hyphens, and strips special characters. Examples:

| Heading | Auto-ID |
|---|---|
| `## Getting Started` | `#getting-started` |
| `## C# 13 Language Features` | `#c-13-language-features` |
| `## ASP.NET Core 10` | `#aspnet-core-10` |
| `## Use IAsyncEnumerable for Streaming` | `#use-iasyncenumerable-for-streaming` |

Verify anchors match your `## ` headings exactly.

---

## Markdown body conventions

### Structure

1. **Opening paragraph** — immediately after the frontmatter. One to three sentences introducing the topic. No heading before it.
2. **`## ` sections** — the main body is organized with `## ` (H2) headings. These are the top-level sections and should correspond to the sidebar entries.
3. **`### ` subsections** — use H3 for subsections within an H2 section. Deeper nesting (`####`) is rare and should be avoided.
4. **Closing section** — end with a `## Conclusion`, `## Summary`, or `## Best Practices` section. A short list of key takeaways or a brief closing paragraph.

### Code blocks

- Always use fenced code blocks with a language identifier: ` ```csharp `, ` ```yaml `, ` ```bash `, ` ```xml `, ` ```dockerfile `, ` ```protobuf `, etc.
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

## Updating the blog listing page

After creating the Markdown file, add a corresponding entry to the `AllPosts` list in `src/SharpForge.Client/Pages/Blog.razor`:

```csharp
new() {
    Title = "Same Title as Frontmatter",
    Excerpt = "Same excerpt as frontmatter.",
    Date = "Month Day, Year",
    Category = "Category",
    Url = "blog/slug-name"
},
```

- Insert in reverse-chronological order (newest at the top of the list).
- The `Title`, `Excerpt`, `Date`, and `Category` should exactly match the frontmatter values.
- The `Url` is `blog/<slug>` (no leading slash).
- The listing page shows 6 posts per page. Page comments (`// Page 2 posts`, `// Page 3 posts`) exist in the list — adjust them if the new post shifts pagination boundaries.

---

## Quick-start template

Copy this to start a new post:

```markdown
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
