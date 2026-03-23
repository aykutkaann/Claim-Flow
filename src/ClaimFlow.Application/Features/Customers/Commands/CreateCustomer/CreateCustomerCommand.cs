using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Customers.Commands.CreateCustomer
{
    public record CreateCustomerCommand(string FullName, string Email, Guid TenantId) : IRequest<Guid>;
    
    
}
