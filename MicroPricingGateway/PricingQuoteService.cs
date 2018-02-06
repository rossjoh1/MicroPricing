using PricingGateway.Models;

using ServiceStack;
using System;
using System.Threading.Tasks;
using ServiceStack.Configuration;
using ServiceStack.Logging;
using System.Diagnostics;

namespace Gateway
{
    public class PricingQuoteService : IService
    {
        public static ILog Log = LogManager.GetLogger(typeof(PricingQuoteService));

        public async Task<PricingQuoteResponse> Get(PricingQuote quote)
        {
            var composedResult = new PricingQuoteResponse();
            var watch = Stopwatch.StartNew();

            // call pricing services concurrently
            Task<AppraisalQuoteResponse> appraisalResult;
            var appraisalQuote = new AppraisalQuote() { ProductType = quote.AppraisalProductType, EffectiveDate = quote.EffectiveDate, StateCode = quote.PropertyState, FromCache = quote.FromCache };
            using (var client = new ServiceStack.JsonServiceClient("http://pricing-appraisal:5001"))
                appraisalResult = client.GetAsync<AppraisalQuoteResponse>(appraisalQuote);

            Task<FeeScheduleQuoteResponse> feeScheduleResult;
            var feeSchedQuote = new FeeScheduleQuote() { ProductType = quote.ClosingProductType, EffectiveDate = quote.EffectiveDate, StateCode = quote.PropertyState, FromCache = quote.FromCache };
            using (var client = new ServiceStack.JsonServiceClient("http://pricing-feeschedule:5002"))
                feeScheduleResult = client.GetAsync<FeeScheduleQuoteResponse>(feeSchedQuote);

            //Task<UnderwriterQuoteResponse> underwriterResult;
            //var underwriterQuote = new UnderwriterQuote() { ProductType = quote.TitleProductType, EffectiveDate = quote.EffectiveDate, StateCode = quote.PropertyState, FromCache = quote.FromCache };
            //using (var client = new ServiceStack.JsonServiceClient("http://localhost:1339"))
            //    underwriterResult = client.GetAsync<UnderwriterQuoteResponse>(underwriterQuote);

            //Task<RecordingQuoteResponse> recordingResult;
            //var recordingQuote = new RecordingQuote() { ProductType = quote.RecordingDocumentType, EffectiveDate = quote.EffectiveDate, StateCode = quote.PropertyState, FromCache = quote.FromCache };
            //using (var client = new ServiceStack.JsonServiceClient("http://localhost:1340"))
            //    recordingResult = client.GetAsync<RecordingQuoteResponse>(recordingQuote);

            // compose results and return once they've all come back
            //await Task.WhenAll(appraisalResult, feeScheduleResult, underwriterResult, recordingResult);

            composedResult.AppraisalNotes = (await appraisalResult).Notes;
            composedResult.AppraisalFee = (await appraisalResult).Fee;
            composedResult.FeeScheduleNotes = (await feeScheduleResult).Notes;
            composedResult.FeeScheduleFee = (await feeScheduleResult).Fee;
            //composedResult.UnderwriterNotes = (await underwriterResult).Notes;
            //composedResult.UnderwriterFee = (await underwriterResult).Fee;
            //composedResult.RecordingNotes = (await recordingResult).Notes;
            //composedResult.RecordingFee = (await recordingResult).Fee;

            watch.Stop();
            Debug.WriteLine(string.Format("GetPricingQuote completed in {0} milliseconds", watch.ElapsedMilliseconds));

            return composedResult;
        }
    }
}
