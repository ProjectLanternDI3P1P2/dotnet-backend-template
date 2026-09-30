# Dungeon.Contracts

Versioned Protocol Buffers contract and generated C# gRPC client/server types for
the Dungeon service.

## Installation

Configure the organisation's GitHub Packages NuGet source, then pin a released
version of the package:

```xml
<PackageReference Include="Dungeon.Contracts" Version="0.1.1" />
```

For a C# gRPC consumer, also reference `Grpc.Net.Client`, create a channel for the
Dungeon service's internal endpoint, then construct
`Dungeon.Contracts.V1.DungeonPlayerService.DungeonPlayerServiceClient`.

The original `.proto` source is included in this package under `proto/`.
