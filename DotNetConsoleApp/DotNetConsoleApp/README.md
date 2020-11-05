# DotNetConsoleApp example

This is an example console application that illustrate how you can integrate with Monthio's public API.

# About integration

First we need to authenticate against Monthio's identity server.
Replace `<YOUR REFRESH TOKEN>` with your secret token.

```
var tokenResponse = await client.RequestRefreshTokenAsync(
                new RefreshTokenRequest
                {
                    Address = discoveryDocument.TokenEndpoint,
                    RefreshToken = "<YOUR REFRESH TOKEN>",
                    ClientId = "external_client",
                    Scope = "budgetApi"
                });
```

In next step we attach the freshly created access token to our apiClient.

```
var apiClient = new HttpClient();
apiClient.SetBearerToken(tokenResponse.AccessToken);
```

That's it now it is possible to call Monthio's public API.

1. First we can check what kind of budget definitions are available for us;

```
var budgetDefinitionsResponse = await apiClient.GetAsync($"{budgetServerUrl}/api/budget-definitions/available");
```

2. Then we create a SmartCheckConfiguration. You can have multiple different configurations. They are used to express/configure how the SmartCheckSession should act, for example:

- Do we want to redirect consumer somewhere after flow has been finished;
- Do we want user to see consent;
- And others.

```
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
```

3. And finally we can construct a SmartCheckSession, which then enables the consumer to go through SmartCheck flow once.

```
var createSessionSmartCheckResponse = await apiClient.PostAsync($"{budgetServerUrl}/api/smart-check-sessions",
                    new StringContent(JsonConvert.SerializeObject(new SmartCheckSessionRequestDto
                {
                    ConsumerId = "consumer_id",
                    ConsumerEmail = "email@example.com",
                    SmartCheckConfigurationId = createdConfiguration.Id,

                }), Encoding.UTF8, "application/json"));
```
