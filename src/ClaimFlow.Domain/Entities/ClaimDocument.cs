using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Domain.Entities
{
    public class ClaimDocument
    {
        public Guid Id { get; set; }
        public Guid ClaimId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public DateTime UploadedAt { get; set; }

        public Claim Claim { get; set; }
    }
}
