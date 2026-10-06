# Copilot Instructions

All project instructions for this repository live in **[`AGENTS.md`](../AGENTS.md)**.

Read `AGENTS.md` before making any change. It is a deliberately small router: it
covers the project overview, commit conventions and the immutability rule, then
points at detail that loads only when needed:

- `.agents/skills/write-blog-post/` — authoring a post (skill + reference)
- `docs/topic-queue.md` — topic queue and the topic scout
- `docs/automation.md` — agent skills, path-scoped instructions, CI

Path-scoped rules live in `.github/instructions/*.instructions.md` and are
injected automatically when a matching file is in context.

Do not duplicate guidance into this file — update `AGENTS.md` or the document it
routes to instead. `AGENTS.md` is prepended to every request, so keep it small;
a test enforces the cap.
