# E-Commerce Microservices Project

This project represents an e-commerce platform developed using microservices architecture. The project aims to create an
API network using various technologies.

## ⚠️ Development in Progress

**Note:** This project is currently under active development, and the development process is ongoing. Please be aware
that some features may not be fully implemented, and you may encounter bugs or incomplete functionality.

## Technologies

This project uses the following technologies:

- ASP.NET Core
- gRPC
- IdentityServer
- RabbitMQ
- SQL Server Database
- API Gateway with Ocelot
- Docker

## Architecture

The project is divided into the following microservices:

- **API Gateway**: This service is responsible for routing requests to the appropriate microservice. It is implemented
  using Ocelot.
- **Identity Service**: This service is responsible for user authentication and authorization. It is implemented using
  IdentityServer.
- **Product Service**: This service is responsible for managing products. It is implemented using ASP.NET Core and gRPC.
- **Order Service**: This service is responsible for managing orders. It is implemented using ASP.NET Core and gRPC.
- **Basket Service**: This service is responsible for managing baskets. It is implemented using ASP.NET Core and gRPC.
- **Message Broker**: This service is responsible for managing messages between microservices. It is implemented using
  RabbitMQ.
- **Database**: This service is responsible for managing the database. It is implemented using SQL Server.

## How to run

### Docker

To run the project using Docker, run the following command:

```bash
docker-compose up
```




