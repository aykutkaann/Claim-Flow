using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
    {

        public CreateCustomerValidator()
        {
            RuleFor(c => c.FullName)
                .NotEmpty().WithMessage("Full name of customers are required.")
                .MaximumLength(200).WithMessage("Maximum length for full name is 200 char.");

            RuleFor(c => c.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(c => c.TenantId)
                .NotEmpty().WithMessage("TenantId is required.");

        }
    }
}
