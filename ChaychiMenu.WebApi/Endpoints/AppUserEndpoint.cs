using MediatR;

using ChaychiMenu.Application.Commands.AppUser;
using ChaychiMenu.Application.Commands.Owner;
using ChaychiMenu.WebApi.Extensions;

namespace ChaychiMenu.WebApi.Endpoints;

public static class AppUserEndpoint
{
    public static WebApplication MapAppUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/auth");
        
        group.MapPost("/login", async (
            LogInCommand command,
            ISender mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            return result.Match(value => Results.Ok(value), errors => errors.ToProblem());
        });
        
        group.MapPost("/otp", async (
            GenerateTelegramOtpCommand request,
            ISender mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(request, cancellationToken);
            return result.Match(value => Results.Ok(value), errors => errors.ToProblem());
        });
        
        group.MapPost("/otp/validate", async (
            ValidateOtpCommand request,
            ISender mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(request, cancellationToken);
            return result.Match(value => Results.Ok(value), errors => errors.ToProblem());
        });
        
        return app;
    }
}