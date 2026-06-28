using FluentValidation;

namespace TodoApi.Features.Todos.UpdateTodo
{
    public class UpdateTodoValidator
           : AbstractValidator<UpdateTodoCommand>
    {
        public UpdateTodoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("A feladat neve kötelező.")
                .MaximumLength(200).WithMessage("A név maximum 200 karakter lehet.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("A leírás maximum 500 karakter lehet.");

            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("Érvénytelen prioritás.");

            RuleFor(x => x.DueDate)
                .GreaterThan(DateTimeOffset.UtcNow).WithMessage("A határidő a jövőben kell legyen.")
                .When(x => x.DueDate.HasValue);
        }
    }
}
