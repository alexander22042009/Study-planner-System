using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using StudyPlanner.API.Middleware;
using StudyPlanner.Core.Exceptions;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Tests.Middleware;

[TestFixture]
public class ExceptionHandlingMiddlewareTests
{
    [Test]
    public async Task InvokeAsync_WhenNotFoundException_Returns404Json()
    {
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new NotFoundException("Item missing"),
            Mock.Of<ILogger<ExceptionHandlingMiddleware>>());

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));

        context.Response.Body.Position = 0;
        var body = await JsonSerializer.DeserializeAsync<ErrorResponse>(context.Response.Body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.That(body!.Message, Is.EqualTo("Item missing"));
    }
}
