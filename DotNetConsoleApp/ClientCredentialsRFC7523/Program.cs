using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using Newtonsoft.Json;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;

namespace Mbf.App
{
    public class Program
    {
        private static string IdentityServerUrl => "https://test-identity.monthio.com";
        private static string BudgetServerUrl => "https://test-budgets.monthio.com";
        private static string EskatServerUrl => "https://test-eskat.monthio.com";

        private static string ClientCredentialsId => "<INSERT Client credentials ID HERE>";

        static async Task Main(string[] args)
        {
            var httpClient = await SetupHttpClient();
            var getSessionSmartChecksResponse = await httpClient.GetAsync($"{BudgetServerUrl}/api/smart-check-sessions");
            if (getSessionSmartChecksResponse.IsSuccessStatusCode)
            {
                var response = getSessionSmartChecksResponse.Content.ReadAsStringAsync();
                Console.WriteLine("Ok");
                return;
            }
            
            Console.WriteLine("Failed");
        }

        private static async Task<HttpClient> SetupHttpClient()
        {
            var httpClient = new HttpClient();
            var discoveryDocument = await httpClient.GetDiscoveryDocumentAsync($"{IdentityServerUrl}/");

            var tokenClient = new HttpClient();

            var response = await tokenClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest()
            {
                ClientId = ClientCredentialsId,
                Address = discoveryDocument.TokenEndpoint,
                GrantType = OidcConstants.GrantTypes.ClientCredentials,
                Scope = "budgetApi eskatApi",
                ClientAssertion = new ClientAssertion
                {
                    Type = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
                    Value = GenerateJwtAsString(discoveryDocument.TokenEndpoint)
                }
            });

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Failed to authenticate using Client credentials");
            }

            httpClient.SetBearerToken(response.AccessToken);
            return httpClient;
        }

        private static string GenerateJwtAsString(string discoveryDocumentTokenEndpoint)
        {
            var tokenHandler = new JwtSecurityTokenHandler { TokenLifetimeInMinutes = 2 };
            var x509Certificate2 = new X509Certificate2("./cert.pfx", password: "");

            var securityToken = tokenHandler.CreateJwtSecurityToken(
                audience: discoveryDocumentTokenEndpoint,
                issuer: ClientCredentialsId,
                subject: new ClaimsIdentity(
                    new List<Claim>
                    {
                        new Claim("sub", ClientCredentialsId),
                        new Claim("jti", Guid.NewGuid().ToString())
                    }),
                signingCredentials: new SigningCredentials(new X509SecurityKey(x509Certificate2), "RS256"));

            return tokenHandler.WriteToken(securityToken);
        }
    }
}