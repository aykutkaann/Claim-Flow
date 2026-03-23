using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ClaimFlow.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }


        public Guid TenantId { get; set; }
        public JsonDocument?   ProfileData { get; set; }
        public  Tenant Tenant { get; set; }
        public ICollection<Policy> Policies { get; set; } = new HashSet<Policy>();
    }
}
