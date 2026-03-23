using ClaimFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Domain.Entities
{
    public class Claim
    {
        public Guid Id { get; set; }
        public string ClaimNumber { get; set; }


        public Guid PolicyId { get; set; }
        public Guid TenantId { get; set; }

        public string Description { get; set; }
        public decimal ClaimedAmount { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public ClaimStatus Status { get; set; }


        public DateTime SubmittedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public int? FraudRiskScore { get; set; }


        public Policy Policy { get; set; }
        public Tenant Tenant { get; set; }


        public ICollection<ClaimStatusHistory> Histories { get; set; } = new HashSet<ClaimStatusHistory>();
        public ICollection<ClaimDocument> Documents { get; set; } = new HashSet<ClaimDocument>();


    }
}
