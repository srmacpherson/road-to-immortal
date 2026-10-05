# Road to Immortal — AI Development Context

## Project

Road to Immortal is a Dota 2 progression and analytics platform.

The goal is to help players understand their performance and answer:

> **What should I work on to improve?**

The product should prioritise **useful, explainable analytics** over generic statistics or black-box predictions.

---

## Stack

* Frontend: React + TypeScript + Vite
* Charts: Recharts
* Backend: ASP.NET Core + C#
* ORM: Entity Framework Core
* Database: PostgreSQL
* Database hosting locally: Docker
* External APIs: OpenDota + Steam Web API
* IDE: Visual Studio

Architecture:

```text
Steam / OpenDota
       ↓
ASP.NET Core
       ↓
PostgreSQL
       ↓
React + TypeScript
```

---

## Engineering Principles

1. **Inspect before modifying.**
2. Make **one small milestone at a time**.
3. Preserve existing working behaviour.
4. Prefer simple, maintainable solutions over premature complexity.
5. Keep business logic out of React presentation components where practical.
6. Backend owns business rules, aggregation and analytics.
7. Frontend owns presentation, interaction and visualisation.
8. Do not introduce dependencies without a clear reason.
9. Build/test after meaningful changes.
10. Never expose or commit secrets.

Do not perform large rewrites unless explicitly requested.

---

## Important Security Rule

The Steam API key is backend-only.

Never:

* Put API keys in React/frontend code.
* Commit secrets.
* Ask the user to paste secrets into chat.
* Hard-code production credentials.

A Steam API key was previously exposed during development and should be considered compromised. If repository/security work becomes relevant, ensure the old key has been rotated and that secrets are excluded from Git.

---

## Important Steam ID Rule

Steam IDs must be treated as **strings in TypeScript/JavaScript**.

Correct:

```ts
const steamId = "76561199124533567";
```

Do not use JavaScript `number` for Steam IDs because 64-bit Steam IDs can exceed JavaScript's safe integer precision.

The backend may use C# `long`.

---

## Current Implementation

The backend currently supports:

* Steam player lookup
* OpenDota match synchronisation
* Player persistence
* Match persistence
* Hero persistence
* MMR snapshots
* Dashboard aggregation
* Recent form
* Hero performance

The frontend currently provides:

* MMR progression chart
* Overall performance
* Recent match cards
* Hero performance
* Performance snapshot
* Recent-vs-overall comparison
* Basic improvement insight
* Match synchronisation

The backend currently runs locally on:

```text
http://localhost:5184
```

The frontend normally runs on:

```text
http://localhost:5173
```

---

## Current Architecture Debt

The project works, but some logic is currently too concentrated.

### Backend

`Program.cs` contains too much endpoint, aggregation and business logic.

Gradually move meaningful business logic into appropriate services/classes.

Do not perform a large backend rewrite.

### Frontend

`App.tsx` currently contains too much dashboard logic and some analytics calculations.

Gradually extract reusable components/helpers and move business analytics to the backend where appropriate.

---

## Analytics Principles

Analytics must be:

* Evidence-based
* Explainable
* Measurable
* Conservative
* Testable

Do not claim causation from correlation.

Prefer:

> "Your recent games have a higher death average than your overall games."

Avoid:

> "You lose because you die too much."

Consider sample size before making strong recommendations.

When practical, show the evidence behind an insight.

---

## Product Direction

The planned progression is:

```text
Working dashboard
      ↓
Better analytics
      ↓
Actionable improvement recommendations
      ↓
Better visualisation
      ↓
Product polish
```

Future analytics may include:

* Recent vs overall trends
* Death/KDA trends
* Win-rate trends
* Hero trends
* MMR progression
* Strongest/weakest heroes
* Performance declines
* Improvement areas
* Prioritised recommendations

Avoid machine learning or complicated statistics until simpler explainable analytics are insufficient.

---

## Testing Direction

As analytics become more complex, add automated tests for business rules such as:

* Win/loss calculation
* Win rate
* MMR calculations
* Recent form
* Hero aggregation
* Analytics/insight rules

Keep analytics deterministic and testable.

---

## Current Milestone

**Establish a maintainable analytics structure.**

Before changing anything:

1. Inspect the relevant code.
2. Identify what currently belongs in the frontend/backend.
3. Propose the smallest sensible change.
4. Implement only that milestone.
5. Build/test it.
6. Report what changed and any remaining issues.

Do not implement the entire future analytics system at once.

---

## Agent Instructions

Treat the **repository code as the source of truth**.

Use this file for project context and constraints, not detailed code documentation.

If this file conflicts with the implementation, inspect the code and correct the context file when appropriate.

After completing a meaningful milestone, update only the **Current Milestone** and **Current Implementation** sections when necessary.

Do not rewrite this document unnecessarily.
