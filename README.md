# mqonnor

A .NET 10 event ingestion, routing, and broadcasting service built with Clean Architecture. mqonnor acts as a unified entry point for events — accepting them over HTTP or gRPC, routing them internally, persisting them, and broadcasting them to other services or frontend clients over SignalR. The internal bus is swappable between an in-process channel (single instance) and an external broker (multi-instance), but that is an implementation detail clients never see.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

---

## Why mqonnor?

Raw message brokers (RabbitMQ, Kafka, Redis Streams) are transports — they move bytes between producers and consumers. They do not provide:

- **A client-facing entry point** — clients have no business talking directly to a broker. mqonnor exposes a stable HTTP / gRPC / SignalR interface regardless of what bus is underneath
- **Bidirectional SignalR** — the same hub that broadcasts events *out* to frontends can receive events *in* from them, making it a unified real-time channel
- **Frontend broadcasting** — brokers have no concept of a browser connection. mqonnor bridges internal events → SignalR → connected frontend clients
- **Persistence with replay** — brokers typically delete messages after consumption. mqonnor stores every event in MongoDB and can re-publish on demand
- **Metadata enforcement** — `EventMetainfo` (source, encoding, length) is validated and stored uniformly; raw brokers are untyped
- **A single deployable unit** — one container, one config, one endpoint. The broker is only relevant when scaling to multiple instances

```
External client  →  mqonnor (HTTP / gRPC / SignalR)
                        │
                  ┌─────┴──────────────────────┐
                  │  ChannelEventBus            │  ← single instance, no broker
                  │  or RabbitMqEventBus        │  ← multi-instance, broker handles fan-out
                  └─────┬──────────────────────┘
                        │
                 EventConsumerWorker
                 ├── MongoDB        (persist + replay)
                 └── SignalR hub    (broadcast to frontends / other services)
```

The bus provider is an internal scaling mechanism. Swapping it does not change what mqonnor exposes to the outside world.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        mqonnor.API                          │
│  TestController  →  IMediator  →  PublishEventCommand       │
└─────────────────────────┬───────────────────────────────────┘
                          │ ICommandHandler
┌─────────────────────────▼───────────────────────────────────┐
│                    mqonnor.Application                       │
│  PublishEventCommandHandler  →  IEventBus                   │
│                                                             │
│  Abstractions: ICommand, ICommandHandler, IMediator         │
│  Mappers:      IMapper<TSource, TDest>, EventMapper         │
│  Messaging:    IEventBus                                    │
│  DTOs:         PublishEventDto                              │
└─────────────────────────┬───────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────┐
│                      mqonnor.Domain                          │
│  Entities:    Event, EventMetainfo                          │
│  Repositories: IEventRepository                             │
│  Primitives:  Result<T>                                     │
└─────────────────────────────────────────────────────────────┘
                          ▲
┌─────────────────────────┴───────────────────────────────────┐
│                      mqonnor.Infra                           │
│  Mediator:    ConcreteMediator                              │
│  Messaging:   ChannelEventBus (System.Threading.Channels)   │
│  Persistence: EventRepository → MongoDB                     │
│  Workers:     EventConsumerWorker (BackgroundService)       │
└─────────────────────────────────────────────────────────────┘
```

### Dependency direction

```
API → Application → Domain
Infra → Application → Domain
```

Infra implements interfaces defined in Application and Domain. Neither Domain nor Application references Infra.

---

## Projects

| Project | Type | Responsibility |
|---|---|---|
| `mqonnor.Domain` | Class Library | Entities, repository interfaces, `Result<T>` primitive |
| `mqonnor.Application` | Class Library | Use cases, DTOs, mapper/mediator/bus abstractions |
| `mqonnor.Infra` | Class Library | MongoDB repository, `ChannelEventBus`, `ConcreteMediator`, background worker |
| `mqonnor.API` | Web API | HTTP controllers, DI wiring, host configuration |

---

## Request flow

```
POST /api/test/publish
        │
        ▼
  TestController
        │  IMediator.NotifyAsync<PublishEventCommand, Result>
        ▼
  ConcreteMediator  (resolves ICommandHandler from DI)
        │
        ▼
  PublishEventCommandHandler
        │  IMapper<PublishEventDto, Event>.Map(dto)
        │  IEventBus.PublishAsync(event)
        ▼
  ChannelEventBus  (bounded Channel<Event>)
        │
        ▼ (async, separate thread)
  EventConsumerWorker  (BackgroundService, await foreach)
        │  IEventRepository.AddAsync(event)
        ▼
  EventRepository → MongoDB
```

---

## Key design decisions

**In-process message bus** — `ChannelEventBus` uses `System.Threading.Channels` for zero-latency, zero-dependency event passing within a single instance. The bounded channel provides natural backpressure: `PublishAsync` suspends when full; `ConsumeAllAsync` suspends workers when empty, with no polling.

**Swappable providers** — `IEventBus` and `IEventRepository` are defined in Application/Domain. Adding a Kafka, RabbitMQ, or Redis Streams bus is a new class in `Infra/Messaging/` and a config branch in DI. External MongoDB is a connection string change — no code change required.

**Self-implemented mediator** — `ConcreteMediator` resolves `ICommandHandler<TCommand, Result>` from the DI container at dispatch time, avoiding a third-party dependency while keeping the use-case dispatch pattern.

**Result pattern** — All command handlers return `Result` or `Result<T>`. Errors are represented as values, not exceptions, across use-case boundaries.

**Embedded MongoDB** — App and MongoDB run in the same container managed by `supervisord`. This treats them as a single deployable unit. To use an external MongoDB, point `ConnectionStrings__MongoDB` at any MongoDB instance — no code changes needed.

---

## Configuration

```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017"
  },
  "MongoDB": {
    "Database": "mqonnor"
  }
}
```

Environment variable overrides (e.g. for Docker):

| Variable | Description |
|---|---|
| `ConnectionStrings__MongoDB` | MongoDB connection string |
| `MongoDB__Database` | Database name |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Production` |

---

## Running locally

**Prerequisites:** .NET 10 SDK, MongoDB (or Docker)

```bash
dotnet run --project src/mqonnor.API
```

---

## Running with Docker

```bash
docker-compose up --build
```

The container runs both the API and MongoDB via `supervisord`. Data is persisted in the `mongo_data` named volume.

---

## API

### `POST /api/test/publish`

Publishes an event into the bus.

**Request body:**
```json
{
  "payload": "<base64-encoded bytes>",
  "encoding": "UTF-8",
  "source": "my-service"
}
```

**Responses:**

| Status | Meaning |
|---|---|
| `202 Accepted` | Event accepted and queued |
| `400 Bad Request` | Invalid request body |

---

## Project structure

```
mqonnor.slnx
Dockerfile
docker-compose.yml
supervisord.conf
src/
  mqonnor.Domain/
    Entities/
      Event.cs
      EventMetainfo.cs
    Primitives/
      Result.cs
    Repositories/
      IEventRepository.cs
  mqonnor.Application/
    Abstractions/
      ICommand.cs
      ICommandHandler.cs
      IMediator.cs
    DTOs/
      PublishEventDto.cs
    Exceptions/
      CastToResultFailedException.cs
      NoSuchCommandHandlerException.cs
    Mappers/
      IMapper.cs
      EventMapper.cs
    Messaging/
      IEventBus.cs
    UseCases/
      Event/
        PublishEvent.cs
  mqonnor.Infra/
    Mediator/
      ConcreteMediator.cs
    Messaging/
      ChannelEventBus.cs
    Persistence/
      Repository/
        Mappers/
          EventToDocumentMapper.cs
          EventDocumentToDomainMapper.cs
        EventDocument.cs
        EventRepository.cs
    Workers/
      EventConsumerWorker.cs
    InfrastructureExtensions.cs
  mqonnor.API/
    Controllers/
      TestController.cs
    DI/
      MapperExtensions.cs
      MediatorExtensions.cs
      PersistenceExtensions.cs
      WorkerExtensions.cs
    Program.cs
    appsettings.json
```

---

## Roadmap

### Transport & broadcasting
- [x] **HTTP ingestion** — `POST /api/test/publish` accepts `PublishEventDto`
- [ ] **SignalR hub — ingestion** — accept `PublishEventDto` from frontend/service clients via a hub method (same pipeline as HTTP, different entry point)
- [ ] **SignalR hub — broadcasting** — push events to connected frontend clients in real time (service → frontend)
- [ ] **SignalR backplane** — fan-out events across multiple broker instances (service → service)
- [ ] **gRPC endpoint** — typed, binary service-to-service event streaming as an alternative to HTTP
- [ ] **WebSocket raw endpoint** — lightweight alternative to SignalR for constrained clients

### Bus providers
- [x] **ChannelEventBus** — in-process, bounded `Channel<Event>`, zero dependencies, single instance
- [ ] **InMemoryEventBus** — simple `ConcurrentQueue`-backed implementation, no backpressure, useful for testing and development without channel complexity
- [ ] **RabbitMQ** — `IEventBus` implementation for multi-instance deployments
- [ ] **Kafka** — high-throughput `IEventBus` implementation with consumer group support
- [ ] **Redis Streams** — lightweight alternative with built-in persistence and consumer groups
- [ ] **Provider selection via config** — switch bus provider with `Bus:Provider` setting, no code change

### Persistence
- [x] **MongoDB** — `EventRepository` backed by `MongoDB.Driver`, append-style writes
- [x] **Embedded MongoDB** — co-located with the API in one container via `supervisord`
- [ ] **External MongoDB support** — configurable via `MongoDB:Mode: External`
- [ ] **Multiple collection routing** — per-topic collections driven by `EventMetainfo.Source`

### Core
- [x] **Clean Architecture** — Domain / Application / Infra / API layers with enforced dependency direction
- [x] **Result pattern** — `Result` / `Result<T>` returned from all command handlers, no exceptions across use-case boundaries
- [x] **Self-implemented mediator** — `ConcreteMediator` resolves `ICommandHandler<TCommand, TResult>` from DI at dispatch time
- [x] **IMapper pattern** — typed, injectable mappers between DTOs, domain entities, and persistence documents
- [x] **CancellationToken propagation** — cancellation checked and threaded through bus, worker, and use case
- [x] **Background worker** — `EventConsumerWorker` (`BackgroundService`) consumes from the bus via `await foreach`

### Resilience
- [ ] **Retry policy on publish** — configurable retry with exponential backoff when the channel is full
- [ ] **Dead-letter handling** — route failed events to a dedicated collection for inspection and replay
- [ ] **Graceful drain on shutdown** — flush in-flight channel events to persistence before the process exits

### Observability
- [ ] **Structured logging** — enrich logs with `EventId`, `Source`, and `Encoding` via `ILogger`
- [ ] **OpenTelemetry tracing** — trace spans across HTTP → mediator → bus → worker → MongoDB
- [ ] **Health checks** — `/health` endpoint reporting channel saturation and MongoDB connectivity
- [ ] **Metrics** — publish/consume rate, channel depth, and processing latency via `System.Diagnostics.Metrics`

### Developer experience
- [ ] **Topic support** — named channels per topic, each with its own `IEventBus` keyed DI registration
- [ ] **Event replay API** — re-publish stored events from MongoDB back into the bus
- [ ] **Admin endpoint** — inspect channel depth and worker status at runtime

---

## Contributing

Contributions are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) for branch naming, commit conventions, architecture rules, and the pull request checklist.

---

## License

This project is licensed under the [MIT License](LICENSE).
