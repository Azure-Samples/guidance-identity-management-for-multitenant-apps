# Identity Management for Multitenant Applications in Microsoft Azure

This sample is a multitenant web application, called Surveys, that allows users to create online surveys. The sample demonstrates some key concerns when managing user identities in a multitenant application, including sign-up, authentication, authorization, and app roles.

To run this sample, see [How to run the Tailspin Surveys sample application][running-the-app].

We also created a set of [written guidance][guidance] to accompany the sample. The written guidance and the sample are designed to complement each other.

Here are the main scenarios covered in both the guidance and the sample:

- [Authentication using Azure Active Directory (Azure AD) and OpenID Connect][authn]
- [Working with claims-based identities][claims]
- [Tenant onboarding (signup)][signup]
- [Application roles][app-roles]
- [Role-based and resource-based authorization][authz]
- [Authenticating in a backend web API][web-api]
- [Caching OAuth tokens in a distributed cache][token-cache]
- [Reading app configuration settings from Azure Key Vault][key-vault]  

<!-- links -->

[guidance]: https://azure.microsoft.com/documentation/articles/guidance-multitenant-identity
[authn]: https://azure.microsoft.com/documentation/articles/guidance-multitenant-identity-authenticate
[claims]: https://azure.microsoft.com/documentation/articles/guidance-multitenant-identity-claims
[signup]: https://azure.microsoft.com/documentation/articles/guidance-multitenant-identity-signup
[authz]: https://azure.microsoft.com/documentation/articles/guidance-multitenant-identity-authorize
[app-roles]: https://azure.microsoft.com/documentation/articles/guidance-multitenant-identity-app-roles
[token-cache]: https://azure.microsoft.com/documentation/articles/guidance-multitenant-identity-token-cache
[web-api]: https://azure.microsoft.com/documentation/articles/guidance-multitenant-identity-web-api
[key-vault]: https://azure.microsoft.com/documentation/articles/guidance-multitenant-identity-key-vault
[running-the-app]: docs/running-the-app.md
