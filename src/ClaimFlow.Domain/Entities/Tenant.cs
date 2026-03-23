using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Domain.Entities
{
    public class Tenant
    {
        public Guid Id { get; set; }
        public string Name { get; set; } //branch or agency name
        public string Code { get; set; } //Unique like "IST-01" "ANK-023"
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public ICollection<Customer> Customers { get; set; } = new HashSet<Customer>();
        public ICollection<Policy> Policies { get; set; } = new HashSet<Policy>();


    }
}
