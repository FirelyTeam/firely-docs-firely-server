---
name: customer-release-notes
description: Turn an internal Firely Server or Firely Auth developer changelog (Keep a Changelog style, with VONK-#### tickets, PR numbers, and Added/Changed/Fixed/Removed/Security/Dependencies sections) into the customer-facing release notes - releasenotes/releasenotes_vX.rst for Firely Server, security/firely-auth/firely-auth-releasenotes.rst for Firely Auth. Use whenever asked to write, update, or "generate" release notes for a Firely Server or Firely Auth release from developer notes, a changelog, or an "Unreleased" section.
---

# Customer release notes from developer notes

Converts the internal changelog for a release into an entry in this repo's customer-facing docs
(built with Sphinx/RST).

The two documents serve different audiences and must stay separate:
- **Developer changelog** (input, usually pasted by the user, not in this repo): every PR, tagged
  `VONK-####` / `#PR`, includes internal refactors and "no behavioural change" items.
- **Customer release notes** (output, this repo): only what a customer or plugin author needs to
  know to use or upgrade the product. No ticket/PR numbers, no internal implementation detail.

## Step 0 — Which product?

Two products use this skill, and they have **different target files, section headings and anchor
conventions**. Settle which one before reading anything else. If the request does not say, ask —
do not infer it from the content, because a Firely Auth change (a new setting, a login fix) reads
much like a Firely Server one.

| | Firely Server | Firely Auth |
|---|---|---|
| Input | `RELEASE_NOTES.md` in `FirelyTeam/Vonk` | `RELEASE_NOTES.md` in `FirelyTeam/Firely.Auth` |
| Target | `releasenotes/releasenotes_vX.rst` (one file per major) | `security/firely-auth/firely-auth-releasenotes.rst` (one file, all versions) |
| Anchor | `.. _vonk_releasenotes_X_Y_Z:` (underscores) | `.. _firelyauth_releasenotes_X.Y.Z:` (**dots**) |
| Sections | `Improvements`, `Features`, `Fix`/`Fixed`, `Database`, `Security`, `Programming API changes and plugins`, `Known behavioral changes` | `Feature`, `Fix`, `Configuration`, `Security`, `Database` |
| Plugin/API section | yes (Step 5) | no — skip Step 5 |

Both inputs are produced by a `cut-release-notes` skill in the respective product repo, which folds
that release's `docs/releases/vnext/` fragments into a versioned section. Ask for that section
rather than working from raw PR titles.

Everything below applies to both products unless a step says otherwise.

## Step 1 — Read the current conventions before writing anything

**Firely Auth:** read the top of `security/firely-auth/firely-auth-releasenotes.rst`. Entries are
newest-first directly under the `Release notes` title, and there is one file for all versions — no
per-major split, so Step 6.3 never applies. Its section headings are `Feature`, `Fix`,
`Configuration`, `Security` and `Database` under `^^^^`; put a new or changed setting under
`Configuration` rather than borrowing `Improvements` from the Firely Server file. Read the 2-3 most
recent entries for voice, then pick up the shared conventions in the Firely Server list below —
underline style, `#.` lists, double backticks and the `.. attention::` placement are the same. The
one thing that is *not* the same is the anchor: dots, not underscores.

**Firely Server:** read the top of `releasenotes/releasenotes_vX.rst` for the highest existing `X` (check
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

### Firely Auth specifics

Firely Auth is deployed and operated rather than called from application code, so "customer-visible"
means visible to whoever runs it: in configuration, in the login/authorization flow, or in the log.
Three things carry disproportionate weight and are easy to under-report:

- **A changed default.** Say what the new default is and how to restore the previous behaviour,
  naming the setting and the file it belongs in (`appsettings.instance.json` /
  `logsettings.instance.json`). This is `Configuration` section material, and a default that
  changes silently on upgrade is `.. attention::` material.
- **A change in log volume.** Deployments forward logs to metered sinks (Application Insights,
  Splunk, Seq, CloudWatch, Elasticsearch), so more lines costs money. Give a rough magnitude —
  "roughly two extra lines per token request" — not just "more logging", and give the snippet that
  turns it off.
- **What is and is not in those logs.** When a release starts logging anything near tokens,
  credentials or user identifiers, state explicitly what does *not* appear. "Firely Auth now logs
  token issuance events" reads as a security regression to a reviewer unless the note says in the
  same breath that token values are obfuscated. Silence here gets escalated back as a support
  question.

Do not carry `Duende.IdentityServer.*` internals into the note as explanation. Name a Duende type
only when the operator has to type it — a log category they configure, a setting they set.

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

## Step 5 — Plugin/API section (Firely Server only)

Firely Auth has no plugin API and no such section in its release notes — skip this step.

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
   at the top — for Firely Server right after the file's intro `.. note::` block, for Firely Auth
   directly under the `Release notes` title).
3. *Firely Server only.* If this is a new major version with no existing file, create
   `releasenotes/releasenotes_vX.rst` titled "Current Firely Server release notes (vX.x)", and
   re-title the previous major's file from "Current" to "Old" — check `index.rst` /
   `releasenotes/releasenotes.rst` toctrees for whether the new file needs to be added there.
   Firely Auth keeps every version in one file, so this never applies.
4. Check the underline. It must be at least as long as the title, and appending a date to a
   heading you drafted earlier without a date lengthens the title — extend the `-----` to match or
   Sphinx warns and the heading may not render as one.
5. Base the branch on this repo's **`develop`** and open the PR against `develop`. The README's
   "send a pull request to our master branch" line is aimed at outside contributors — the
   maintainers' convention for release notes is `develop`. `master` is what the published site
   builds from, and the two branches do drift (in September 2026 `master` carried a Firely Auth
   release entry that `develop` did not), so do not "fix" a missing entry by adding it to `master`:
   that is the drift, not the remedy. Add it to `develop` and raise the divergence separately.

## Step 7 — Re-check for stragglers

Before finishing, re-scan the *entire* original input once more specifically for: a trailing
`Dependencies` section (only surface a runtime/CVE-relevant line from it, not the full package
list), a `Removed`/`Security` section (easy to miss if empty or below the fold), and any items the
user supplies in a follow-up message correcting or extending an earlier paste — diff against what's
already in the drafted section rather than assuming the first pass was complete.
