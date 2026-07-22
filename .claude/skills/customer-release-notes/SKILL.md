---
name: customer-release-notes
description: Turn the internal Firely Server developer changelog (Keep a Changelog style, with VONK-#### tickets, PR numbers, and Added/Changed/Fixed/Removed/Security/Dependencies sections) into the customer-facing release notes in releasenotes/releasenotes_vX.rst. Use whenever asked to write, update, or "generate" release notes for a Firely Server release from developer notes, a changelog, or an "Unreleased" section.
---

# Customer release notes from developer notes

Converts the internal `firely-server` repo changelog for a release into an entry in this repo's
`releasenotes/releasenotes_vX.rst` (customer-facing docs, built with Sphinx/RST).

The two documents serve different audiences and must stay separate:
- **Developer changelog** (input, usually pasted by the user, not in this repo): every PR, tagged
  `VONK-####` / `#PR`, includes internal refactors and "no behavioural change" items.
- **Customer release notes** (output, this repo): only what a customer or plugin author needs to
  know to use or upgrade Firely Server. No ticket/PR numbers, no internal implementation detail.

## Step 1 — Read the current conventions before writing anything

Read the top of `releasenotes/releasenotes_vX.rst` for the highest existing `X` (check
`releasenotes/` for the file whose title starts with "Current Firely Server release notes"; older
majors are titled "Old Firely Server release notes (vY.x)"). Look at the 2-3 most recent release
entries to (re-)learn:
- Which section headings this project actually uses, and in what order. Common ones, use only the
  ones a given release needs — most releases don't need all of them:
  `Improvements`, `Features`, `Fix`/`Fixed`, `Database`, `Security`,
  `Programming API changes and plugins`, `Known behavioral changes`.
- Heading underline style (`^^^^` under each section, `----` under the release title, `====` for
  the file's top title), and that the release title underline must be at least as long as the title.
- List style: `#.` auto-numbered RST lists (not `-` or `*`), double backticks for
  code/settings/operation names (`` ``$everything`` ``), single backticks for `:ref:` targets.
- The anchor convention: `.. _vonk_releasenotes_X_Y_Z:` immediately above each release heading.
- That breaking/must-notice changes get a `.. attention::` directive directly under the release
  intro, above the section headings.
- Whether a doc page already exists for a feature being released (grep for a plausible `:ref:`
  label, e.g. `feature_customresources`) — link it with `:ref:`\`label\` instead of re-explaining it
  from scratch.

If unsure which release this is or its date, ask the user rather than guessing — don't invent a
date.

## Step 2 — Triage every entry in the dev changelog

Go through **every** section of the input, including ones that are easy to skim past:
`Added`, `Changed`, `Fixed`, `Removed`, `Security`, `Dependencies`, and any curated
"User-facing changes" or "Plugin/Breaking Developer Notes" block at the top — that curated block is
a strong signal of what the author already considers customer-facing, but it is not exhaustive;
still read the detail sections below it.

For each entry, decide:

**Include** if it's:
- A new customer-visible feature or operation/parameter support.
- A behavior change visible in requests/responses/config/logs (including changed defaults).
- A real bug fix (wrong output, crash, incorrect status code) — even one filed under "Changed" in
  the dev notes if the net effect is "this was broken and now it's right".
- A performance improvement substantial enough to be worth telling a customer about (large
  result sets, request/response throughput), even if the dev notes say "no behavioural change" —
  rephrase around the *impact*, not the mechanism.
- A new/changed configuration setting.
- A runtime/SDK/major dependency upgrade that affects deployment (e.g. .NET version bump, Docker
  base image, minimum supported database version) — customers and plugin authors need to know
  even without a CVE attached.
- A security fix / CVE.
- Anything that changes a public plugin API, deprecates it, or removes it.

**Exclude** if it's:
- Marked "internal" or "not yet wired into X" (groundwork with no observable effect yet).
- An internal refactor explicitly marked "no behavioural change" with **no** plugin-visible surface
  (e.g. moving one internal bundle-building code path to another, caching an internally-computed
  list, adopting Central Package Management).
- A fix to a component customers don't run in production (e.g. an in-memory/test-only provider).
- CI/tooling/test-only/docs-only work.
- Digest-only Docker refreshes, or per-package dependency bumps with no behavior change and no CVE.

**Always strip**: `VONK-####` ticket refs and `#PR` numbers — these never appear in customer notes.

## Step 3 — Rewrite, don't copy

Dev-note prose explains *how* the code changed. Customer-note prose explains *what the customer
sees change*. Rewrite each kept item:
- Lead with the observable behavior/outcome, not the internal mechanism ("no longer walks the
  full POCO tree" → "large result sets such as `Patient/$everything` are noticeably faster").
- Keep FHIR resource/operation/parameter/setting names verbatim in double backticks — this
  audience is technical.
- Collapse multi-paragraph dev explanations (which often justify *why* a PR did something a
  certain way) into one or two sentences about the result.
- Preserve genuinely useful caveats (e.g. "can be disabled via `X` setting, but disabling it has
  a performance cost") — these are customer-relevant trade-offs, not implementation detail.

## Step 4 — Flag breaking changes

Anything that changes wire behavior a client parses by name/shape (renamed fields, relations,
changed default response content) or that requires a customer/plugin action on upgrade (runtime
bump, recompilation, config migration) gets a `.. attention::` block right under the release's
intro paragraph — don't bury it in a numbered list where it can be skimmed past.

## Step 5 — Plugin/API section

If the dev notes include a "Plugin Developer Notes" section or any deprecated/removed public API,
mirror it under a `Programming API changes and plugins` heading, in the same voice as this repo's
existing examples (e.g. the 6.0.0 and 6.9.0 entries): one bullet per API change, naming the old and
new API, with brief migration guidance. Also fold in here: SDK version bumps ("recompile plugins
against vX"), and runtime/target-framework upgrades ("recompile targeting netX.0").

## Step 6 — Assemble and place

1. Build the release entry: anchor, title (`Release X.Y.Z, Month Dayth, YYYY`) with matching
   underline, optional intro paragraph, `.. attention::` block(s), then only the section headings
   this release actually needs, in the order Step 1 established.
2. Insert it directly above the previously-latest release entry in the same file (new releases go
   at the top, right after the file's intro `.. note::` block).
3. If this is a new major version with no existing file, create `releasenotes/releasenotes_vX.rst`
   titled "Current Firely Server release notes (vX.x)", and re-title the previous major's file from
   "Current" to "Old" — check `index.rst` / `releasenotes/releasenotes.rst` toctrees for whether the
   new file needs to be added there.

## Step 7 — Re-check for stragglers

Before finishing, re-scan the *entire* original input once more specifically for: a trailing
`Dependencies` section (only surface a runtime/CVE-relevant line from it, not the full package
list), a `Removed`/`Security` section (easy to miss if empty or below the fold), and any items the
user supplies in a follow-up message correcting or extending an earlier paste — diff against what's
already in the drafted section rather than assuming the first pass was complete.
