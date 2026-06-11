using Microsoft.AspNetCore.Mvc;
using StudyPlanner.API.Extensions;
using StudyPlanner.API.Middleware;
using StudyPlanner.Core.Models.Common;
using StudyPlanner.Infrastructure;
using StudyPlanner.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddStudyPlannerCors(builder.Configuration);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage));

            var response = ErrorResponse.Create(
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                context.HttpContext.TraceIdentifier,
                errors);

            return new BadRequestObjectResult(response);
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddStudyPlannerSwagger();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

await DependencyInjection.InitializeDatabaseAsync(app.Services);

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Study Planner System API v1");
    options.RoutePrefix = "swagger";
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors(CorsExtensions.PolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapStudyPlannerHealthChecks();

app.Run();
