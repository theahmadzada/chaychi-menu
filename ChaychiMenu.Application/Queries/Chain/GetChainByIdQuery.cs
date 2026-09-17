using ChaychiMenu.Application.Dto;

using ErrorOr;

using MediatR;

namespace ChaychiMenu.Application.Queries.Chain;

public record GetChainByIdQuery : IRequest<ErrorOr<GetChainByIdDto>>
{
    public Guid Id { get; init; }
    public Guid AppUserId { get; init; }
}