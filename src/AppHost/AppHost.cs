using Projects;

var builder = DistributedApplication.CreateBuilder(args);
#region SQL Server
var sqlPassword = builder.AddParameter("sql-password", secret: true);
var sqlserver = builder.AddSqlServer("sqlserver", sqlPassword)
    .WithImageTag("2022-latest")
    .WithEndpoint(port: 14330, targetPort: 1433, name: "tcp", isProxied: false)
    .WithDataVolume("sqlserver_data")
    .WithExternalHttpEndpoints()
    .WithEnvironment("MSSQL_SA_PASSWORD", sqlPassword)
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("MSSQL_AGENT_ENABLED", "True")
    .WithLifetime(ContainerLifetime.Persistent);
sqlserver.AddDatabase("keycloak-db");
#endregion
#region Keycloak
var kcDb = builder.AddParameter("KC-DB", secret: true);
var kcDbUrl = builder.AddParameter("KC-DB-URL", secret: true);
var kcDbUsername = builder.AddParameter("KC-DB-USERNAME", secret: true);
var kcDbPassword = builder.AddParameter("KC-DB-PASSWORD", secret: true);
var keycloak = builder.AddKeycloak("keycloak", 8082)
    .WithImageTag("latest")
    .WithDataVolume()
    .WithOtlpExporter()
    .WithExternalHttpEndpoints()
    .WithEnvironment("KC_DB", kcDb)
    .WithEnvironment("KC_DB_URL", kcDbUrl)
    .WithEnvironment("KC_DB_USERNAME", kcDbUsername)
    .WithEnvironment("KC_DB_PASSWORD", kcDbPassword)
    .WithReference(sqlserver)
    // .WithRealmImport("./Realms")
    .WithArgs("--features=docker,admin-fine-grained-authz,token-exchange,quick-theme")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithUrls(context =>
        {
            foreach (var u in context.Urls)
            {
                u.DisplayLocation = UrlDisplayLocation.DetailsOnly;
            }

            context.Urls.Add(
                new ResourceUrlAnnotation()
                {
                    Url = "/",
                    DisplayText = "Admin Dashboard",
                    Endpoint = context.GetEndpoint("http"),
                }
            );
        });

keycloak.WaitFor(sqlserver);
#endregion

#region Kafka
var kafka = builder.AddKafka("kafka")
    .WithEndpoint(port: 9094, targetPort: 9094, name: "external", isProxied: false)
    .WithEndpoint(targetPort: 9092, name: "internal", isProxied: false)
    .WithEndpoint(port: 9095, targetPort: 9095, name: "external-docker", isProxied: false)
    .WithImage("confluentinc/cp-kafka", "latest")
    .WithEnvironment("KAFKA_NODE_ID", "1")
    .WithEnvironment("KAFKA_PROCESS_ROLES", "broker,controller")
    .WithEnvironment("KAFKA_CONTROLLER_QUORUM_VOTERS", "1@kafka:9093")
    .WithEnvironment("KAFKA_LISTENERS", "INTERNAL://0.0.0.0:9092,EXTERNAL_DOCKER://0.0.0.0:9095,EXTERNAL://0.0.0.0:9094,CONTROLLER://0.0.0.0:9093")
    .WithEnvironment("KAFKA_ADVERTISED_LISTENERS", "INTERNAL://kafka:9092,EXTERNAL_DOCKER://host.docker.internal:9095,EXTERNAL://localhost:9094,CONTROLLER://kafka:9093")
    .WithEnvironment("KAFKA_LISTENER_SECURITY_PROTOCOL_MAP", "INTERNAL:PLAINTEXT,EXTERNAL_DOCKER:PLAINTEXT,EXTERNAL:PLAINTEXT,CONTROLLER:PLAINTEXT")
    .WithEnvironment("KAFKA_INTER_BROKER_LISTENER_NAME", "INTERNAL")
    .WithEnvironment("KAFKA_CONTROLLER_LISTENER_NAMES", "CONTROLLER")
    .WithEnvironment("KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR", "1")
    .WithEnvironment("KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR", "1")
    .WithEnvironment("KAFKA_TRANSACTION_STATE_LOG_MIN_ISR", "1")
    .WithEnvironment("CLUSTER_ID", "MkU3OEVBNTcwNTJENDM2Qk")
    .WithVolume("kafka_data", "/var/lib/kafka/data");


#endregion

#region Kafka UI

var kafkaUi = builder.AddContainer("kafka-ui", "kafbat/kafka-ui", "latest")
.WithHttpEndpoint(port: 8088, targetPort: 8080, name: "http", isProxied: false)
    .WithEnvironment("KAFKA_CLUSTERS_0_NAME", "local")
    .WithEnvironment("KAFKA_CLUSTERS_0_BOOTSTRAPSERVERS", "kafka:9092")
    .WithReference(kafka)
      .WithUrls(context =>
        {
            foreach (var u in context.Urls)
            {
                u.DisplayLocation = UrlDisplayLocation.DetailsOnly;
            }

            context.Urls.Add(
                new ResourceUrlAnnotation()
                {
                    Url = "/",
                    DisplayText = "Kafka UI Dashboard",
                    Endpoint = context.GetEndpoint("http"),
                }
            );
        });


#endregion

#region Debezium
var debezium = builder.AddContainer("debezium", "quay.io/debezium/connect", "latest")
    .WithEnvironment("BOOTSTRAP_SERVERS", "kafka:9092")
    .WithEnvironment("GROUP_ID", "banking-connect")
    .WithEnvironment("CONFIG_STORAGE_TOPIC", "connect_configs")
    .WithEnvironment("OFFSET_STORAGE_TOPIC", "connect_offsets")
    .WithEnvironment("STATUS_STORAGE_TOPIC", "connect_status")
    .WithEnvironment("CONFIG_STORAGE_REPLICATION_FACTOR", "1")
    .WithEnvironment("OFFSET_STORAGE_REPLICATION_FACTOR", "1")
    .WithEnvironment("STATUS_STORAGE_REPLICATION_FACTOR", "1")
    .WithReference(kafka)
    .WithReference(sqlserver)
    .WithHttpEndpoint(port: 8083, targetPort: 8083, name: "http", isProxied: false)
    .WithExternalHttpEndpoints();

#endregion

#region Debezium UI
var debeziumUi = builder.AddContainer("debezium-ui", "debezium/debezium-ui", "latest")
    .WithEnvironment("KAFKA_CONNECT_URIS", debezium.GetEndpoint("http"))
    .WithHttpEndpoint(port: 8085, targetPort: 8080, name: "http", isProxied: false)

    .WithExternalHttpEndpoints()
      .WithUrls(context =>
        {
            foreach (var u in context.Urls)
            {
                u.DisplayLocation = UrlDisplayLocation.DetailsOnly;
            }

            context.Urls.Add(
                new ResourceUrlAnnotation()
                {
                    Url = "/",
                    DisplayText = "Debezium Dashboard",
                    Endpoint = context.GetEndpoint("http"),
                }
            );
        });

#endregion

#region Redis
var redis = builder.AddRedis("redis")
    .WithDataVolume(isReadOnly: false, name: "redis_data")
    .WithRedisInsight(redisInsight => redisInsight.WithHostPort(8001));
#endregion


#region RabbitMQ

var rabbitMqUsername = builder.AddParameter("RABBITMQ-USERNAME", secret: true);
var rabbitMqPassword = builder.AddParameter("RABBITMQ-PASSWORD", secret: true);

var rabbitmq = builder.AddRabbitMQ("rabbitmq", rabbitMqUsername, rabbitMqPassword, 5672)
    .WithManagementPlugin(15672)  //UI on port 15672
    .WithDataVolume("stocktrading-rabbit-data")
    .WithLifetime(ContainerLifetime.Persistent);
#endregion

var orderService = builder.AddProject<OrderService>("order-service")
    .WithReference(sqlserver)
    .WithReference(rabbitmq)
    .WithExternalHttpEndpoints();


var executionService = builder.AddProject<ExecutionService>("execution-service")
.WithReference(sqlserver)
.WithReference(rabbitmq)
.WithExternalHttpEndpoints();

builder.AddProject<ApiGateway>("APIGateway")
.WithReference(orderService)
.WithReference(executionService)
.WithReference(keycloak)
.WithReference(redis);



builder.Build().Run();
