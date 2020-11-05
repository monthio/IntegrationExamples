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
            var client = await SetupMonthioIntegrationClient();
        }

        private static async Task<HttpClient> SetupMonthioIntegrationClient()
        {
            var client = new HttpClient();
            var identityServerUrl = "https://test-identity.monthio.com/";
            var discoveryDocument = await client.GetDiscoveryDocumentAsync(identityServerUrl);

            var tokenResponse = await client.RequestRefreshTokenAsync(
                new RefreshTokenRequest
                {
                    Address = discoveryDocument.TokenEndpoint,
                    RefreshToken = "xuAB0OmNiteR1zfVCy47T0o7Cz7GGMTfszedGwxWFvQ",
                    ClientId = "external_client",
                    Scope = "budgetApi"
                });

            var apiClient = new HttpClient();
            apiClient.SetBearerToken(tokenResponse.AccessToken);

            var budgetDefinitionsResponse = await apiClient.GetAsync("https://test-budgets.monthio.com/api/budget-definitions/available");
            var budgetDefinitions = JsonConvert.DeserializeObject<List<BudgetDefinition>>(await budgetDefinitionsResponse.Content.ReadAsStringAsync());

            var response = await apiClient.PostAsync("https://test-budgets.monthio.com/api/smart-check-configurations",
                new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new SmartCheckConfigurationRequestDto
                {
                    Name = "Smartcheck configuration from API",
                    CallbackUrl = "https://0bfa3777fadcbdd27080aa1cd80b5c0c.m.pipedream.net",
                    Consent = new Consent
                    {
                        Header = "Consent header",
                        Text = "Consent text"
                    },
                    BudgetDefinitionId = budgetDefinitions.First().Id
                }), Encoding.UTF8, "application/json"));
            var createdConfiguration = JsonConvert.DeserializeObject<SmartCheckConfigurationResponseDto>(await response.Content.ReadAsStringAsync());


            var getAllSmartCheckConfigurationResponse =
                await apiClient.GetAsync("https://test-budgets.monthio.com/api/smart-check-configurations");
            var allSmartCheckConfiguration = JsonConvert.DeserializeObject<List<SmartCheckConfigurationResponseDto>>(
                await getAllSmartCheckConfigurationResponse.Content.ReadAsStringAsync());

            var parsedAggregate = Newtonsoft.Json.JsonConvert.SerializeObject(new SmartCheckSessionRequestDto
            {
                ConsumerId = "my_persona_consumer_id",
                ConsumerEmail = "oap@monthio.com",
                SmartCheckConfigurationId = createdConfiguration.Id,

            });
            var jsonContent = new StringContent(parsedAggregate, Encoding.UTF8, "application/json");
            var createSessionSmartCheckResponse =
                await apiClient.PostAsync("https://test-budgets.monthio.com/api/smart-check-sessions", jsonContent);
            var createdSmartCheckSession =
                JsonConvert.DeserializeObject<SmartCheckSessionResponseDto>(await createSessionSmartCheckResponse
                    .Content.ReadAsStringAsync());

            //  Get all
            var getAllSmartCheckSessionRequest =
                await apiClient.GetAsync("https://test-budgets.monthio.com/api/smart-check-sessions");
            var allSmartCheckSessions = JsonConvert.DeserializeObject<List<SmartCheckSessionResponseDto>>(
                await getAllSmartCheckSessionRequest.Content.ReadAsStringAsync());

            Console.WriteLine(await response.Content.ReadAsStringAsync());
            return apiClient;
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
        public DateTime FinishedOn { get; set; }
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

    public class Consent
    {
        public string Header { get; set; }
        public string Text { get; set; }
    }
}
