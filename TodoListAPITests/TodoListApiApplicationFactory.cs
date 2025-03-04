using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MySql;

namespace TodoListAPI.Tests;

public class TodoListApiApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string Database = "TodoList";
    private const string Username = "root";
    private const string Password = "MySqlRootPassword";
    private const ushort MysqlPort = 3306;

    private readonly MySqlContainer _mysqlContainer;

    public TodoListApiApplicationFactory()
    {
        _mysqlContainer = new MySqlBuilder()
            .WithImage("mysql:8.0-debian")
            .WithPortBinding(MysqlPort, true)
            .WithEnvironment("MYSQL_ROOT_PASSWORD", Password)
            .WithEnvironment("MYSQL_DATABASE", Database)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(MysqlPort))
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var host = _mysqlContainer.Hostname;
        var port = _mysqlContainer.GetMappedPublicPort(MysqlPort);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<TodoListDbContext>));

            services.AddDbContext<TodoListDbContext>(options =>
            {
                options.UseMySql($"Server={host};Port={port};Database={Database};User={Username};Password={Password};", ServerVersion.AutoDetect($"Server={host};Port={port};Database={Database};User={Username};Password={Password};"));
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _mysqlContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _mysqlContainer.DisposeAsync();
    }

}