using FluentValidation;
using TaskManager.Domain.DTOs;

namespace TaskManager.API.Validators;

public class UpdateTaskAssigneeDtoValidator : AbstractValidator<UpdateTaskAssigneeDto>
{
    public UpdateTaskAssigneeDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID cannot be empty.")
            .MaximumLength(20).WithMessage("User ID cannot exceed 20 characters.");
    }
}