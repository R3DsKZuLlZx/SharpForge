---
name: write-blog-post
description: Draft a new SharpForge blog post from a topic brief, matching the site's existing voice and passing the content validation suite.
---

# Skill: Write a blog post

Drafts a new post into `src/SharpForge.Client/Content/Blog/<slug>.md`.

**[`reference.md`](./reference.md) is the specification.** It defines the
frontmatter schema, allowed categories, anchor rules, body conventions, tone and
exactly what CI enforces. Read it before drafting. This file covers *process* —
what to do before and after writing — and the quality bar that automated checks
cannot enforce.

---

## Input: the topic brief

Topics live as **GitHub issues** labelled `blog post`. The usual invocation is
an issue number:

```bash
# Read the brief
gh issue view <number>

# What's approved and waiting?
gh issue list --label "blog post" --label "topic:approved"
```

The issue form captures the fields below. If the user supplies a brief inline
instead, the same fields apply.

| Field | Required | Notes |
|---|---|---|
| Topic | yes | What the post is about |
| **Angle** | yes | The opinion or insight. "Why this, why now, what do *we* think?" |
| Category | yes | Must be in the `reference.md` allowed list |
| Post type | yes | News-pegged / deep-dive / practical / opinion |
| Sources | if news-pegged | Release notes, specs, issues — real URLs |

**Do not invent an angle** — it is the one thing that makes the post worth
publishing. If the brief has no angle beyond "explain X", say so and ask for
one. A post that only explains X is indistinguishable from every other post
about X.

Only draft from issues labelled `topic:approved`. A `topic:proposed` issue has
not been triaged yet — confirm with the user before writing.

---

## Process

### 1. Orient before writing

```bash
# What already exists? Avoid duplicating or contradicting yourself.
ls src/SharpForge.Client/Content/Blog/
```

- Check whether an existing post already covers this. If one does, apply the
  **supersede-or-skip** rule below.
- Confirm the slug is unique and lowercase kebab-case.
- **Read 2–3 existing posts in the same category, in full.** This is not
  optional — it is how the draft matches the site's voice instead of sounding
  like generic LLM output. Note sentence length, how code is introduced, how
  opinions are phrased.

#### Supersede or skip — never edit

**Published posts are immutable.** Never modify, rewrite or "refresh" an
existing post in `Content/Blog/`. They are a dated record of what was true when
written. When a topic overlaps something already published there are exactly two
outcomes — there is no third:

| Situation | Action |
|---|---|
| Something **fundamental** changed — a breaking API, a new major version, advice that is now wrong | Write a **brand-new post** that supersedes it |
| Anything less — a minor revision, a small correction, a nice-to-have detail | **Write nothing.** Say so and stop. |

If it is not worth a whole new post, it is not worth doing — say that plainly
rather than producing a thin post to justify the ticket.

A superseding post must stand alone (assume the reader has not read the older
one), use its own slug and its own date, and name what changed and why. It may
link to the older post; it must never silently contradict it.

### 2. Draft

Follow the body conventions in [`reference.md`](./reference.md). Structure:

1. Opening paragraph — no heading, states the problem and the take
2. `## ` sections, `### ` subsections
3. Closing `## Conclusion` / `## Summary` / `## Best Practices`

### 3. Write the sidebar last

Write the body first, then derive sidebar entries from the `## ` headings that
actually exist.

> **Do not hand-compute anchor slugs.** The rules are subtle — dots are
> preserved (`.NET` → `.net`), `&` and `/` vanish entirely. Getting this wrong
> is the single most common failure. Write your best guess, then let the test
> suite tell you the real ids.

### 4. Validate — mandatory

```bash
dotnet test tests/SharpForge.Content.Tests/SharpForge.Content.Tests.csproj
```

Failures name the exact file and problem. The sidebar test prints **every
available heading id**, so fixing an anchor is copy-paste.

Iterate until green. **A post is not finished until this passes.**

### 5. Report and close the loop

Tell the user: the slug/URL, the angle, word count and readTime, and anything
you were unsure about or had to guess.

If the post came from an issue, relabel it so the scout knows not to
re-propose the topic:

```bash
gh issue edit <number> --remove-label "topic:approved" --add-label "topic:published"
```

Reference the issue in the commit: `feat(blog): add post on <topic> (#<number>)`

---

## Hard rules

These fail CI. They are specified once, in [`reference.md`](./reference.md) —
frontmatter fields and formats, the allowed category list, the one-Featured-post
rule, fence languages, and the body structure rules. Do not restate them here;
check them there.

---

## Quality bar — what tests cannot check

The validator proves a post is *well-formed*. It cannot prove it is *worth
reading*. That part is on you.

**Write:**
- Code that would compile and that solves a real problem.
- The trade-off and the "when not to use this" — the part most posts omit.
- Concrete specifics: version numbers, actual benchmark figures, real error
  messages.
- A clear opinion, stated plainly.

**Avoid:**
- Padding: "In today's fast-paced world", "It's important to note that".
- Listicles assembled from documentation. If the reader could get it from
  Microsoft Learn in 30 seconds, it doesn't need a post.
- Toy examples (`Foo`, `Bar`, `MyClass`). Use plausible domain names.
- Fabricated benchmarks, quotes, or API surfaces. **If you are not certain an
  API exists, do not write it.** Verify against the supplied sources.
- Hedging every claim into meaninglessness.

**Length:** 100–300 lines of Markdown. If it's shorter, the topic was too thin;
if longer, it's probably two posts.

---

## Definition of done

- [ ] File at `src/SharpForge.Client/Content/Blog/<slug>.md`, nothing else changed
- [ ] **No existing post was modified** — new file only
- [ ] Frontmatter complete; category from the allowed list
- [ ] Sidebar entries all resolve to real headings
- [ ] `dotnet test tests/SharpForge.Content.Tests/...` passes
- [ ] Every technical claim traceable to a source, or flagged to the user
- [ ] Conventional Commit suggested, e.g. `feat(blog): add post on <topic>`

Alternatively, a valid outcome is **no post at all** — with a clear explanation
of why the change was not significant enough to warrant one.
