using ClaimFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Domain.Entities
{
    public class Premium
    {
        public Guid Id { get; set; }
        public Guid PolicyId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public PremiumStatus PremiumStatus { get; set; }

        public Policy Policy { get; set; }

    }
}
