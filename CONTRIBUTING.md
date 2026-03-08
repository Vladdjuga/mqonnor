# Contributing to mqonnor

Thank you for your interest in contributing. This document outlines the conventions and process to follow.

---

## Getting started

1. Fork the repository and clone your fork
2. Create a branch from `main` using the naming convention below
3. Make your changes
4. Ensure the project builds and all tests pass
5. Open a pull request against `main`

---

## Branch naming

| Type | Pattern | Example |
|---|---|---|
| Feature | `feat/<short-description>` | `feat/kafka-event-bus` |
| Bug fix | `fix/<short-description>` | `fix/channel-cancellation` |
| Refactor | `refactor/<short-description>` | `refactor/result-pattern` |
| Docs | `docs/<short-description>` | `docs/readme-update` |
| Chore | `chore/<short-description>` | `chore/update-mongo-driver` |

---

## Commit messages

Follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <short summary>

[optional body]

[optional footer]
```

**Types:** `feat`, `fix`, `refactor`, `docs`, `test`, `chore`, `perf`

**Examples:**
```
feat(messaging): add RabbitMQ IEventBus implementation
fix(worker): handle OperationCanceledException on shutdown
refactor(domain): make EventMetainfo a readonly record struct
```

---

## Code conventions

**Architecture**
- Respect the dependency rule: `Domain` ← `Application` ← `Infra` / `API`. Domain and Application must never reference Infra.
- New infrastructure providers (bus, persistence) go in `mqonnor.Infra` and implement interfaces defined in `Application` or `Domain`.
- New use cases go in `mqonnor.Application/UseCases/<Entity>/` with command and handler in one file.

**C# style**
- Use primary constructors for dependency injection
- Prefer `sealed` on concrete classes unless inheritance is intentional
- `internal` on Infra types that should not be referenced from outside the assembly
- Use `Result<T>` as return type from all command handlers — no exceptions across use-case boundaries
- `ValueTask` for hot-path async methods, `Task` for infrequent operations
- No logic in constructors

**Naming**
- Commands: `<Action><Entity>Command` (e.g. `PublishEventCommand`)
- Handlers: `<Action><Entity>CommandHandler`
- Repository implementations: `<Entity>Repository`
- Bus implementations: `<Provider>EventBus`
- DI extension files: `<Feature>Extensions.cs` in `API/DI/`

---

## Adding a new bus provider

1. Create `src/mqonnor.Infra/Messaging/<Provider>EventBus.cs` implementing `IEventBus`
2. Install the required NuGet package in `mqonnor.Infra`
3. Add a registration branch in `InfrastructureExtensions.cs` driven by `Bus:Provider` config
4. Document the new provider and its config keys in `README.md`

## Adding a new persistence provider

1. Create `src/mqonnor.Infra/Persistence/<Provider>/` with a repository implementing `IEventRepository`
2. Add internal document/entity models and mappers implementing `IMapper<TSource, TDest>`
3. Register via `InfrastructureExtensions.cs`
4. Document connection config in `README.md`

---

## Pull request checklist

- [ ] Branch follows the naming convention
- [ ] Commits follow Conventional Commits
- [ ] `dotnet build` passes with no errors
- [ ] No new warnings introduced
- [ ] Architecture dependency rules are respected
- [ ] Public-facing interfaces in Application/Domain have XML doc comments if non-obvious
- [ ] `README.md` updated if configuration, endpoints, or project structure changed

---

## Reporting issues

Open a GitHub issue with:
- A clear description of the problem or feature request
- Steps to reproduce (for bugs)
- Expected vs actual behaviour
- .NET version and OS

---

## License

By contributing you agree that your contributions will be licensed under the [MIT License](LICENSE).
