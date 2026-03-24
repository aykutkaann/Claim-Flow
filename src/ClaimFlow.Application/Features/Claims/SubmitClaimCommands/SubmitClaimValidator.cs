using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Claims.Commands
{
    public class SubmitClaimValidator : AbstractValidator<SubmitClaimCommand>
    {

        public SubmitClaimValidator()
        {
            RuleFor(c => c.PolicyId).NotEmpty().WithMessage("Policy Id is required.");

            RuleFor(c => c.Description).NotEmpty().WithMessage("Description is required (minimum 10 char).")
                .MinimumLength(10).MaximumLength(200);

            RuleFor(c => c.ClaimedAmount).NotEmpty().WithMessage("Amount is required.").GreaterThan(0);
                
        }
    }
}
