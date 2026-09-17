using ChaychiMenu.Application.Commands.MenuItem;
using ChaychiMenu.Application.Dto;
using ChaychiMenu.Domain;
using ChaychiMenu.Infrastructure.DbContext;

using ErrorOr;

using MediatR;

using Microsoft.EntityFrameworkCore;

using ChaychiMenu.Infrastructure.ServiceContracts;

namespace ChaychiMenu.Application.Handlers.MenuItem;

public class CreateMenuItemCommandHandler(
    AppDbContext dbContext, 
    IFileStorageService storage) : IRequestHandler<CreateMenuItemCommand, ErrorOr<MenuItemDto>>
{
    public async Task<ErrorOr<MenuItemDto>> Handle(CreateMenuItemCommand request, CancellationToken cancellationToken)
    {
        var query = dbContext.Categories.Where(x => x.Id == request.CategoryId);
        if (request.UserRole != UserRole.Admin)
            query = query.Where(x => x.Restaurant.Owner.AppUserId == request.AppUserId);
        
        var categoryExists = await query.AnyAsync(cancellationToken);
        if (!categoryExists)
            return Error.NotFound("Category.NotFound", "Category not found");
        
        string? imageUrl = null;
        if (request.Image is not null && request.ContentType is not null)
            imageUrl = await storage.UploadAsync(request.Image, request.ContentType, cancellationToken);        
        
        var menuItem = new Domain.Entities.MenuItem
        {
            Title = request.Title,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Price = request.Price,
            ImageUrl = imageUrl
        };

        if(request.Order is null)
        {
            var menuItems = await dbContext.MenuItems
                .Where(x => x.CategoryId == request.CategoryId)
                .CountAsync(cancellationToken);
            menuItem.Order = menuItems;
        }
        else
        { 
            menuItem.Order = request.Order.Value;
        }
        
        dbContext.MenuItems.Add(menuItem);
        await dbContext.SaveChangesAsync(cancellationToken);
            
        return new MenuItemDto
        {
            Id = menuItem.Id,
            Title = menuItem.Title,
            Description = menuItem.Description,
            CategoryId = menuItem.CategoryId,
            Price = menuItem.Price,
            Order = menuItem.Order,
            ImageUrl = menuItem.ImageUrl
        };
    }
}