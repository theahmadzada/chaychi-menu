using MediatR;

using Microsoft.AspNetCore.Mvc;

using ChaychiMenu.Application.Commands.Restaurant;
using ChaychiMenu.Application.Dto;
using ChaychiMenu.Domain;
using ChaychiMenu.WebApi.Extensions;
using ChaychiMenu.WebApi.Filters;

namespace ChaychiMenu.WebApi.Endpoints;

public static class RestaurantEndpoints
{
    public static WebApplication MapRestaurantEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/restaurants");

        group.MapPost("/", async (
            [FromBody] CreateRestaurantDto request,
            HttpContext httpContext,
            ISender mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateRestaurantCommand() { Name = request.Name, AppUserId = (Guid)httpContext.Items["AppUserId"]! };
            var result = await mediator.Send(command, cancellationToken);
            return result.Match(value => Results.Ok(value), errors => errors.ToProblem());
        })//.RequireAuthorization(policy => policy.RequireRole(UserRole.Owner))
            .AddEndpointFilter<RequireAppUserFilter>();
        
        return app;
    }
}