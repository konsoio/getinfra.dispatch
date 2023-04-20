# Getinfra.Dispatch

This library provides a standardized interface for working with messaging queues from different providers. It includes adapters for popular queue providers, as well as support for custom adapters.


## Installation

To install the base library, you can use the NuGet package manager:

```powershell
Install-Package Getinfra.Dispatch.Abstractions
```

Or, if you prefer, you can download the source code and build the library yourself.

## Adapters

Here is a list of supported adapters

| Name | Status | Nuget |
|---|---|---|
| RabbitMq | Ready |   |
| Kafka | Future |   |
| Azure Eventhub | Future |   |
| AWS SQS | Future |   |

### RabbitMq

To install the RabbitMq adapter, you can use the NuGet package manager:

```powershell
Install-Package Getinfra.Dispatch.RabbitMq
```

Or, if you prefer, you can download the source code and build the library yourself.

#### Usage

To use RabbitMQ adapter you should first add configuration to `appsettings.json`:

```
"Getinfra.Dispatch": {
    "RabbitMq": {
      "Publisher": {
        "Host": "rabbitmq.default.svc.cluster.local",
        "Port": 5672,
        "Username": "queue-publisher",
        "Password": "pass",
        "Exchange": "test.direct",
        "ExchangeType": "direct",
        "Queue": "test",
        "RoutingKey": "dev-key"
      }
    }
  }
```

And configure services at `Startup.cs`:

```csharp
services.ConfigureRabbitMqPublisher(new LoggerFactory(), _configuration, "Publisher"));
```
Note: Configuration name should be same as in `appsettings.json`, in this example it is "Publisher"

Then in your code you can use `IQueuePublisher`, to enqueue messages:
```csharp
// resolved from DI
IQueuePublisher publisher
// enqueue
publisher.Enqueue(new QMessage() { Body = <object> });
```
