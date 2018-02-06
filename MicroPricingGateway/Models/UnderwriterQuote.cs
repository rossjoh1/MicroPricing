using System;

namespace PricingGateway.Models
{
    public class UnderwriterQuote
    {
        public string ProductType { get; set; }
        public string StateCode { get; set; }
        public DateTime EffectiveDate { get; set; }
        public bool FromCache { get; set; }
    }

    public class UnderwriterQuoteResponse
    {
        public string Notes { get; set; }
        public decimal Fee { get; set; }
    }
}