using ClaimFlow.Domain.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Policies.Commands.CreatePolicy
{
    public class CreatePolicyValidator : AbstractValidator<CreatePolicyCommand>
    {

        public CreatePolicyValidator()
        {
            RuleFor(p => p.PolicyNumber)
                .NotEmpty().WithMessage("Policy number is required.")
                .Matches(@"^POL-\d{4}-\d{5}$").WithMessage("Invalid format for policy number. EG: POL-2026-00123");


            RuleFor(p => p.CustomerId)
                .NotEmpty().WithMessage("CustomerId is required.");

            RuleFor(p => p.StartDate)
                .NotEmpty().WithMessage("Start date is required.")
                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Start date cant be before today. ");

            RuleFor(p => p.EndDate)
                 .NotEmpty().WithMessage("End date is required.")
                 .GreaterThan(p => p.StartDate).WithMessage("End date must be after start date.");

            RuleFor(p => p.ProductType)
                .NotEmpty().WithMessage("ProductType is required.")
                .IsInEnum().WithMessage("Invalid product type eg: 0 for health, 1 for Auto, 2 for Home, 3 for Travel");

        }
    }
}
