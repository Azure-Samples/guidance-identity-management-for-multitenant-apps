![Microsoft patterns & practices](http://pnp.azurewebsites.net/images/pnp-logo.png)
# Building Multi-Tenant SaaS Applications on Windows Azure

## Part 1: Identity Management

To get started with the reference implementation, see [Running the Surveys application](docs/running-the-app.md).

- [Introduction](docs/01-intro.md)
- [Authentication with Azure AD](docs/02-authentication.md)
    - How to authenticate users from Azure Active Directory (Azure AD), using OpenID Connect (OIDC) to authenticate
- [Sign-up and tenant onboarding](docs/03-tenant-signup.md)
    - How to implement a sign-up process that allows a customer to sign up their organization for your application
- [Authorization](docs/04-authorization.md)
    - Role-based authorization
    - Resource-based authorization
- [Federating with a customer's AD FS](docs/05-adfs.md)
    - How to authenticate via Active Directory Federation Services (AD FS), in order to federate with a customer's on-premise Active Directory.
- Appendixes
    - [Authorization APIs in ASP.NET 5](docs/appendixes/aspnet5-authorization.md)
