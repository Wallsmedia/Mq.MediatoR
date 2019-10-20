# Mq.MediatoR

The simple abstract, Dependency Inversion solution for communicating between 
Application hosted MicroServices.

## Version 3.0.0
 - NetStandard 2.0 [NetCoreApp  2.2]
  
 - NetStandard 2.1 [NetCoreApp  3.0]

## Nuget packages:

### Abstractions:
- https://www.nuget.org/packages/Mq.Mediator.Abstractions

### Event Bus for Rabbit MQ
- https://www.nuget.org/packages/Mq.MediatoR.EventBus.RabbitMq
  - https://www.nuget.org/packages/Mq.Mediator.Abstractions

### "Notification" for Application MicroServices:

- https://www.nuget.org/packages/Mq.MediatoR.Notification.InMem
  - https://www.nuget.org/packages/Mq.Mediator.Abstractions

### "Request" for Application MicroServices:

- https://www.nuget.org/packages/Mq.MediatoR.Request.InMem
  - https://www.nuget.org/packages/Mq.Mediator.Abstractions


# How to use

The Mq.MediatoR implementations are based on using **Microsoft.Extensions.DependencyInjection**
dependency injection container, but they can be adopted to other types DI containers.


## Message Queue Notifications

Based on using package: Mq.MediatoR.Notification.InMem
### How to use works

## Message Queue Requests
Based on using package: Mq.MediatoR.Request.InMem
### How to use works

## Message Queue Event Bus
Based on using package: Mq.MediatoR.EventBus.RabbitMq
### How to use works



