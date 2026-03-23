using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Domain.Entities
{
    public class Coverage
    {
        public Guid Id { get; set; }
        public Guid PolicyId { get; set; }
        public string CoverageType { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal DeductibleAmount { get; set; }


        public Policy Policy { get; set; }

    }
}
