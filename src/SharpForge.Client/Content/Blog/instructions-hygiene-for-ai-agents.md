---
title: "Instructions Hygiene for AI Agents"
category: "AI"
date: "October 6, 2026"
readTime: "6 min read"
excerpt: "Microsoft's .NET team argues that agent instruction files should carry only what a frontier model cannot discover itself. Here is how to apply that, and why a lean file is a cost lever as much as a quality one."
tags: ["AI Agents", "Instructions", "Context Engineering", "Cost"]
sidebar:
  - href: "#context-is-a-budget"
    text: "Context Is a Budget"
  - href: "#what-models-still-need"
    text: "What Models Still Need"
  - href: "#what-to-cut"
    text: "What to Cut"
  - href: "#a-compact-file"
    text: "A Compact File"
  - href: "#why-this-is-a-cost-argument"
    text: "The Cost Argument"
  - href: "#keep-remove-move-verify"
    text: "Keep, Remove, Move, Verify"
  - href: "#conclusion"
    text: "Conclusion"
---

Most repositories that use an AI coding agent have an instruction file, and most of those files only ever grow. Someone hits a bad output, adds a paragraph, and nobody deletes anything. Wendy Breiding's recent post on the .NET Blog, [Instructions Hygiene – What Frontier Models Still Need You to Say](https://devblogs.microsoft.com/dotnet/instructions-hygiene-what-frontier-models-still-need-you-to-say/), argues the opposite discipline: write less, and only what the model cannot work out for itself. This post summarises that argument and adds our own take on what it means for what you spend.

## Context Is a Budget

The core idea is that every line in an instruction file competes for the model's attention with your code and the conversation so far. A line is not free just because it is true.

The test the article proposes is a single question: what does the model need to know that it cannot reliably discover itself? If it can read the answer from the code, the build output or a tool's error message, the line is noise. If it cannot, the line earns its place.

That framing also explains why advice written for older models ages badly. The article's premise is that modern frontier models need less procedural hand-holding than their predecessors, so scaffolding that once helped can now just take up room.

## What Models Still Need

The article names five categories of content that are still worth writing down:

1. **Non-obvious system facts.** Architecture boundaries, ownership rules, and which components are still production-critical.
2. **The shortest reliable validation path.** The authoritative command, rather than the obvious-but-incomplete one.
3. **Intentional codebase choices.** Local decisions such as the testing framework, API patterns or error-handling conventions.
4. **Hard constraints.** Anything phrased as "never" or "always" — security, compatibility and compliance rules.
5. **Where the source of truth lives.** A pointer to focused documentation instead of a copy of it.

Notice what these have in common. None of them is a general statement about how to write software. Each is a fact about *this* repository that is expensive or impossible to infer by reading files.

The validation path deserves particular attention, because it is where an agent most often wastes effort. If `dotnet build` passes but the real gate is a content test suite, the agent will declare victory early. One line naming the real command prevents that:

```bash
dotnet test tests/Shipping.Contracts.Tests/Shipping.Contracts.Tests.csproj
```

## What to Cut

The article is equally specific about what to leave out:

- Generic software engineering advice.
- Exhaustive directory listings.
- Rules that a tool already enforces.
- Documentation duplicated from elsewhere.
- "Prompt folklore" such as telling the model to take a deep breath.
- Workarounds for problems that no longer exist.

The third item is the easiest to act on. If a formatter or analyzer already fails the build on a style violation, an instruction saying "use file-scoped namespaces" adds nothing — the agent will find out the first time it builds. Let the tool be the rule, and spend your tokens on things no tool can say.

```text
// ❌ BAD: enforced by tooling anyway, and generic
- Write clean, readable, well-tested code.
- Use file-scoped namespaces.
- Source lives in src/, tests live in tests/, docs live in docs/.

// ✅ GOOD: a fact the model cannot discover
- Carrier adapters must never call the tracking API directly; go through
  ICarrierGateway so rate limiting applies.
```

## A Compact File

The article includes a sample repository instructions file of around 30 lines, covering project context, engineering decisions, validation commands, constraints and documentation references, with no generic advice and no procedural scaffolding. We have not reproduced it here. This is our own illustration of the same shape, using a made-up logistics service:

```markdown
# AGENTS.md

## Context
Shipment tracking API. ASP.NET Core, PostgreSQL. The Legacy.Billing project
is still production-critical; do not refactor it as part of other work.

## Decisions
- Tests use xUnit. Integration tests run against a real database, not mocks.
- Errors cross the API boundary as ProblemDetails, never raw exceptions.

## Validate
- `dotnet test Shipping.sln` is the gate. A passing build alone is not enough.

## Never
- Never log tracking numbers; they are customer data.
- Never edit applied migrations; add a new one.

## Read more
- Carrier integration details: docs/carriers.md
- Release process: docs/release.md
```

Every entry is either a hard constraint, a local decision, a command or a pointer. The "Read more" section is the important one: it tells the agent where to look *if* the task needs it, so the detail is loaded on demand rather than on every request.

## Why This Is a Cost Argument

Here we go beyond the article, so treat the next paragraphs as our opinion rather than something Microsoft claims.

Instruction files are usually sent with every request. A bloated file is therefore a recurring cost, paid on every task regardless of whether the task needs any of it. A lean file reduces that fixed overhead, and it leaves more of the context window for the code the agent is actually working on.

There is a second, less obvious benefit. When the essential context is short, specific and verified, a smaller task is more likely to succeed on a less expensive model, because the model is not being asked to infer your conventions from scratch. We think the practical move is to try your routine tasks — dependency bumps, test additions, small fixes — against a cheaper tier with a tight instruction file, and only escalate to the most capable model when the cheaper one demonstrably fails. We have not benchmarked this, and the source article does not make a model-selection claim, so measure it on your own repository before you build a policy on it.

The reasoning matters for adoption too. Teams keep using AI tooling when it is cheap enough that nobody has to justify each run. Trimming the overhead is one of the few levers that costs engineering time once and pays back continuously.

## Keep, Remove, Move, Verify

The article recommends reviewing instructions with a four-way pass rather than only ever appending:

- **Keep** what the model cannot discover and would get wrong.
- **Remove** what it already does correctly, or what a tool enforces.
- **Move** repository-wide rules into the main file, and domain-specific guidance into path-specific files so it only loads where relevant.
- **Verify** by running real tasks on a frontier model and observing what goes wrong, instead of imagining failures in advance.

The last step is the one teams skip. An instruction written to prevent a hypothetical mistake is just as costly as one that prevents a real mistake, and far less useful. A reasonable habit is to delete a line, run a representative task, and restore the line only if the output actually degrades.

The article also frames instructions as engineering maintenance rather than a permanent artifact. They deserve the same review cadence as any other file that drifts out of date when the codebase changes.

## Conclusion

Good instruction files are short because they are edited, not because the author had little to say. The standard to hold each line to is whether the model could find the answer itself.

- **Treat context as a budget.** Every line competes with your code for attention.
- **Write down only what is undiscoverable:** non-obvious system facts, the real validation command, deliberate local choices, hard constraints and pointers to sources of truth.
- **Cut generic advice, directory listings, tool-enforced rules, duplicated docs and folklore.**
- **Split by scope.** Universal rules in the repository-wide file, specialised guidance in path-specific ones.
- **Verify against real tasks,** then measure whether a cheaper model now copes before assuming you need the most expensive one.
