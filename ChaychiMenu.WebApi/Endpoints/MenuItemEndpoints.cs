using MediatR;

using Microsoft.AspNetCore.Mvc;

using ChaychiMenu.Application.Commands.MenuItem;
using ChaychiMenu.Application.Dto;
using ChaychiMenu.Application.Queries.MenuItem;
using ChaychiMenu.Domain;
using ChaychiMenu.WebApi.Extensions;
using ChaychiMenu.WebApi.Filters;

namespace ChaychiMenu.WebApi.Endpoints;

public static class MenuItemEndpoints
{
    public static WebApplication MapMenuItemEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/menu-items");

        group.MapPost("/", async (
                [FromForm] CreateMenuItemDto dto,
                HttpContext httpContext,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var request = new CreateMenuItemCommand
                {
                    AppUserId = (Guid)httpContext.Items["AppUserId"]!,
                    Title = dto.Title,
                    Description = dto.Description,
                    CategoryId = dto.CategoryId,
                    Price = dto.Price,
                    Order = dto.Order,
                    Image = dto.Image?.OpenReadStream(),
                    ContentType = dto.Image?.ContentType,
                    UserRole = httpContext.Items["UserRole"]!.ToString()!
                };
                var result = await mediator.Send(request, cancellationToken);
                return result.Match(value => Results.Ok(value), errors => errors.ToProblem());
            })//.RequireAuthorization(options => options.RequireRole(UserRole.Owner))
            .AddEndpointFilter<RequireAppUserFilter>()
            .AddEndpointFilter<RequireAppRoleFilter>()
            .DisableAntiforgery();

        group.MapPatch("/{id}", async (
            Guid id,
            [FromBody] ToggleMenuItemAvailabilityDto request,
            HttpContext httpContext,
            ISender mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new ToggleMenuItemAvailabilityCommand()
            {
                MenuItemId = id, 
                IsAvailable = request.IsAvailable, 
                AppUserId = (Guid)httpContext.Items["AppUserId"]!, 
                UserRole = httpContext.Items["UserRole"]!.ToString()!,
            };
            var result = await mediator.Send(command, cancellationToken);
            return result.Match(value => Results.Ok(value), errors => errors.ToProblem());
        })//.RequireAuthorization(options => options.RequireRole(UserRole.Owner))
            .AddEndpointFilter<RequireAppUserFilter>()
            .AddEndpointFilter<RequireAppRoleFilter>();

        group.MapGet("/{id}", async (
            Guid id,
            ISender mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetMenuItemByIdQuery() { Id = id };
            var result = await mediator.Send(query, cancellationToken);
            return result.Match(value => Results.Ok(value), errors => errors.ToProblem());
        });

        return app;
    }
}