using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Domain.Entities
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public DateTime OccuredAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? Error { get; set; }

    }
}
