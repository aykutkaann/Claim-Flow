using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.DTOs
{
    public record CustomerDto
    (
         Guid Id ,
         string FullName ,
         string Email,
         Guid TenantId,
         DateTime CreatedAt 
      );    
}
