# Authentication and sign-in

This chapter describes how a multitenant app can authenticate users from Azure Active Directory (Azure AD), using OpenID Connect (OIDC) to authenticate.

Our reference implementation is an ASP.NET 5 application. The application uses the built-in OpenID Connect middleware to perform the OIDC authentication flow. The following diagram shows what happens when the user signs in, at a high level.

![Authentication flow](media/authentication/auth-flow.png)

1.	The user clicks the "sign in" button in the app. This action is handled by an MVC controller.
2.	The MVC controller returns a ChallengeResult action.
3.	The middleware intercepts the ChallengeResult and creates a 302 response, which redirects the user to the Azure AD sign-in page. _(Authentication request)_
4.	The user authenticates with Azure AD.
5.	Azure AD sends an ID token to the application, via HTTP POST. _(Authentication response)_
6.	The middleware validates the ID token. At this point, the user is now authenticated inside the application.
7.	The middleware redirects the user back to application.

In terms of the OpenID Connect protocol, step 3 is the authentication request, and step 5 is the authentication response from Azure AD.

## Registering the application with AAD

To enable OpenID Connect, the SaaS provider registers their application within their Azure AD tenant.
To register the application, follow the steps in [Integrating Applications with Azure Active Directory](https://azure.microsoft.com/en-us/documentation/articles/active-directory-integrating-applications/), in the section "To register an application in the Azure Management Portal." In the **Configure** page, do the following:

-	Note the client ID.
-	Under "Application is Multi-Tenant", select **Yes**.
-	Set **Reply URL** to a URL where Azure AD will send the authentication response. You can use the base URL of your app.
  -	Note: The URL path can be anything, as long as the host matches your deployed app.
  -	You can set multiple reply URLs. During development, you can use a `localhost` address, for running the app locally.
-	Generate a client secret: Under **keys**, click on the drop down that says **Select duration** and pick either 1 or 2 years. The key will be visible when you click **Save**. Be sure to copy the value, because it's not shown again when you reload the configuration page.

## Configuring the OpenID Connect middleware in ASP.NET 5

This section describes how to configure the OIDC middleware for multitenant authentication.

In your startup class, add the OpenID Connect middleware:

```
app.UseOpenIdConnectAuthentication(options =>
{
    options.AutomaticAuthentication = true;
    options.ClientId = <<clientID>>;
    options.Authority = "https://login.microsoftonline.com/common/";
    options.CallbackPath = <<clientID>>;
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = false
    };

    options.Events = <<event callback class>>;
});
```

> Note: For more information about the startup class, see [Application Startup](https://docs.asp.net/en/latest/fundamentals/startup.html) in the ASP.NET 5 documentation.

Set the following middleware options:

- **ClientId**: The application's client ID, which you got when you registered the application in Azure AD.
-	**Authority**: For a multitenant application, set this to "https://login.microsoftonline.com/common/". This is the URL for the Azure AD common endpoint, which enables users from any Azure AD tenant to sign in.
- In **TokenValidationParameters**, set **ValidateIssuer** to false. That means the app will be responsible for validating the issuer value in the ID token. (The middleware still validates the token itself.) For more information about validating the issuer, see [Working with Claims](#working-with-claims).
-** CallbackPath**. Set this equal to the path in the Reply URL that you registered in Azure AD. For example, if the reply URL is `http://contoso.com/aadsignin`, **CallbackPath** should be `aadsignin`.
- **SignInScheme**. Set this to `CookieAuthenticationDefaults.AuthenticationScheme`. This setting means that after the user is authenticated, information about the user is stored in a cookie. This cookie is how the user stays logged in during the browser session.


In addition, you must add the Cookie Authentication middleware to the pipeline:

```
app.UseCookieAuthentication(options =>
{
    options.AutomaticAuthentication = true;
});
```

This middleware is responsible for writing the authentication ticket to a cookie, and then reading the cookie during subsequent page loads.

> See /src/MultiTenantSurveyApp/Startup.cs

## Authentication events

The Open ID Connect middleware raises a series of events during authentication, which your app can hook into.

Event | Description
------|------------
RedirectToAuthenticationEndpoint |	Called right before the middleware redirects to the authentication endpoint. You can use this event to modify the redirect URL; for example, to add request parameters. (For example, see [Adding the admin consent prompt](03-tenant-signup.md#adding-the-admin-consent-prompt).)
AuthorizationResponseReceived	|	Called when the middleware receives the authentication response from the IDP, but before the middleware has validated the response.  
AuthorizationCodeReceived	|	Called with the authorization code.
TokenResponseReceived	|	Called after the middleware gets an access token from the IDP. Applies only to authorization code flow.
AuthenticationValidated	|	Called after the middleware validates the ID token. At this point, the authentication ticket is valid and has a valid set of claims. You can use this event to perform additional validation on the claims, or to transform claims. See [Working with claims](#working-with-claims).
UserInformationReceived	|	Called if the middleware gets the user profile from the user info endpoint. Applies only to authorization code flow, and only when `GetClaimsFromUserInfoEndpoint = true` in the middleware options.
TicketReceived |	Called when authentication is completed. After this event is handled, the user is signed into the app.
AuthenticationFailed	|	Called if authentication fails. Use this event to handle authentication failures &mdash; for example, by redirecting to an error page.

To provide callbacks for the events, set the **Events** option on the middleware. There are two different ways to declare the event handlers:

**Inline with lambdas:**

```
app.UseOpenIdConnectAuthentication(options =>
{
    // Other options not shown.
    options.Events = new OpenIdConnectEvents
    {
        OnTicketReceived = (x) =>
        {


        // Handle event
        },
        // other events
    }
});
```

**Derive from OpenIdConnectEvents:**

```
public class SurveyAuthenticationEvents : OpenIdConnectEvents
{
    public override Task TicketReceived(TicketReceivedContext context)
    {
        // Handle event
    }
    // other events
}

// In Startup.cs:
app.UseOpenIdConnectAuthentication(options =>
{
    // Other options not shown.
    options.Events = new SurveyAuthenticationEvents();
});
```

The second approach is recommended if your event callbacks have any substantial logic, so they don't clutter your startup class. Our reference implementation uses this approach; see /src/MultiTenantSurveyApp/Security/SurveyAuthenticationEvents.cs.

## Understanding the OpenID Connect middleware

This section contain background information about the OpenID Connect middleware. The middleware hides most of these details, but it can be useful to understand what's actually happening.

Open ID Connect Endpoints. AAD supports [OpenID Connect Discovery](https://openid.net/specs/openid-connect-discovery-1_0.html), wherein the identity provider (IdP) returns a JSON metadata document from a well-known endpoint. The metadata document contains information such as:

-	The URL of the authorization endpoint. This is where the app redirects to authenticate the user.
-	The URL of the "end session" endpoint, where the app goes to log out the user.
-	The URL to get the signing keys, which the client uses to validate the OIDC tokens that it gets from the IdP.

By default, the OIDC middleware knows how to fetch this metadata. You just set the **Authority** property, and the middleware constructs the URL for the metadata.

Every AAD tenant has its own metadata endpoint, which you would use for single-tenant sign on. In addition, AAD defines a "Common" endpoint, which enables users of a multitenant app to sign in from any AAD tenant. (For more information, see [this blog post](http://www.cloudidentity.com/blog/2014/08/26/the-common-endpoint-walks-like-a-tenant-talks-like-a-tenant-but-is-not-a-tenant/).) The Common endpoint has its own ODIC metadata document. The URL of the Common endpoint is `https://login.microsoftonline.com/common/oauth2/authorize`.

**Authentication Flow**. By default, the OIDC middleware uses _hybrid_ flow with _form post response mode_.

-	Hybrid flow means the client can get an ID token and an authorization code in the same round-trip to the authorization server.
-	Form post reponse mode means the authorization server uses an HTTP POST request to send the ID token and authorization code to the app. The values are form-urlencoded (content type = "application/x-www-form-urlencoded").

When the OIDC middleware redirects to the authorization endpoint, the redirect URL includes all of the query string parameters needed by OIDC. Among these are:

-	client_id. This value is set in the ClientId option
-	scope = "openid profile", which means it's an OIDC request and we want the user's profile.
-	response_type  = "code id_token".  This specifies hybrid flow.
-	response_mode = "form_post". This specifies form post response.

**Sign-in Action**. To trigger the authentication flow in MVC 6, return a ChallengeResult from the contoller:

```
[AllowAnonymous]
public IActionResult SignIn()
{
    return new ChallengeResult(
        OpenIdConnectDefaults.AuthenticationScheme,
        new AuthenticationProperties
        {
            RedirectUri = Url.Action("SignInCallback", "Account")
        });
}
```

This causes the middleware to return a 302 (Found) response that redirects to the authentication endpoint.

**Authentication Ticket**. If authentication succeeds, the OIDC middleware creates an authentication ticket, which contains a claims principal that holds the user's claims. You can access the ticket inside the **AuthenticationValidated** or **TicketReceived** event.

> Note: At this point in the process, `HttpContext.User` is _not_ the authenticated user. Until the entire authentication flow is completed, `HttpContext.User` still holds an anonymous principal.
After the **TicketReceived** event, the OIDC middleware calls the cookie middleware to persist the authentication ticket. At that point, the app redirects again. That's when the cookie middleware deserializes the ticket and sets `HttpContext.User` to the authenticated user.

## Working with Claims

When a user signs in, Azure AD sends an ID token that contains a set of claims about the user. A claim is simply a piece of information, expressed as a key/value pair. For example, "email=bob@contoso.com".  Claims have an issuer -- in this case, Azure AD -- which is the entity that authenticates the user and creates the claims. You trust the claims because you trust the issuer. (Conversely, if you don't trust the issuer, don't trust the claims!)
Note: In OpenID Connect, the set of claims that you get is controlled by the scope parameter of the authentication request. See http://nat.sakimura.org/2012/01/26/scopes-and-claims-in-openid-connect/. However, Azure AD issues a limited set of claims through OpenID Connect - see https://azure.microsoft.com/en-us/documentation/articles/active-directory-token-and-claims/ - If you want more information about the user, you'll need to use the Azure AD Graph API.
At a high level:

1.	The user authenticates.
2.	The IDP sends a set of claims.
3.	The app normalizes or augments the claims. [Optional]
4.	The app uses the claims to make authorization decisions.

Here are some of the claims from AAD that an app might typically care about:

Claim type in ID token |	Description |	Example
-----------------------|--------------|---------
aud | Who the token was issued for. This will be the application's client ID. Generally, you shouldn't need to worry about this claim, because the middleware automatically validates it.	 | "91464657-d17a-4327-91f3-2ed99386406f"
groups	 | A list of AAD groups of which the user the user is a member.  | ["93e8f556-8661-4955-87b6-890bc043c30f","fc781505-18ef-4a31-a7d5-7d931d7b857e" ]
iss	 | The issuer of the OIDC token. (See [OIDC spec](http://openid.net/specs/openid-connect-core-1_0.html#IDToken)) | "https://sts.windows.net/b9bd2162-77ac-4fb2-8254-5c36e9c0a9c4/"
name	| The user's display name. | "Alice A."
oid	| The object identifier for the user in AAD. This value is the immutable and non-reusable identifier of the user. Use this value, not email, as a unique identifier for users; email addresses can change. If you use the Azure AD Graph API in your app, object ID is that value used to query profile information. | "59f9d2dc-995a-4ddf-915e-b3bb314a7fa4"
roles	| A list of app roles for the user.	| ["SurveyCreator"]
tid	| Tenant ID. This value is a unique identifier for the tenant in Azure AD. | "b9bd2162-77ac-4fb2-8254-5c36e9c0a9c4"
unique_name	| A human readable display name of the user. | "alice@contoso.com"
upn	| User principal name	| "alice@contoso.com"

This table lists the claim types as they appear in the ID token. In ASP.NET 5, the OpenID Connect middleware converts some of the claim types when it populates the Claims collection for the user principal:

-	oid > "http://schemas.microsoft.com/identity/claims/objectidentifier"
-	tid > "http://schemas.microsoft.com/identity/claims/tenantid"
-	unique_name > "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
-	upn > "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn"

### Claims Transformations

During the authentication flow, you might want to modify the claims that you get from the IDP. In ASP.NET 5, you can perform claims transformation inside of the **AuthenticationValidated** event from the OpenID Connect middleware.

Any claims that you add during AuthenticationValidated are stored in the session authentication cookie. They don't get pushed back to Azure AD.

Here are some examples of claims transformation:

-	**Claims normalization**, or making claims consistent across users. This is particularly relevant if you are getting claims from multiple IDPs, which might use different claim types for similar information.
For example, Azure AD sends a "upn" claim that contains the user's email. Other IDPs might send an "email" claim. The following code converts the "upn" claim into an "email" claim:

  ```
  var email = principal.FindFirst(ClaimTypes.Upn)?.Value;
  if (!string.IsNullOrWhiteSpace(email))
  {
      identity.AddClaim(new Claim(ClaimTypes.Email, email));
  }
  ```

- Adding **default claim values** for claims that aren't present -- for example, assigning a user to a default role. In some cases this can simplify authorization logic.
-	Adding **custom claim types**, to represent application-specific information about the user. For example, you might store some information about the user in a database. You could create a custom claim for that information, adding it to the authentication ticket. The advantage of this approach is that the claim gets stored in the user cookie, so you only need to get it from the database once per login session.
After the authentication flow is complete, the claims are available in the HttpContext.User. At this point you should generally treat them as a read-only collection - i.e., use them only to make authorization decisions.

## Issuer Validation
In OpenID Connect, the issuer claim ("iss") identifies the IDP that issued the ID token. Part of the OIDC authentication flow is to verify that the issuer claim matches the actual issuer. The OIDC middleware handles this for you.

In Azure AD, the issuer value is unique per AD tenant ("https://sts.windows.net/<tenantID>"). Therefore, an application should do an additional check, to make sure the issuer represents a tenant that is allowed to sign in to the app.

For a single-tenant application, you can just check that the issuer is your own tenant. In fact, the OIDC middleware does this automatically. In a multi-tenant app, you need to allow for multiple issuers, corresponding to the different tenants. Here is a general approach to use:

-	In the OIDC middleware options, set **ValidateIssuer** to false. This turns off the automatic check.
-	When a tenant signs up, store the tenant and the issuer in your user DB.
-	Whenever a user signs in, look up the issuer in the database. If the issuer isn't found, it means that tenant hasn't signed up. You can redirect them to a sign up page.
- For a more detailed description, see [Sign-up and tenant onboarding](03-tenant-signup.md).

You could also blacklist certain tenants; for example, for customers that didn't pay their subscription.

## Using claims for authorization

With claims, a user's identity is no longer a monolithic entity. For example, a user might have an email address, phone number, birthday, gender, etc. Maybe the user's IdP stores all of this information. But when you authenticate the user, you'll typically get a subset of these as claims. In this model, the user's identity is simply a bundle of claims. When you make authorization decisions about a user, you will look for particular sets of claims. In other words, the question "Can user X perform action Y" ultimately becomes "Does user X have claim Z".

Here are some basic patterns for checking claims.

- 	Check that the user has a particular claim with a particular value:

          if (User.HasClaim(ClaimTypes.Role, "Admin")) { ... }

    This code checks whether the user has a Role claim with the value "Admin". It correctly handles the case where the user has no Role claim or multiple Role claims.

    The **ClaimTypes** class defines constants for commonly-used claim types. However, you can use any string value for the claim type.

-	Get a single value for a claim type, when you expect there to be at most one value:

         string email = User.FindFirst(ClaimTypes.Email)?.Value;

-	Get all the values for a claim type:

        IEnumerable<Claim> groups = User.FindAll("groups");

For more details about using claims in authorization, see [Authorization](04-authorization.md).
