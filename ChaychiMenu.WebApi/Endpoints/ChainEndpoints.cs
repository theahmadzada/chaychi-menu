using MediatR;

using Microsoft.AspNetCore.Mvc;

using ChaychiMenu.Application.Commands.Chain;
using ChaychiMenu.Application.Dto;
using ChaychiMenu.Application.Queries.Chain;
using ChaychiMenu.Domain;
using ChaychiMenu.WebApi.Extensions;
using ChaychiMenu.WebApi.Filters;

namespace ChaychiMenu.WebApi.Endpoints;

public static class ChainEndpoints
{
    public static WebApplication MapChainEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/chains");

        group.MapPost("/", async (
            [FromBody] CreateChainDto dto,
            HttpContext httpContext,
            ISender mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateChainCommand() { Name = dto.Name, AppUserId = (Guid)httpContext.Items["AppUserId"]! };
            var result = await mediator.Send(command, cancellationToken);
            return result.Match(value => Results.Ok(value), errors => errors.ToProblem());
        }).AddEndpointFilter<RequireAppUserFilter>()
            .RequireAuthorization(options => options.RequireRole(UserRole.Owner));

        group.MapGet("/{id}", async (
            Guid id,
            HttpContext httpContext,
            ISender mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetChainByIdQuery() { Id = id, AppUserId = (Guid)httpContext.Items["AppUserId"]! };
            var result = await mediator.Send(query, cancellationToken);
            return result.Match(value => Results.Ok(value), errors => errors.ToProblem());
        }).AddEndpointFilter<RequireAppUserFilter>()
            .RequireAuthorization(options => options.RequireRole(UserRole.Owner));

        group.MapGet("/", async (
            HttpContext httpContext,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllOwnerChainsQuery() { AppUserId = (Guid)httpContext.Items["AppUserId"]! };
            var result = await mediator.Send(query, cancellationToken);
            return result.Match(value => Results.Ok(value), errors => errors.ToProblem());
        }).AddEndpointFilter<RequireAppUserFilter>()
            .RequireAuthorization(options => options.RequireRole(UserRole.Owner));

        return app;
    }
}