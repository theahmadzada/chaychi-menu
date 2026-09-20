var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("docker-compose");

var psql = builder.AddPostgres("rms")
    .WithDataVolume()
    .AddDatabase("rms-db");

var redis = builder.AddRedis("redis");

var migrations = builder.AddProject<Projects.ChaychiMenu_MigrationService>("migrations")
    .WithReference(psql)
    .WaitFor(psql);

var minio = builder.AddMinioContainer("minio")
    .WithDataVolume();

var jwtIssuer = builder.AddParameter("jwt-issuer");
var jwtAudience = builder.AddParameter("jwt-audience");
var jwtSigningKey = builder.AddParameter("jwt-signing-key", secret: true);
var jwtExpiry = builder.AddParameter("jwt-valid-for");
var refreshTokenKey = builder.AddParameter("refresh-token-key", secret: true);
var refreshTokenExpiry = builder.AddParameter("refresh-token-valid-for");

builder.AddProject<Projects.ChaychiMenu_WebApi>("webapi")
    .WithExternalHttpEndpoints()
    .WithReference(psql)
    .WaitFor(psql)
    .WithReference(migrations)
    .WaitForCompletion(migrations)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(minio)
    .WaitFor(minio)
    .WithEnvironment("JwtSettings__Issuer", jwtIssuer)
    .WithEnvironment("JwtSettings__Audience", jwtAudience)
    .WithEnvironment("JwtSettings__SigningKey", jwtSigningKey)
    .WithEnvironment("JwtSettings__ValidFor", jwtExpiry)
    .WithEnvironment("RefreshTokenSettings__ValidFor", refreshTokenExpiry)
    .WithEnvironment("RefreshTokenSettings__Key", refreshTokenKey)
    .PublishAsDockerComposeService((resource, service) =>
    {
        service.Ports = ["5100:8080"];
    });

builder.Build().Run();