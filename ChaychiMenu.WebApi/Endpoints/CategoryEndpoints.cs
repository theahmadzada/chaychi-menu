using MediatR;

using ChaychiMenu.Application.Commands.Category;
using ChaychiMenu.Application.Dto;
using ChaychiMenu.Application.Queries.Category;
using ChaychiMenu.Domain;
using ChaychiMenu.WebApi.Extensions;
using ChaychiMenu.WebApi.Filters;

namespace ChaychiMenu.WebApi.Endpoints;

public static class CategoryEndpoints
{
    public static WebApplication MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/categories");

        group.MapPost("/", async (
            CreateCategoryDto request,
            HttpContext httpContext,
            ISender mediatr,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateCategoryCommand()
            {
                AppUserId = (Guid)httpContext.Items["AppUserId"]!,
                Name = request.Name,
                RestaurantId = request.RestaurantId,
                Order = request.Order
            };
            var result = await mediatr.Send(command, cancellationToken);
            return result.Match(value => Results.Ok(value), errors => errors.ToProblem());
        })
        //     .RequireAuthorization(options =>
        // {
        //     options.RequireRole(UserRole.Owner);
        // })
            .AddEndpointFilter<RequireAppUserFilter>();

        group.MapGet("/{slug}", async (
            string slug,
            ISender mediatr,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCategoriesAccordingToSlugQuery() { Slug = slug };
            var result = await mediatr.Send(query, cancellationToken);
            return result.Match(value => Results.Ok(value), errors => errors.ToProblem());
        });
        
        return app;
    }
}