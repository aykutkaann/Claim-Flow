using ClaimFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Domain.Entities
{
    public class Policy
    {
        public Guid Id { get; set; }

        public string PolicyNumber { get; set; }
        public Guid TenantId { get; set; }
        public Guid CustomerId { get; set; }
        public ProductType ProductType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public PolicyStatus PolicyStatus { get; set; }

        public Customer Customer { get; set; }
        public Tenant Tenant { get; set; }
        
        public ICollection<Coverage> Coverages { get; set; } = new HashSet<Coverage>();
        public ICollection<Beneficiary> Beneficiaries { get; set; } = new HashSet<Beneficiary>();

        public ICollection<Premium> Premia { get; set; } = new HashSet<Premium>();


    }
}
