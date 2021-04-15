using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotNetConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var url= await GetSmartCheckSessionUrl();
            Console.WriteLine(url);
        }

        private static string identityServerUrl = "https://test-identity.monthio.com";
        private static string budgetServerUrl = "https://test-budgets.monthio.com";

        private static async Task<string> GetSmartCheckSessionUrl()
        {
            var client = new HttpClient();
            var discoveryDocument = await client.GetDiscoveryDocumentAsync($"{identityServerUrl}/");

            var tokenResponse = await client.RequestRefreshTokenAsync(
                new RefreshTokenRequest
                {
                    Address = discoveryDocument.TokenEndpoint,
                    RefreshToken = "<YOUR REFRESH TOKEN>",
                    ClientId = "external_client", // Do not replace this with your clientId, this should stay external_client
                    Scope = "budgetApi"
                });

            var apiClient = new HttpClient();
            apiClient.SetBearerToken(tokenResponse.AccessToken);

            var budgetDefinitionsResponse = await apiClient.GetAsync($"{budgetServerUrl}/api/budget-definitions/available");
            var budgetDefinitions = JsonConvert.DeserializeObject<List<BudgetDefinition>>(await budgetDefinitionsResponse.Content.ReadAsStringAsync());

            var configurationResponse = await apiClient.GetAsync($"{budgetServerUrl}/api/smart-check-configurations");
            var existingConfigurations = JsonConvert.DeserializeObject<CollectionEnvelope<SmartCheckConfigurationResponseDto>>(await configurationResponse.Content.ReadAsStringAsync());

            if (existingConfigurations.Collection.Count == 0)
            {
                await CreateConfiguration(apiClient, budgetDefinitions);
                configurationResponse = await apiClient.GetAsync($"{budgetServerUrl}/api/smart-check-configurations");
                existingConfigurations = JsonConvert.DeserializeObject<CollectionEnvelope<SmartCheckConfigurationResponseDto>>(await configurationResponse.Content.ReadAsStringAsync());
            }
            
            var configuration = existingConfigurations.Collection.First();

                var createSessionSmartCheckResponse = await apiClient.PostAsync($"{budgetServerUrl}/api/smart-check-sessions", 
                    new StringContent(JsonConvert.SerializeObject(new SmartCheckSessionRequestDto
                {
                    ConsumerId = "consumer_id",
                    ConsumerEmail = "oap@monthio.com",
                    SmartCheckConfigurationId = configuration.Id,

                }), Encoding.UTF8, "application/json"));
            
            var createdSmartCheckSession =
                JsonConvert.DeserializeObject<SmartCheckSessionResponseDto>(await createSessionSmartCheckResponse
                    .Content.ReadAsStringAsync());

            return "https://test-flow.monthio.com/?sessionId=" + createdSmartCheckSession.Id;
        }

        private static async Task CreateConfiguration(HttpClient apiClient, List<BudgetDefinition> budgetDefinitions)
        {
            var response = await apiClient.PostAsync($"{budgetServerUrl}/api/smart-check-configurations",
                new StringContent(JsonConvert.SerializeObject(new SmartCheckConfigurationRequestDto
                {
                    Name = "SmartCheck configuration from API",
                    CallbackUrl = "<YOUR CALLBACK URL>",
                    Consent = new Consent
                    {
                        Header = "Consent header",
                        Text = "Consent text"
                    },
                    BudgetDefinitionId = budgetDefinitions.First().Id
                }), Encoding.UTF8, "application/json"));
        }
    }

    public class SmartCheckSessionRequestDto
    {
        public string ConsumerId { get; set; }
        public string ConsumerEmail { get; set; }
        public long SmartCheckConfigurationId { get; set; }
    }

    public class SmartCheckSessionResponseDto
    {
        public string Id { get; set; }
        public long ClientId { get; set; }
        public string ConsumerId { get; set; }
        public string ConsumerEmail { get; set; }
        public long SmartCheckConfigurationId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime? FinishedOn { get; set; }
        public List<ParringApplicant> ParringApplicants { get; set; }
        public string Self { get; set; }
    }

    public class ParringApplicant
    {
        public string ParringId { get; set; }
        public string ConsumerId { get; set; }
        public string ConsumerEmail { get; set; }
        public DateTime FinishedOn { get; set; }
    }

    public class BudgetDefinition
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class SmartCheckConfigurationRequestDto
    {
        public string Name { get; set; }
        public string CallbackUrl { get; set; }
        public Consent Consent { get; set; }
        public long BudgetDefinitionId { get; set; }
    }

    public class SmartCheckConfigurationResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string RedirectUrl { get; set; }
        public string CallbackUrl { get; set; }
        public Consent Consent { get; set; }
        public long BudgetDefinitionId { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Self { get; set; }
    }

    public class CollectionEnvelope<T>
    {
       public List<T> Collection { get; set; }
    }

    public class Consent
    {
        public string Header { get; set; }
        public string Text { get; set; }
    }
}
