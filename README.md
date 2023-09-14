# RabbitMQServer

This project simulates user interaction using the RabbitMQ message broker.

## Getting Started
1.  **Activate** the RabbitMQ and Redis servers:
    + **Run** docker-compose.yml file;
    + Or **Run** the docker commands:
      ```Console
      docker run -d --hostname rmq --name rabbit-server -p 8080:15672 -p 5672:5672 rabbitmq:3-management
      ```
      ```Console
      docker run -d -p 6379:6379 --name my-redis redis
      ```
      
2. **Run** the project. Now you can send requests using Swagger.

## Usage
1. This project implements the following requests:
    + **Auth**
      + Authorization - allows the user to register in the app *(validates the user request data and creates a unique token)*;
      + Login - allows the user to login in the app *(verifies user data)*
    + **Consumer**
      + Receive - a method of receiving messages from the queue
    + **Producer**
      + Send - a method of adding messages to the queue
 
2. You can check all activities by viewing the corresponding log files(auth_log.txt, consumer_log.txt, producer_log.txt).