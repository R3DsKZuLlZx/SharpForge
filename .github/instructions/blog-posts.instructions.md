---
applyTo: "src/SharpForge.Client/Content/Blog/**/*.md"
description: Blog post authoring rules — loaded only when a blog post is in context.
---

# Blog post authoring

Read `.agents/skills/write-blog-post/reference.md` for the full spec
(frontmatter schema, allowed categories, anchor rules, body conventions) and
`.agents/skills/write-blog-post/SKILL.md` for the process and quality bar.

Two rules that must not be got wrong:

1. **Published posts are immutable.** Never edit, rewrite or "refresh" a post in
   this directory. Something fundamental changed → write a brand-new post that
   supersedes it. Anything less → write nothing and say so.
2. **Never hand-compute a sidebar anchor.** Write your best guess, then run
   `dotnet test tests/SharpForge.Content.Tests/SharpForge.Content.Tests.csproj`
   — it prints the real heading ids on failure.

