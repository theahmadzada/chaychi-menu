using MediatR;

using Microsoft.AspNetCore.Mvc;

using ChaychiMenu.Application.Commands.Owner;
using ChaychiMenu.Application.Dto;
using ChaychiMenu.Domain;
using ChaychiMenu.WebApi.Extensions;
using ChaychiMenu.WebApi.Filters;

namespace ChaychiMenu.WebApi.Endpoints;

public static class OwnerEndpoints
{
    public static WebApplication MapOwnerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/owners");

        group.MapPost("/", async (
            CreateOwnerCommand command,
            ISender mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            return result.Match(value => Results.Ok(value), errors => errors.ToProblem());
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.Admin));
        
        group.MapPatch("/{id}", async (
            Guid id,
            UpdateOwnerDto dto,
            ISender mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateOwnerCommand { AppUserId = id, Document = dto.Document };
            var result = await mediator.Send(command, cancellationToken);
            return result.Match(value => Results.Ok(value), errors => errors.ToProblem());
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.Owner));

        group.MapPost("/password", async (
            [FromBody] ChangePasswordDto dto,
            HttpContext httpContext,
            ISender mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new ChangeOwnerPasswordCommand()
            {
                AppUserId = (Guid)httpContext.Items["AppUserId"]!, OldPassword = dto.OldPassword, NewPassword = dto.NewPassword
            };
            var result = await mediator.Send(command, cancellationToken);
            return result.Match(value => Results.Ok(value), errors => errors.ToProblem());
        }).AddEndpointFilter<RequireAppUserFilter>()
            .RequireAuthorization(policy => policy.RequireRole(UserRole.Owner));

        group.MapDelete("/{id}", async (
            Guid id,
            ISender mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteOwnerCommand { AppUserId = id };
            var result = await mediator.Send(command, cancellationToken);
            return result.Match(value => Results.Ok(value), errors => errors.ToProblem());
        }).RequireAuthorization(policy =>
        {
            policy.RequireRole(UserRole.Owner);
            policy.RequireRole(UserRole.Admin);
        });

        return app;
    }
}
