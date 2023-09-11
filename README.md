# RabbitMQServer

1. **Create** an _"appsettings.json"_ file for both projects to save user data and fill it with the appropriate structure according to the template.
```JSON
{
  "RabbitMQ": {
    "HostName": "hostname",
    "Port": 5672,
    "UserName": "username",
    "Password": "password"
  }
}
```

2. **Run** the docker command to activate the RabbitMQ server:
```Console
docker run -d --hostname rmq --name rabbit-server -p 8080:15672 -p 5672:5672 rabbitmq:3-management
```

3. **Launch** both projects separately or together.
