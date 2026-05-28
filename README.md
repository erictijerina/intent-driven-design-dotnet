# Intent-Driven Design — .NET Skeleton

A compilable, runnable reference skeleton for the [Intent-Driven Design (IDD)](https://medium.com/@tijerina.eric/intent-driven-design-net-9aa6c839aadf) architecture pattern. Uses `Acme` as the placeholder company and `Order` as the placeholder aggregate domain.

This repo is the **hydration target** for solutions migrating to IDD. Replace the placeholder names with your domain and layer in real business logic.

---

## Solution Structure

```
Acme.Domain/
  Abstractions/          IIntentRuntime, IEventRuntime
  Core/
    IntentDispatchers/   IntentDispatcher (static AsyncLocal)
    EventPublishers/     EventPublisher (static AsyncLocal)
  Order/                 Aggregate root + static facade
    Commands/            PlaceOrder, SendReceipt
    Queries/             GetOrder
    Events/              OrderPlaced
    Workflows/           ProcessOrder

Acme.Application/
  Dtos/Order/            Request/response shapes (never in Api)
  UseCases/Order/
    Orchestration/       Tier-2 handlers (invariants, multi-step, events)
    Events/              Domain event handlers
  PipelineBehaviors/     LoggingBehavior
  Contracts/             IOrderStore

Acme.Infrastructure/
  IntentRuntimes/        MediatRIntentRuntime
  EventRuntimes/         MediatREventRuntime
  Database/Sql/          EF Core (SQLite), Tier-1 query/command handlers
  Database/Mongo/        MongoDB, Tier-1 read handlers (OrderHistory)
  Messaging/Email/       Email sender, Tier-1 command handler
  Messaging/Outbox/      Transactional outbox
  BackgroundWorkers/     Outbox dispatcher

Acme.Api/
  Controllers/           Thin controllers — map DTOs, call aggregate facade, return results
  Configuration/
    ApplicationBuilderExtensions.cs   UseApiPipeline (inline runtime middleware)
    CoreServicesConfigurator.cs       DI registration
```

---

## Core IDD Patterns

### Static Facade Aggregate

Aggregates are the single entry point for all domain operations. Each intent category (Command, Query, Event, Workflow) maps to a static method that delegates to `IntentDispatcher` or `EventPublisher`:

```csharp
// Domain/Order/Order.cs
public static Task<Order> PlaceOrder(PlaceOrder intent, CancellationToken ct = default)
    => IntentDispatcher.SendAsync(intent, ct);

public static Task OrderPlaced(OrderPlaced @event, CancellationToken ct = default)
    => EventPublisher.RaiseAsync(@event, ct);
```

Controllers call the facade directly — no injected services, no handler references:

```csharp
var result = await Order.PlaceOrder(intent, ct);
```

### AsyncLocal Runtime Dispatch

`IntentDispatcher` and `EventPublisher` are static classes backed by `AsyncLocal<T>`. They are configured per-request in inline middleware and cleared in the `finally` block — not standalone middleware classes:

```csharp
// Api/Configuration/ApplicationBuilderExtensions.cs
app.Use(async (context, next) =>
{
    IntentDispatcher.Configure(context.RequestServices.GetRequiredService<IIntentRuntime>());
    EventPublisher.Configure(context.RequestServices.GetRequiredService<IEventRuntime>());
    try { await next(); }
    finally
    {
        IntentDispatcher.Configure(null);
        EventPublisher.Configure(null);
    }
});
```

### Tier Routing

| Tier | Layer | Purpose |
|---|---|---|
| Tier-1 | Infrastructure | Simple reads/writes — no business logic, no events |
| Tier-2 | Application/Orchestration | Invariants, domain events, multi-step coordination |

Tier-2 handlers live in `UseCases/{Aggregate}/Orchestration/`. Tier-1 handlers live under `Infrastructure/Database/` or `Infrastructure/Messaging/`.

---

## Hydration Guide

Hydration = replacing the placeholder domain with your real domain. The skeleton compiles and runs as-is; replace as you go.

### Step 1 — Rename the company prefix

Replace `Acme` with your company/solution name across all projects, namespaces, and the `.sln` file.

### Step 2 — Rename or replace the Order aggregate

`Order` is the placeholder aggregate. Either:
- **Rename** it to your first real aggregate (find/replace `Order` in the Domain, Application, Infrastructure, and Api projects), or
- **Delete** it and add your aggregates following the same structure

### Step 3 — Add your aggregates

For each aggregate, follow this checklist:

```
Domain/{Aggregate}/
  {Aggregate}.cs              — aggregate root with static facade methods
  {Aggregate}Status.cs        — status enum if needed
  Commands/                   — IRequest<T> records
  Queries/                    — IRequest<T> records
  Events/                     — INotification records
  Workflows/                  — IRequest<T> records (multi-step, cross-aggregate)

Application/
  Dtos/{Aggregate}/           — raw input DTOs, response DTOs
  UseCases/{Aggregate}/
    Orchestration/            — Tier-2 IRequestHandler<,> implementations
    Events/                   — INotificationHandler<> implementations
  Contracts/                  — I{Aggregate}Store interface

Infrastructure/
  Database/.../Handlers/{Aggregate}/   — Tier-1 handlers
```

### Step 4 — Wire up DI

In `CoreServicesConfigurator`, register your stores and infrastructure services. The runtime registrations (`IIntentRuntime`, `IEventRuntime`, MediatR) are already correct — do not change them.

### Step 5 — Replace placeholder business logic

`PlaceOrderHandler`, `ProcessOrderHandler`, and the infrastructure handlers contain minimal placeholder logic. Replace with real invariants, persistence calls, and event coordination.

---

## Conventions to Preserve

- **DTOs belong in `Application/Dtos/`** — never in the Api/Domain projects
- tbd..