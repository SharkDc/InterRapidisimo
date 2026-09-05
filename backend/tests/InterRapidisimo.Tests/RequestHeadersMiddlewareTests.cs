using System.IO;
using System.Text.Json;
using FluentAssertions;
using InterRapidisimo.Api.Middlewares;
using InterRapidisimo.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace InterRapidisimo.Tests;

public class RequestHeadersMiddlewareTests
{
    private readonly Mock<ILogger<RequestHeadersMiddleware>> _loggerMock = new();

    [Fact]
    public async Task InvokeAsync_Should_Fail_When_SystemId_Is_Missing()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/courses";
        context.Request.Method = "GET";
        context.Response.Body = new MemoryStream();

        // Falta systemid, solo se envían uuid y timestamp
        context.Request.Headers["uuid"] = Guid.NewGuid().ToString();
        context.Request.Headers["timestamp"] = DateTime.UtcNow.ToString("o");

        bool nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new RequestHeadersMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(400);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseText, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        apiResponse.Should().NotBeNull();
        apiResponse!.Estado.Should().BeFalse();
        apiResponse.Descripcion.Should().Contain("systemid");
    }

    [Fact]
    public async Task InvokeAsync_Should_Fail_When_Uuid_Is_Missing_Or_Invalid()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/students";
        context.Request.Method = "GET";
        context.Response.Body = new MemoryStream();

        context.Request.Headers["systemid"] = "inter-rapidisimo-web";
        context.Request.Headers["uuid"] = "not-a-valid-guid";
        context.Request.Headers["timestamp"] = DateTime.UtcNow.ToString("o");

        bool nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new RequestHeadersMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(400);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        responseText.Should().Contain("uuid");
    }

    [Fact]
    public async Task InvokeAsync_Should_Fail_When_Timestamp_Is_Missing_Or_Invalid()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/students";
        context.Request.Method = "GET";
        context.Response.Body = new MemoryStream();

        context.Request.Headers["systemid"] = "inter-rapidisimo-web";
        context.Request.Headers["uuid"] = Guid.NewGuid().ToString();
        context.Request.Headers["timestamp"] = "fecha-invalida";

        bool nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new RequestHeadersMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(400);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        responseText.Should().Contain("timestamp");
    }

    [Fact]
    public async Task InvokeAsync_Should_Succeed_When_All_Headers_Are_Valid()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/courses";
        context.Request.Method = "GET";
        context.Response.Body = new MemoryStream();

        var validGuid = Guid.NewGuid().ToString();
        var validTimestamp = DateTime.UtcNow.ToString("o");

        context.Request.Headers["systemid"] = "inter-rapidisimo-web";
        context.Request.Headers["uuid"] = validGuid;
        context.Request.Headers["timestamp"] = validTimestamp;

        bool nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new RequestHeadersMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Items["SystemId"].Should().Be("inter-rapidisimo-web");
        context.Items["CorrelationId"].Should().Be(validGuid);
        context.Items["Timestamp"].Should().Be(validTimestamp);
    }

    [Fact]
    public async Task InvokeAsync_Should_PassThrough_For_Swagger_And_Non_Api_Routes()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/swagger/index.html";
        context.Request.Method = "GET";

        bool nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new RequestHeadersMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }
}
