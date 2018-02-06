using ServiceStack;
using System;
using System.Threading.Tasks;
using ServiceStack.Redis;
using ServiceStack.Configuration;
using ServiceStack.Logging;

namespace AppraisalPricing
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

    public class AppraisalQuoteService : IService
    {
        public static ILog Log = LogManager.GetLogger(typeof(AppraisalQuoteService));

        public async Task<AppraisalQuoteResponse> Get(AppraisalQuote request)
        {
            AppraisalQuoteResponse result;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            if (request.FromCache)
                result = await GetFromCache(request);
            else
                result = await AppraisalDataLookup.GetQuoteForState(request);

            Log.InfoFormat("Call returning from {0} in {1} milliseconds", request.FromCache ? "cache" : "db", watch.ElapsedMilliseconds);
            return result;
        }

        private async Task<AppraisalQuoteResponse> GetFromCache(AppraisalQuote request)
        {
            AppraisalQuoteResponse result;

            using (var client = new RedisClient(ConfigUtils.GetAppSetting("RedisEndpoint").ToRedisEndpoint()))
            {
                result = client.As<AppraisalQuoteResponse>().GetValue("JLR-" + request.StateCode + "-appraisalPrices");
                if (result == null)
                {
                    result = await AppraisalDataLookup.GetQuoteForState(request);
                    client.Set("JLR-" + request.StateCode + "-appraisalPrices", result, DateTime.Now.AddHours(1));
                }
            }

            return result;
        }
    }

    public class AppraisalDataLookup
    {
        public static async Task<AppraisalQuoteResponse> GetQuoteForState(AppraisalQuote request)
        {
            var fee = Math.Round((decimal)request.StateCode[0] / 5, 2);
            var appraisalType = request.ProductType ?? "N/A";

            await Task.Delay(500);

            return new AppraisalQuoteResponse
            {
                Notes = string.Format("That'll be {0} for {1}", fee, appraisalType),
                Fee = fee
            };
        }
    }
}