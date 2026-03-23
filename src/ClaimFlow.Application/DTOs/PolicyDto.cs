using ClaimFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.DTOs
{
    public record PolicyDto
    (
         Guid Id,
         string PolicyNumber,
         string Type,
         string Status,
         DateTime StartDate,
         DateTime? EndDate,

         Guid CustomerId
        
    );
}
