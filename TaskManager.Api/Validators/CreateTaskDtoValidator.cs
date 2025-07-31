using FluentValidation;
using TaskManager.Domain.DTOs;
using System;

namespace TaskManager.API.Validators;

public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description cannot be empty.")
            .MaximumLength(50).WithMessage("Description cannot exceed 50 characters.");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow).WithMessage("Due date must be in the future or today.");
        RuleFor(x => x.UserId)
            .MaximumLength(20).WithMessage("User ID cannot exceed 20 characters.");
    }
}