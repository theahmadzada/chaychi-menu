using ChaychiMenu.Application.Dto;

using ErrorOr;

using FluentValidation;

using MediatR;

namespace ChaychiMenu.Application.Commands.Owner;

public record ValidateOtpCommand() : IRequest<ErrorOr<AuthDto>>
{
    public required string Otp { get; init; }
    public long TelegramId { get; init; }
};

public class ValidateOtpCommandValidator : AbstractValidator<ValidateOtpCommand>
{
    public ValidateOtpCommandValidator()
    {
        RuleFor(c => c.Otp)
            .NotEmpty()
            .WithMessage("Otp is required");
        RuleFor(c => c.TelegramId)
            .NotEmpty()
            .WithMessage("TelegramId is required");
    }
}