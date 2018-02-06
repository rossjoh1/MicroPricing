using ServiceStack;
using ServiceStack.Redis;
using System;
using System.Threading.Tasks;
using ServiceStack.Configuration;
using ServiceStack.Logging;

namespace FeeSchedulePricing
{
    public class FeeScheduleQuote
    {
        public string ProductType { get; set; }
        public string StateCode { get; set; }
        public DateTime EffectiveDate { get; set; }
        public bool FromCache { get; set; }
    }

    public class FeeScheduleQuoteResponse
    {
        public string Notes { get; set; }
        public decimal Fee { get; set; }
    }

    public class FeeScheduleQuoteService : IService
    {
        public static ILog Log = LogManager.GetLogger(typeof(FeeScheduleQuoteService));

        public async Task<FeeScheduleQuoteResponse> Get(FeeScheduleQuote request)
        {
            FeeScheduleQuoteResponse result;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            if (request.FromCache)
                result = await GetFromCache(request);
            else
                result = await FeeScheduleDataLookup.GetQuoteForState(request);

            Log.InfoFormat("Call returning from {0} in {1} milliseconds", request.FromCache ? "cache" : "db", watch.ElapsedMilliseconds);
            return result;
        }

        private async Task<FeeScheduleQuoteResponse> GetFromCache(FeeScheduleQuote request)
        {
            FeeScheduleQuoteResponse result;

            using (var client = new RedisClient(ConfigUtils.GetAppSetting("RedisEndpoint").ToRedisEndpoint()))
            {
                result = client.As<FeeScheduleQuoteResponse>().GetValue("JLR-" + request.StateCode + "-feeSchedulePrices");
                if (result == null)
                {
                    result = await FeeScheduleDataLookup.GetQuoteForState(request);
                    client.Set("JLR-" + request.StateCode + "-feeSchedulePrices", result, DateTime.Now.AddHours(1));
                }
            }

            return result;
        }
    }

    public class FeeScheduleDataLookup
    {
        public static async Task<FeeScheduleQuoteResponse> GetQuoteForState(FeeScheduleQuote request)
        {
            var fee = Math.Round((decimal)request.StateCode[0] * 2, 2);

            await Task.Delay(500);

            return new FeeScheduleQuoteResponse
            {
                Notes = string.Format("That'll be {0} for {1}", fee, request.ProductType ?? "N/A"),
                Fee = fee
            };
        }
    }
}
