using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Domain.Entities
{
    public class Beneficiary
    {
        public Guid Id { get; set; }
        public Guid PolicyId { get; set; }
        public string FullName { get; set; }
        public string Relationships { get; set; }
        public decimal Percentage { get; set; }

        public Policy Policy { get; set; }

    }
}
