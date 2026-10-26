# Belsoft

Belsoft is an order-management backend built step by step while learning
professional .NET development. It starts as a single ASP.NET Core Web API and
grows into a cloud-deployed, identity-protected, message-driven system.

> **Status:** learning project, early stage (lesson P02).
> Endpoints are read-only and data is held in memory. There are no automated tests yet.

## Tech

- .NET 10 (LTS), C# 14
- ASP.NET Core Web API with MVC controllers
- OpenAPI document (built-in) + Swagger UI (Development only)

## Run it locally

1. Install Visual Studio 2026 with the **ASP.NET and web development** workload.
2. Open `Belsoft.slnx`.
3. Set **Belsoft.Api** as the startup project and press **F5** (profile: `https`).
4. Swagger UI opens in the browser. Alternatively, open
   `src/Belsoft.Api/Belsoft.Api.http` and click **Send request** above any request.

## Endpoints

| Method | Route               | Result                         |
|--------|---------------------|--------------------------------|
| GET    | `/api/health`       | 200 `"Healthy"`                |
| GET    | `/api/orders`       | 200 with all orders            |
| GET    | `/api/orders/{id}`  | 200 with one order, or 404     |

## Repository conventions

- Work happens on `lesson/Lxx-...` branches and reaches `master` through a pull request.
- Each lesson's merge commit is tagged `Pxx` (e.g. `P02`), so
  `compare/P01...P02` shows exactly what a lesson changed.
- No secrets in this repository: local configuration lives in user secrets or
  git-ignored `.env` files.

