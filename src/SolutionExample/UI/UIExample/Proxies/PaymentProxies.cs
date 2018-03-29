using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PaymentService.Messages;
using UIExample.ViewModels;

namespace UIExample.Proxies
{
    public class PaymentProxies : IPaymentProxies
    {
        public async Task<PaymentViewModel> GetPayments()
        {
            var apiURL = "http://localhost:7072/api/GetPayments";
            HttpClient client = new HttpClient(); HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, new Uri(apiURL));

            HttpResponseMessage response = await client.SendAsync(request);

            var responseString = response.Content.ReadAsStringAsync().Result;

            var responseVM = JsonConvert.DeserializeObject<PaymentViewModel>(responseString);

            return responseVM;
        }
    }
}
