using DataAccessLayer.Models.Payu;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Service_Layer.Services
{
    public interface IPayuService
    {
        Task<OrderCreateResponse> CreateNewOrderAsync(OrderCreateRequest orderCreateRequestData);
        Task<RefundOrderResponse> RefundAsync(string payuOrderId);
        Task<AcceptPayment> AcceptPaymentAsync(string payuOrderId);
        bool PayuSignatureIsValid(string requestInputStream, string payuHeaderSignature);
    }

    public class PayuService : IPayuService
    {
        private PayuAuth PayuAuth { get; set; }

        private Uri PayuBaseAddress = new Uri("https://secure.snd.payu.com/");

        private string ClientId = System.Configuration.ConfigurationManager.AppSettings["PayuClientId"];
        private string ClientSecret = System.Configuration.ConfigurationManager.AppSettings["PayuClientSecret"];

        private async Task GetAccessToken()
        {
            //Common testing requirement. If you are consuming an API in a sandbox/test region, uncomment this line of code ONLY for non production uses.
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            using (var httpClient = new HttpClient { BaseAddress = PayuBaseAddress })
            {
                using (var content = new StringContent("grant_type=client_credentials&client_id=" + ClientId + "&client_secret=" + ClientSecret + "", Encoding.UTF8, "application/x-www-form-urlencoded"))
                {
                    using (var response = await httpClient.PostAsync("pl/standard/user/oauth/authorize", content))
                    {
                        string tokenData = await response.Content.ReadAsStringAsync();
                        PayuAuth = JsonConvert.DeserializeObject<PayuAuth>(tokenData);
                    }
                }
            }
        }

        public async Task<OrderCreateResponse> CreateNewOrderAsync(OrderCreateRequest orderCreateRequestData)
        {
            if (PayuAuth == null)
                await GetAccessToken();

            OrderCreateResponse orderResponse;

            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            using (var httpClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = PayuBaseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", PayuAuth.Token_type + " " + PayuAuth.Access_token);

                using (var content = new StringContent(CreateNewOrderStringContent(orderCreateRequestData), Encoding.UTF8, "application/json"))
                {
                    using (var response = await httpClient.PostAsync("api/v2_1/orders/", content))
                    {
                        string responseData = await response.Content.ReadAsStringAsync();
                        orderResponse = JsonConvert.DeserializeObject<OrderCreateResponse>(responseData);
                    }
                }
            }

            return orderResponse;
        }

        private string CreateNewOrderStringContent(OrderCreateRequest orderCreateRequestData)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"extOrderId\": \"{orderCreateRequestData.ExtOrderId}\", ");
            sb.Append($"\"notifyUrl\": \"{orderCreateRequestData.NotifyUrl}\", ");
            sb.Append($"\"continueUrl\": \"{orderCreateRequestData.ContinueUrl}\", ");
            sb.Append($"\"customerIp\": \"{orderCreateRequestData.CustomerIp}\", ");
            sb.Append("\"merchantPosId\": \"311051\", ");
            sb.Append($"\"description\": \"{orderCreateRequestData.Description}\", ");
            sb.Append("\"currencyCode\": \"PLN\", ");
            sb.Append($"\"totalAmount\": \"{orderCreateRequestData.TotalAmount}\", ");

            sb.Append("\"buyer\": { ");
            sb.Append($"\"email\":\"{ orderCreateRequestData.Buyer.Email }\",");
            sb.Append($"\"firstName\":\"{ orderCreateRequestData.Buyer.FirstName }\",");
            sb.Append($"\"lastName\":\"{ orderCreateRequestData.Buyer.LastName }\"");
            sb.Append("},");

            sb.Append("\"products\": [ ");

            foreach (var p in orderCreateRequestData.Products)
            {
                sb.Append("{ ");
                sb.Append($"\"name\": \"{p.Name}\", ");
                sb.Append($"\"unitPrice\": \"{p.UnitPrice.ToString()}\", ");
                sb.Append($"\"quantity\": \"{p.Quantity.ToString()}\"");
                sb.Append(" },");
            }

            //Delete last ','
            sb.Length -= 1;
            sb.Append("]}");

            return sb.ToString();
        }

        public async Task<RefundOrderResponse> RefundAsync(string payuOrderId)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            RefundOrderResponse refundResponse = null;

            if (PayuAuth == null)
                await GetAccessToken();

            using (var httpClient = new HttpClient { BaseAddress = PayuBaseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", PayuAuth.Token_type + " " + PayuAuth.Access_token);

                using (var content = new StringContent("{  \"refund\": {    \"description\": \"Zamówienie anulowane przez sklep\" } }", Encoding.UTF8, "application/json"))
                {
                    using (var response = await httpClient.PostAsync("api/v2_1/orders/" + payuOrderId + "/refunds", content))
                    {
                        string responseData = await response.Content.ReadAsStringAsync();
                        refundResponse = JsonConvert.DeserializeObject<RefundOrderResponse>(responseData);
                    }
                }
            }

            return refundResponse;
        }

        public async Task<AcceptPayment> AcceptPaymentAsync(string payuOrderId)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            AcceptPayment status = null;

            if (PayuAuth == null)
                await GetAccessToken();

            using (var httpClient = new HttpClient { BaseAddress = PayuBaseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", PayuAuth.Token_type + " " + PayuAuth.Access_token);

                using (var content = new StringContent("{  \"orderId\": \"" + payuOrderId + "\",  \"orderStatus\": \"COMPLETED\"}", Encoding.UTF8, "application/json"))
                {
                    using (var response = await httpClient.PutAsync("api/v2_1/orders/" + payuOrderId + "/status", content))
                    {
                        string responseData = await response.Content.ReadAsStringAsync();
                        status = JsonConvert.DeserializeObject<AcceptPayment>(responseData);
                    }
                }
            }

            return status;
        }

        public bool PayuSignatureIsValid(string requestInputStream, string payuHeaderSignature)
        {
            var payuHeaderParts = payuHeaderSignature.Split(new char[] { ';' });

            string actualSignature = payuHeaderParts.Where(s => s.StartsWith("signature=")).Single().Substring(10);

            string algorithm = payuHeaderParts.Where(s => s.StartsWith("algorithm=")).Single().Substring(10);

            string expectedSignature = ComputeHash((requestInputStream + System.Configuration.ConfigurationManager.AppSettings["PayuSecondKey"]), algorithm);

            return expectedSignature != actualSignature ? false : true;
        }

        private static string ComputeHash(string input, string algorithm)
        {
            var hashAlgorithm = (HashAlgorithm)CryptoConfig.CreateFromName(algorithm);

            Byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            Byte[] hashBytes = hashAlgorithm.ComputeHash(inputBytes);

            return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
        }
    }
}