# Client credentials RFC723

Example console app illustrates how to retrieve access token using signed JWT and client credentials. Grant type - `client_credentials`.

`Program.cs` contains example implementation.

### 1. Upload certificate public key to Monthio portal

1. Go to https://test-portal.monthio.com/client-credential-list
2. Create new client credential with type certificate, _upload only the public key part, remove headers and all whitespace_
3. Take a note of ID value of the just created client credential

### 2. Retrieve access token using just created client credential

1. Create JWT with payload like this. [More about payload here](https://datatracker.ietf.org/doc/html/rfc7523#section-4);

**Example JWT payload**

```json
{
  "sub": "0631450d-e308-4397-be6b-5203fc147f60",
  "jti": "0234d26a-4f91-472e-becc-3c4a5f2d6cb1",
  "nbf": 1643978320,
  "exp": 1643978440,
  "iat": 1643978320,
  "iss": "0631450d-e308-4397-be6b-5203fc147f60",
  "aud": "https://test-identity.monthio.com/connect/token"
}
```

2. Sign the JWT with your `private key`;
   1. A private key can be generated with `openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -sha256 -days 365` use `-nodes` for passwordless file
   1. _Optional_ `openssl pkcs12 -inkey key.pem -in cert.pem -export -out archive.pfx` can be used to change the pem key to pfx archive format. This is needed for this specific C# integration example
3. Make request to the token endpoint;
4. Retrieve `access token` from the response;

### 3. Start calling Monthio API

1. Add the access token as `Bearer` to the auth header;
2. Call Monthio API.
