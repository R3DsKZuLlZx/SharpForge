# Agent automation

How agent instructions are laid out in this repo and how the automated drafting
workflow is wired. Read this when adding a skill or changing CI — not needed to
write a post.

## Context budget

`AGENTS.md` is prepended to **every** agent request in every session, whether or
not the task has anything to do with blogging. Everything in it is paid for on
every turn, so it is deliberately kept under 120 lines and a test enforces that.

The rule: **guidance lives at the narrowest scope that still reaches the agent
that needs it.**

| Scope | Location | Loaded |
|---|---|---|
| Whole repo, every task | `AGENTS.md` | Always — keep minimal |
| One task | `.agents/skills/<name>/SKILL.md` + siblings | When the skill is invoked |
| One file pattern | `.github/instructions/*.instructions.md` (`applyTo` glob) | When a matching file is in context |
| Humans / CI setup | `docs/*.md` | Only when linked to |

If you are about to add a section to `AGENTS.md`, ask which of the lower three
rows it belongs in first. It is almost always one of them.

## Agent skills

Reusable agent workflows live in `.agents/skills/<skill-name>/SKILL.md`. That is
the canonical, tool-neutral location. Tool-specific entry points are **thin
wrappers** that point at it — a test caps them at 40 lines:

| Path | Purpose |
|---|---|
| `.agents/skills/<name>/SKILL.md` | Canonical instructions — edit here |
| `.agents/skills/<name>/*.md` | Supporting reference loaded on demand |
| `.claude/skills/<name>/SKILL.md` | Claude Code entry point (wrapper) |
| `.github/prompts/<name>.prompt.md` | Copilot entry point (wrapper) |

Available skills:

- **`write-blog-post`** — drafts a post from a topic brief, matches the site's
  voice by reading existing posts in the same category, and iterates until the
  content validation suite passes. Its schema reference is a sibling file
  (`reference.md`) so it only loads when a post is actually being written.

When adding a skill, write the real content once in `.agents/skills/` and add
wrappers for each tool. Never duplicate the instructions.

## Path-scoped instructions

`.github/instructions/*.instructions.md` files carry an `applyTo` glob in
frontmatter and are injected by Copilot only when a matching file is in context:

```markdown
---
applyTo: "src/SharpForge.Client/Content/Blog/**/*.md"
---
```

These must stay thin and point at the canonical skill files, for the same reason
the tool wrappers do.

## Automated drafting

`.github/workflows/draft-blog-post.yml` runs the `write-blog-post` skill when an
issue is labelled `topic:approved`, then opens a draft PR.

**Setup required before it will run:**

1. Add **one** of these repository secrets
   (Settings → Secrets and variables → Actions):

   | Secret | Where it comes from | Billing |
   |---|---|---|
   | `CLAUDE_CODE_OAUTH_TOKEN` | Run `claude setup-token` locally while signed in with a Claude Pro/Max subscription | Counts against subscription usage |
   | `ANTHROPIC_API_KEY` | `console.anthropic.com` | Pay-as-you-go API credit |

   A Claude Pro subscription does **not** include API credit — the two are
   billed separately. The workflow fails fast with a clear message if neither
   secret is present.

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

