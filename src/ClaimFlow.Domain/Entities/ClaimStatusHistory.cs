using ClaimFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Domain.Entities
{
    public class ClaimStatusHistory
    {
        public Guid Id { get; set; }
        public Guid ClaimId { get; set; }
        public ClaimStatus  FromStatus   { get; set; }

        public ClaimStatus ToStatus { get; set; }


        public DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; }
        public string Notes { get; set; }


        public Claim Claim { get; set; }
    }
}
