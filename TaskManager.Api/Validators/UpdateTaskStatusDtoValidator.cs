using FluentValidation;
using TaskManager.Domain.DTOs;
using TaskManager.Data.Models;

namespace TaskManager.API.Validators;

public class UpdateTaskStatusDtoValidator : AbstractValidator<UpdateTaskStatusDto>
{
    public UpdateTaskStatusDtoValidator()
    {
        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid status value. Must be one of ToDo, InProgress, InReview, Done, Paused, Failed.");
    }
}