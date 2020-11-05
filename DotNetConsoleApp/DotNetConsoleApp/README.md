# Hello

````var tokenResponse = await client.RequestRefreshTokenAsync(
                new RefreshTokenRequest
                {
                    Address = discoveryDocument.TokenEndpoint,
                    RefreshToken = "<YOUR REFRESH TOKEN>",
                    ClientId = "external_client",
                    Scope = "budgetApi"
                });
                ```
````
