# Postman

This is Postman request collection demonstrating how to get access to Monthio APIs using OAuth 2.0 token endpoint.

## Import

You can import this collection using your postman client. Set variables

# Authentication

To authenticate you have to use OAuth 2.0 token endpoint - `/connect/token`.

_Click on collection and choose edit. Then navigate to Variables tab to set variable values_

1. Retrieve refresh token from [portal](https://test-portal.monthio.com/api-integration) or [test-portal](https://test-portal.monthio.com/api-integration). Inside collection set variable `refresh_token` to this value.

2. When using test set `identity_app_url` to _test-identity.monthio.com_; when using production _identity.monthio.com_.

3. In response property `access_token` is your Bearer token that you can use in future requests Authorization header.
