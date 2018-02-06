using ServiceStack;
using ServiceStack.Logging;

namespace AppraisalPricing
{
    public class AppHost : AppHostBase
    {
        public AppHost() : base("Appraisal Quote Service", typeof(AppraisalQuoteService).Assembly)
        {
            LogManager.LogFactory = new ConsoleLogFactory(false);
        }

        public override void Configure(Funq.Container container)
        {
            Routes
                .Add<AppraisalQuote>("/appraisalQuote")
                .Add<AppraisalQuote>("/appraisalQuote/{ProductType}");
        }
    }
}
