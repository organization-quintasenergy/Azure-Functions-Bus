using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AFBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace PaymentService
{
    public static class PaymentsReadAPI
    {
        [FunctionName("GetPayments")]
        public static HttpResponseMessage RunGetPayments([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, ILogger log)
        {
            log.LogInformation("GetPayments received a request.");

            var repository = HandlersContainer.SolveDependency<IPaymentsRepository>();
            
            var payments = repository.GetOrdersPayed();
            return payments == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, payments);
        }

        [FunctionName("GetPaymentsByUser")]
        public static HttpResponseMessage RunGetPaymentsByUser([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, ILogger log)
        {
            log.LogInformation("GetPaymentsByUser received a request.");
            
            var repository = HandlersContainer.SolveDependency<IPaymentsRepository>();

            var payments = repository.GetOrdersPayed();

            return payments == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, payments);
        }
    }
}
