using System;

namespace PricingGateway.Models
{
    public class AppraisalQuote
    {
        public string ProductType { get; set; }
        public string StateCode { get; set; }
        public DateTime EffectiveDate { get; set; }
        public bool FromCache { get; set; }
    }

    public class AppraisalQuoteResponse
    {
        public string Notes { get; set; }
        public decimal Fee { get; set; }
    }
}