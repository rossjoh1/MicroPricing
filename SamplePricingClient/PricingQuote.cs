using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PricingGateway.Models
{
    public class PricingQuote
    {
        public int ReferenceNumber { get; set; }
        public decimal LoanAmount { get; set; }
        public string AppraisalProductType { get; set; }
        public string TitleProductType { get; set; }
        public string ClosingProductType { get; set; }
        public string RecordingDocumentType { get; set; }
        public string PropertyState { get; set; }
        public DateTime EffectiveDate { get; set; }
        public bool FromCache { get; set; }
    }

    public class PricingQuoteResponse
    {
        public decimal AppraisalFee { get; set; }
        public string AppraisalNotes { get; set;}
        public decimal UnderwriterFee { get; set; }
        public string UnderwriterNotes { get; set; }
        public decimal FeeScheduleFee { get; set; }
        public string FeeScheduleNotes { get; set; }
        public decimal RecordingFee { get; set; }
        public string RecordingNotes { get; set; }
    }
}