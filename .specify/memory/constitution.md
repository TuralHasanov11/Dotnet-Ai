<!--
Sync Impact Report
- Version change: template -> 1.0.0
- Modified principles:
	- Principle 1 placeholder -> I. Code Quality as a Release Gate
	- Principle 2 placeholder -> II. Test Coverage and Reliability Standards
	- Principle 3 placeholder -> III. User Experience Consistency Requirements
	- Principle 4 placeholder -> IV. Performance Budgets and Regression Control
	- Principle 5 placeholder -> V. Simplicity, Security, and Operability
- Added sections:
	- Engineering Standards and Constraints
	- Delivery Workflow and Quality Gates
- Removed sections:
	- None
- Templates requiring updates:
	- ✅ updated: .specify/templates/plan-template.md
	- ✅ updated: .specify/templates/spec-template.md
	- ✅ updated: .specify/templates/tasks-template.md
	- ⚠ pending: .specify/templates/commands/*.md (directory not present)
- Deferred items:
	- None
-->

# Dotnet-Ai Constitution

## Core Principles

### I. Code Quality as a Release Gate
All production code MUST meet defined quality gates before merge: clear naming,
single-responsibility structure, bounded complexity, secure coding defaults, and
updated documentation when behavior changes. Code that increases incidental
complexity without explicit justification MUST be rejected.
Rationale: High quality code reduces defect density, review friction, and long-term
maintenance cost.

### II. Test Coverage and Reliability Standards
Every functional change MUST include automated tests at the appropriate level
(unit, integration, and contract where applicable). New logic MUST ship with
failing-then-passing tests for critical paths and deterministic assertions.
Flaky tests MUST be fixed or removed before release.
Rationale: Reliable automated tests are required to sustain deployment speed and
confidence.

### III. User Experience Consistency Requirements
User-facing behavior MUST remain consistent across pages, flows, and platforms.
Changes to UI or interaction patterns MUST align with existing design conventions,
accessibility requirements, and predictable navigation behavior. Any intentional
deviation MUST be documented and approved.
Rationale: Consistent UX improves usability, accessibility, and trust.

### IV. Performance Budgets and Regression Control
Features MUST define measurable performance expectations for latency,
resource usage, or throughput when relevant. Changes that materially affect
hot paths MUST include profiling evidence or benchmark validation.
Performance regressions beyond agreed thresholds MUST block release.
Rationale: Performance is a core product requirement, not a post-release
optimization task.

### V. Simplicity, Security, and Operability
Solutions MUST favor the simplest design that satisfies requirements, with
explicit security controls and operational visibility (logging, health checks,
and error diagnostics) for production behavior. Unnecessary abstraction or
tooling sprawl MUST be avoided.
Rationale: Simple and observable systems are safer to operate and faster to
evolve.

## Engineering Standards and Constraints

- Runtime and framework choices MUST remain aligned with repository standards
	and centrally managed dependencies.
- Security-sensitive data MUST NOT be hardcoded and MUST use secure configuration
	sources.
- Accessibility conformance MUST target WCAG 2.2 AA for user-facing changes.
- Feature specifications MUST define measurable success criteria, including
	quality and performance outcomes.

## Delivery Workflow and Quality Gates

1. Requirements and design MUST be documented before implementation for
	non-trivial changes.
2. Pull requests MUST pass build, tests, and required static analysis checks.
3. Reviewers MUST validate constitutional compliance explicitly in code review.
4. User-facing updates MUST include UX validation and accessibility checks.
5. Performance-impacting changes MUST include benchmark or profiling evidence.

## Governance
This constitution supersedes conflicting project conventions for engineering
decision-making.

- Amendments MUST include: rationale, impact analysis, and updates to dependent
	templates.
- Versioning policy MUST follow semantic versioning:
	- MAJOR for incompatible governance changes or principle removals.
	- MINOR for new principles/sections or materially expanded guidance.
	- PATCH for clarifications and non-semantic wording improvements.
- Compliance review MUST occur during planning and pull request review.
- Exceptions MUST be time-bound, documented, and approved by maintainers.

**Version**: 1.0.0 | **Ratified**: 2026-04-03 | **Last Amended**: 2026-04-03
