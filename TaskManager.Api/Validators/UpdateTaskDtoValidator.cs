using FluentValidation;
using TaskManager.Domain.DTOs;
using System;

namespace TaskManager.API.Validators;

public class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description cannot be empty.")
            .MaximumLength(50).WithMessage("Description cannot exceed 50 characters.");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow).WithMessage("Due date must be in the future or today.");
    }
}