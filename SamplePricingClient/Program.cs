using System;
using PricingGateway.Models;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace SamplePricingClient
{
    class Program
    {
        private static bool _useCache { get; set; } = false;

        static void Main(string[] args)
        {
            while (true)
            {
                switch (DisplayMenu())
                {
                    case 1:
                        DoCallPricing();
                        break;
                    case 2:
                        DoViewCache();
                        break;
                    case 3:
                        DoClearCache();
                        break;
                    default:
                        Console.WriteLine("Invalid selection");
                        break;
                }
            }
        }

        private static void DoViewCache()
        {
            using (var client = new RedisClient(ConfigUtils.GetAppSetting("RedisEndpoint").ToRedisEndpoint()))
            {
                client.ConnectTimeout = 100;

                try
                {
                    var keys = client.GetKeysStartingWith("JLR");

                    if (keys != null)
                    {
                        Console.WriteLine("Existing keys: ");
                        foreach (var key in keys)
                            Console.WriteLine("{0}", key);
                    }
                    else
                        Console.WriteLine("No cache items exist yet");
                }
                catch(RedisException)
                {
                    Console.WriteLine("Redis not available");
                }
                
            }
        }

        private static void DoClearCache()
        {
            using (var client = new RedisClient(ConfigUtils.GetAppSetting("RedisEndpoint").ToRedisEndpoint()))
            {
                Console.Write("Clearing cache... ");
                client.RemoveAll(client.GetKeysStartingWith("JLR"));
                Console.Write("\ndone\n");
            }
        }

        private static void DoCallPricing()
        {
            Console.Write("Number of iterations: ");
            var numberOfCalls = int.Parse(Console.ReadLine());
            //Console.Write("Use cache? (Y or N): ");
            //_useCache = Console.ReadLine().Equals("Y", StringComparison.CurrentCultureIgnoreCase) ? true : false;

            for (int i = 0; i < numberOfCalls; i++)
                GenerateAndCall();
        }

        private static int DisplayMenu()
        {
            Console.WriteLine("Menu\n----");
            Console.WriteLine("1) Call Pricing");
            //Console.WriteLine("2) View cached keys");
            //Console.WriteLine("3) Clear all cache");

            return int.Parse(Console.ReadLine());
        }

        private static void GenerateAndCall()
        {
            var quoteRequest = GenerateRequest();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            var result = GetPricingQuote(quoteRequest);
            watch.Stop();

            if (result != null)
            {
                Console.WriteLine("Received quote for {0} in {1} milliseconds", quoteRequest.PropertyState, watch.ElapsedMilliseconds);
                Console.WriteLine("Appraisal Fee: {0} | {1}", result.AppraisalFee, result.AppraisalNotes);
                Console.WriteLine("FeeSchedule Fee: {0} | {1}", result.FeeScheduleFee, result.FeeScheduleNotes);
                //Console.WriteLine("Underwriter Fee: {0} | {1}", result.UnderwriterFee, result.UnderwriterNotes);
                //Console.WriteLine("Recording Fee: {0} | {1}", result.RecordingFee, result.RecordingNotes);
                Console.WriteLine("----------------------------------------");
            }
        }

        private static PricingQuoteResponse GetPricingQuote(PricingQuote quote)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigUtils.GetAppSetting("MicroPricingGatewayUri"));
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var args = quote.ToGetUrl().SafeSubstring(quote.ToGetUrl().IndexOf('?'));
                HttpResponseMessage response = client.GetAsync("getQuote" + args).Result;

                if (response.IsSuccessStatusCode)
                {
                    var serial = new ServiceStack.Serialization.JsonDataContractSerializer();
                    var ret = serial.DeserializeFromString<PricingQuoteResponse>(response.Content.ReadAsStringAsync().Result);
                    return ret;
                }

                return null;
            }
        }

        private static PricingQuote GenerateRequest()
        {
            var quote = new PricingQuote()
            {
                AppraisalProductType = "1004SingleFamilyFull",
                TitleProductType = "TitleCommitment",
                ClosingProductType = "NotarySigning",
                RecordingDocumentType = "OwnersMortgage",
                LoanAmount = 300000,
                ReferenceNumber = 301232442,
                EffectiveDate = DateTime.Today,
                FromCache = _useCache
            };

            var randomizer = new Random();
            quote.PropertyState = stateList[randomizer.Next(0, 49)];

            return quote;
        }

        private static string[] stateList = new string[] {"AK", "AL", "AR", "AZ", "CA", "CO", "CT", "DC", "DE", "FL", "GA", "GU", "HI", "IA", "ID", "IL", "IN", "KS", "KY", "LA", "MA", "MD", "ME", "MH", "MI", "MN", "MO", "MS", "MT", "NC", "ND", "NE", "NH", "NJ", "NM", "NV", "NY", "OH", "OK", "OR", "PA", "PR", "PW", "RI", "SC", "SD", "TN", "TX", "UT", "VA", "VI", "VT", "WA", "WI", "WV", "WY"};
    }
}
