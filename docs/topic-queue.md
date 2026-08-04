# Topic queue

How blog topics are captured, triaged and drafted. Read this when working the
queue or the topic scout — not needed to write a post you already have a brief
for.

Blog topics are tracked as **GitHub issues**, not as a file in the repo. This
keeps capture friction low — an idea can be filed from a phone in seconds — and
gives a single queue shared by manual ideas and the automated topic scout.

Open one with the **Blog post topic** issue form
(`.github/ISSUE_TEMPLATE/blog-post.yml`), which captures topic, **angle**,
category, post type and sources.

## Labels

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

## Lifecycle

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

## Rules

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
> categories in `.agents/skills/write-blog-post/reference.md` — a test enforces
> this.

## Topic scout

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
| **DECAY** | Overlaps an existing post which may now be misleading. Approve only if it justifies a **new superseding post** — never an edit |
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

### Schedule

`.github/workflows/topic-scout.yml` runs it **every Monday at 08:00 UTC**
(`--days 10 --min-score 3 --max-issues 5`) and writes the report to the job
summary. It can also be triggered manually via `workflow_dispatch`, including a
`dry_run` option.

Re-running is safe: the scout reads every existing `blog post` issue — open,
closed and rejected — and skips anything already queued.

