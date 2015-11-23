# Authorization

In this guidance we'll examine two general approaches to authorization.

-	**Role-based authorization**. Authorize an action based on the roles assigned to a user. For example, some actions require an administrator role.
-	**Resource-based authorization**. Authorize an action based on a particular resource. For example, every resource has an owner. The owner can delete the resource; other users cannot.

A typical app will employ a mix of both. For example, to delete a resource, the user must be the resource owner _or_ an admin.

## Role-Based Authorization

In the Surveys application, we defined these roles:

•	Administrator. Can perform all CRUD operations on any survey that belongs to that tenant.
•	Creator. Can create new surveys
•	Reader. Can read any surveys that belong to that tenant

Roles apply to _users_ of the application. In the Surveys application, a user is either an administrator, creator, or reader.

The first question is how to assign and manage roles. We identified three main options:

-	Use Azure AD App Roles.
-	Use Azure AD security groups.
-	Manage roles entirely within the application.

### Using Azure AD App Roles

In this approach, The SaaS provider defines the application roles by adding them to the application manifest. An admin for the customer's AD directory assigns users to the roles. When a user signs in, the user's assigned roles are sent as claims.

> Note: If the customer has Azure AD Premium, the admin can assign a security group to a role, and members of the group will inherit the app role. This is a convenient way to manage roles, because the group owner doesn't need to be an AD admin.

Advantages:
-	Simple programming model.
-	Roles are specific to the application. The role claims for one application are not sent to another application.
-	If the customer removes the application from their AD tenant, the roles go away.
-	The application doesn't need any extra Active Directory permissions, other than reading the user's profile.

Drawbacks:
-	Customers without Azure AD Premium cannot assign security groups to roles. For these customers, all user assignments must be done by an AD administrator.

#### Implementation using Azure AD App Roles

**Define the roles.** The SaaS provider declares the app roles in the application manifest. For example, here are the roles that we defined for the Surveys app:

    "appRoles": [
      {
        "allowedMemberTypes": [
          "User"
        ],
        "description": "Creators can create Surveys",
        "displayName": "SurveyCreator",
        "id": "1b4f816e-5eaf-48b9-8613-7923830595ad",
        "isEnabled": true,
        "value": "SurveyCreator"
      },
      {
        "allowedMemberTypes": [
          "User"
        ],
        "description": "Administrators can manage the Surveys in their tenant",
        "displayName": "SurveyAdmin",
        "id": "c20e145e-5459-4a6c-a074-b942bbd4cfe1",
        "isEnabled": true,
        "value": "SurveyAdmin"
      }
    ],

The `value`  property appears in the role claim. The `id` property is the unique identifier for the defined role. Always generate a new GUID value for `id`.

**Assign users**. When a new customer signs up, after the application is registered in the customer's AD tenant, an AD admin for that tenant will assign users to roles.

>	As noted earlier, customers with Azure AD Premium can also assign security groups to roles.

The following screenshot from the Azure portal shows three users. Alice was assigned directly to a role. Bob inherited a role as a member of a security group named "Surveys Admin", which is assigned to a role. Charles is not assigned to any role.

![Assigned users](media/authorization/role-assignments.png)

> Note: Alternatively, the application can assign roles programmatically, using the Azure AD Graph API.  However, this requires the application to obtain write permissions for the customer's AD directory. An application with those permissions could do a lot of mischief &mdash; the customer is trusting the app not to mess up their directory. Many customers might be unwilling to grant this level of access.

**Get role claims**. When a user signs in, the application receives the user's assigned role(s) in a claim with type `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`.  

A user can have multiple roles, or no role. In your authorization code, don't assume the user has exactly one role claim. Instead, write code that checks whether a particular claim value is present:

    if (context.User.HasClaim(ClaimTypes.Role, "Admin"))

### Using Azure AD security groups

In this approach, roles are represented as AD security groups. The application assigns permissions to users based on their security group memberships.

Advantages:
-	For customers who do not have Azure AD Premium, this approach enables the customer to use security groups to manage role assignments.

Disadvantages
-	Complexity. Because every tenant sends different group claims, the app must keep track of which security groups correspond to which application roles, for each tenant.
-	If the customer removes the application from their AD tenant, the security groups are left in the AD tenant.

#### Implementation

In the application manifest, set the `groupMembershipClaims` property to "SecurityGroup". This is needed to get group membership claims from AAD.

    {
       // ...
       "groupMembershipClaims": "SecurityGroup",
    }

When a new customer signs up, the application instructs the customer to create security groups for the roles needed by the application. The customer then neesd to enter the group object IDs into the application. The application stores these in a table that maps group IDs to application roles, per tenant.

> Note: Alternatively, the application could create the groups programmatically, using the Azure AD Graph API.  This would be less error prone. However, it requires the application to obtain "read and write all groups" permissions for the customer's AD directory. Many customers might be unwilling to grant this level of access.

When a user signs in:

1.	The application receives the user's groups as claims. The value of each claim is the object ID of a group.
2.	Azure AD limits the number of groups sent in the token. If the number of groups exceeds this limit, Azure AD sends a special "overage" claim. If that claim is present, the application must query the Azure AD Graph API to get all of the groups to which that user belongs. For details, see [Authorization in Cloud Applications using AD Groups](http://www.dushyantgill.com/blog/2014/12/10/authorization-cloud-applications-using-ad-groups/), inder the section titled "Groups claim overage".
3.	The application looks up the object IDs in its own database, to find the corresponding application roles to assign to the user.
4.	The app adds a custom claim value to the user principal that expresses the application role. For example: "survey_role"="SurveyAdmin".

Authorization policies should use the custom role claim, not the group claim.

## Managing roles entirely in the application

With this approach, application roles are not stored in Azure AD at all. Instead, the application stores the role assignments for each user in its own DB &emdash; for example, using the RoleManager class in ASP.NET Identity.

Advantages:
-	The app has full control over the roles and user assignments.

Drawbacks:
-	More complex, harder to maintain.
- Cannot use AD security groups to manage role assignments.
- Stores user information in the application database, where it can get out of sync with the tenant's AD directory, as users are added or removed.   

There are many existing examples for this approach. For example, see [Create an ASP.NET MVC app with auth and SQL DB and deploy to Azure App Service](https://azure.microsoft.com/en-us/documentation/articles/web-sites-dotnet-deploy-aspnet-mvc-app-membership-oauth-sql-database/).

## Performing role-based authorization

Regardless of how you manage the roles, your authorization code will look similar. ASP.NET 5 introduces an abstraction called _authorization policies_. With this feature, you define authorization policies in code, and then apply those policies to controller actions. The policy is decoupled from the controller.

To define a policy, first create a class that implements `IAuthorizationRequirement`. It's easiest to derive from `AuthorizationHandler`. In the `Handle` method, examine the relevant claim(s).

Here is an example from the Tailspin Surveys application:

```
public class SurveyCreatorRequirement : AuthorizationHandler<SurveyCreatorRequirement>,
    IAuthorizationRequirement
{
    protected override void Handle(AuthorizationContext context, SurveyCreatorRequirement requirement)
    {
        if (context.User.HasClaim(ClaimTypes.Role, Roles.SurveyAdmin) ||
            context.User.HasClaim(ClaimTypes.Role, Roles.SurveyCreator))
        {
            context.Succeed(requirement);
            return;
        }
        context.Fail();
    }
}
```

> This code is located in /src/MultiTenantSurveyApp.Security/Policy/SurveyCreatorRequirement.cs

This class defines the requirement for a user to create a new survey. The user must be in the SurveyAdmin or SurveyCreator role.

In your startup class, define a named policy that includes one or more requirements. (If there are multiple requirements, the user must meet _each_ requirement to be authorized.) The following code defines two policies:

```
services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.RequireSurveyCreator, policy =>
    {
        policy.AddRequirements(new SurveyCreatorRequirement());
    });
    options.AddPolicy(PolicyNames.RequireSurveyAdmin, policy =>
    {
        policy.AddRequirements(new SurveyAdminRequirement());
    });
});
```

> This code is located in /src/MultiTenantSurveyApp/Startup.cs

Finally, to authorize an action in an MVC controller, set the policy in the Authorize attribute:

```
[Authorize(Policy = "SurveyCreatorRequirement")]
public IActionResult Create()
{
    // ...
}
```

In earlier versions of ASP.NET, you would set the **Roles** property on the attribute:

```
// old way
[Authorize(Roles = "SurveyCreator")]

```

This is still supported in ASP.NET 5, but it has some drawbacks compared with authorization policies:

-	It assumes a particular claim type. Policies can check for any claim type. Roles are just a type of claim.
-	The role name is hard-coded into the attribute. With policies, the authorization logic is all in one place, making it easier to update.
-	Policies enable more complex authorization decisions (e.g., age >= 21) that can't be expressed by simple role membership.

## Resource Based Authorization

_Resource based authorization_ occurs whenever the authorization depends on a specific resource that will be affected by an operation. In the Tailspin Surveys application, every survey has an owner and zero-to-many contributors.

-	The owner can read, update, delete, publish, and unpublish the survey.
-	The owner can assign contributors to the survey.
-	Contributors can read and update the survey.

Note that "owner" and "contributor" are not application roles; they are stored per survey, in the application database. To check whether a user can delete a survey, for example, the app checks whether the user is the owner for that survey.

In ASP.NET 5, you can implement resource-based authorization by creating a class that derives from **AuthorizationHandler** and overriding the **Handle** method.

```
public class SurveyAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Survey>
{
     protected override void Handle(AuthorizationContext context, OperationAuthorizationRequirement operation, Survey resource)
    {
    }
}
```

Notice that this class is strongly typed for Survey objects.  Register the class for DI on startup:

```
services.AddSingleton<IAuthorizationHandler>(factory =>
{
    return new SurveyAuthorizationHandler();
});
```

To perform authorization checks, use the **IAuthorizationService** interface, which you can inject into your controllers. The following code checks whether a user can read a survey:

```
if (await _authorizationService.AuthorizeAsync(User, survey, Operations.Read) == false)
{
    return new HttpStatusCodeResult(403);
}
```

Because we pass in a `Survey` object, this call will invoke the `SurveyAuthorizationHandler`.

In your authorization code, a good approach is to aggregate all of the user's role-based and resource-based permissions, then check the aggregate set against the desired operation.
Here is an example from the Surveys app. The application defines three permission types:

- Admin
- Contributor
- Creator
- Owner
- Reader

The application also defines a set of possible operations on surveys:

- Create
- Read
- Update
- Delete
- Publish
- Unpublsh

The following code creates a list of permissions, given a particular user and survey. Notice that this code looks at both the user's app roles, and the owner/contributor fields in the survey.

```
var permissions = new List<UserPermissionType>();
if (resource.TenantId == userTenantId)
{
    // The survey belongs to this user's tenant.
    if (context.User.HasClaim(ClaimTypes.Role, Roles.SurveyAdmin))
    {
        // SurveyAdmin can do anything with this survey, so short circuit.
        context.Succeed(operation);
        return;
    }
    // Otherwise add the role permission.
    if (context.User.HasClaim(ClaimTypes.Role, Roles.SurveyCreator))
    {
        permissions.Add(UserPermissionType.Creator);
    }
    else
    {
        permissions.Add(UserPermissionType.Reader);
    }
    // Check if the user is the resource owner.
    if (resource.OwnerId == userId)
    {
        permissions.Add(UserPermissionType.Owner);
    }
}
// Check if the user is a contributor. Contributors can be from other tenants.
if (resource.Contributors != null && resource.Contributors.Any(x => x.UserId == userId))
{
    permissions.Add(UserPermissionType.Contributor);
}
```

This code is located in /src/Multitenantsurveyapp.Security/Policy/Surveyauthorizationhandler.cs.

In a multi-tenant application, you must ensure that permissions don't "leak" to other tenant's data. In the Surveys app, the Contributor permission is allowed across tenants. (You can assign a user in another tenant as a contriubutor.) The other permission types are restricted to resources that belong to that user's tenant, so the code checks the tenant ID before granting those permission types. (The `TenantId` field as assigned when the survey is created.)

The next step is to check the operation (read, update, delete, etc) against the permissions. The Surveys app implements this step by using a lookup table of functions:

```
static readonly Dictionary<OperationAuthorizationRequirement, Func<List<UserPermissionType>, bool>> ValidateUserPermissions
    = new Dictionary<OperationAuthorizationRequirement, Func<List<UserPermissionType>, bool>>

    {
        { Operations.Create, x => x.Contains(UserPermissionType.Creator) },

        { Operations.Read, x => x.Contains(UserPermissionType.Creator) ||
                                x.Contains(UserPermissionType.Reader) ||
                                x.Contains(UserPermissionType.Contributor) ||
                                x.Contains(UserPermissionType.Owner) },

        { Operations.Update, x => x.Contains(UserPermissionType.Contributor) ||
                                x.Contains(UserPermissionType.Owner) },

        { Operations.Delete, x => x.Contains(UserPermissionType.Owner) },

        { Operations.Publish, x => x.Contains(UserPermissionType.Owner) },

        { Operations.UnPublish, x => x.Contains(UserPermissionType.Owner) }
    };
```

## Additional resources

-	[Authorization in a web app using Azure AD application roles & role claims](C:\Users\mwasson\pnp\MultiTenantSaaSGuidance\WordDocs\-	https:\azure.microsoft.com\en-us\documentation\samples\active-directory-dotnet-webapp-roleclaims) (sample)
-	[Authorization in Cloud Applications using AD Groups](http://www.dushyantgill.com/blog/2014/12/10/authorization-cloud-applications-using-ad-groups/)
-	[Roles based access control in cloud applications using Azure AD](http://www.dushyantgill.com/blog/2014/12/10/roles-based-access-control-in-cloud-applications-using-azure-ad/)
-	[Supported Token and Claim Types](https://azure.microsoft.com/en-us/documentation/articles/active-directory-token-and-claims/).  Describes the role and group claims in Azure AD.
-	[Understanding the Azure Active Directory application manifest](https://azure.microsoft.com/en-us/documentation/articles/active-directory-application-manifest/)
