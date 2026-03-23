using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.DTOs
{
    public record TenantDto
    (
         Guid Id ,
         string Name ,
         string Code ,
         bool IsActive ,
         DateTime CreatedAt 

    );
}
