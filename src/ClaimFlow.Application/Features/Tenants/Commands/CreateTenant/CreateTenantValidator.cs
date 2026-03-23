using ClaimFlow.Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Tenants.Commands.CreateTenant
{
    public class CreateTenantValidator : AbstractValidator<CreateTenantCommand>
    {

        public CreateTenantValidator()
        {
            RuleFor(t => t.Name)
                .NotEmpty().WithMessage(" Tenant name is required")
                .MaximumLength(200).WithMessage("Maximum length for name is 200 char. ");

            RuleFor(t => t.Code)
                .NotEmpty().WithMessage("Tenant code is required.")
                .Matches(@"^[A-Z]{2,4}-\d{2,3}$").WithMessage("Invalid code format. EG: 'IST-01' 'DZC-025'");

               
        }


    }
}
