# ASP.NET 5 authorization handlers

In ASP.NET 5, authorization logic can be encapsulated by writing an _authorization handler_, which implements the IAuthorizationHandler interface. This appendix shows some patterns for writing authorization handlers. We'll start by implementing `IAuthorizationHandler` directly (it's not much code), then show the more typical approach, which is to derive from the abstract `AuthorizationHandler` class.

## What is an authorization handler?

The [authorization APIs](https://docs.asp.net/en/latest/security/authorization/index.html) in ASP.NET 5 define three main abstractions:

-	**Authorization handler**. Makes authorization decisions
-	**Authorization requirement**. Defines a requirement that must be met, in order to authorize an action.
-	**Policy**. A collection of requirements. Policies can be registered by name.

To make an authorization decision, an authorization handler is passed three things:

-	A collection of requirements.
-	A claims principal that represents the current user (whether authenticated or anonymous).
-	An optional resource that is being acted upon.

The basic approach for a handler is to iterate over the collection of requirements. For each requirement, the handler does one of the following:

-	Mark the requirement as "succeeded".
-	Mark the requirement as "failed".
-	Skip this requirement. This is appropriate if the handler doesn't know about this type of requirement. Another handler may look at it.

Authorization succeeds if _every_ requirement succeeds and _no_ requirement fails.

> You can have a situation where one handler marks a requirement as "succeeded" but another marks it as "failed." If so, "failed" overrides.

## The simplest authorization handler

Let's start with the simplest case. Here is a handler and a requirement:

    public class SimpleRequirement : IAuthorizationRequirement { }

    public class SimpleAuthZHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationContext context)
        {
            foreach (var requirement in context.Requirements.OfType<SimpleRequirement>())
            {
                context.Succeed(requirement);
            }
            return Task.FromResult(0);
        }
    }

A requirement derives from **IAuthorizationRequirement**. This interface has no methods &mdash; it is only used to mark an object as a requirement.

A handler derives from **IAuthorizationHandler**, which defines a single method named **HandleAsync**. In this example, the handler loops through all of the requirements of type `SimpleRequirement`, and calls **Succeeded** for each one. In a real handler, there would be some logic at that point, to decide success or failure.

Register the handler on startup as follows:

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthorization(); // Adds the authorization service.
        services.AddScoped<IAuthorizationHandler, SimpleAuthZHandler>();
    }

Here is an MVC controller that uses `SimpleRequirement` to authorize an action:

    public class HomeController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        public HomeController(IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
        }

        public async Task<IActionResult> Index()
        {
            if (await authorizationService.AuthorizeAsync(User, resource: null, requirement: new SimpleRequirement()))
            {
                return View();
            }
            else
            {
                return new ChallengeResult();
            }
        }
    }

Notes:
-	Inject **IAuthorizationService** into the controller.
-	Call **AuthorizeAsync** and pass in the user, an optional resource (`null` in this example), and a requirement. Optionally, you can also pass in a list of requirements.
-	If **AuthorizeAsync** returns `true`, it means the user it authorized to perform this action. Otherewise, the user is not authorized.

## Deriving from AuthorizationHandler

Authorization handlers are often designed to handle a single requirement type. In that case, you can derive from the **AuthorizationHandler** class, which is strongly typed for a single requirement type.
The following example does the same thing as the previous version, except the base class handles iterating through the requirements collection.

    public class SimpleAuthZHandler2 : AuthorizationHandler<SimpleRequirement>
    {
        protected override void Handle(AuthorizationContext context, SimpleRequirement requirement)
        {
            context.Succeed(requirement);
        }
    }

The base class calls the **Handle** method once for every requirement of type `SimpleRequirement` in the `context.Requirements` collection. Notice that this version doesn't need to filter the requirements by type, because the base class does it for you.

## Resource-based authorization

The previous examples did not use resources. Here is an example of a resource-based authorization handler.

    // The resource type
    public class Document
    {
        // various properties...
    }

    // The operations that you can perform on the reource are requirements.    
    public class Operations
    {
        public static OperationAuthorizationRequirement Create = new OperationAuthorizationRequirement { Name = "Create" };
        public static OperationAuthorizationRequirement Read = new OperationAuthorizationRequirement { Name = "Read" };
        public static OperationAuthorizationRequirement Update = new OperationAuthorizationRequirement { Name = "Update" };
        public static OperationAuthorizationRequirement Delete = new OperationAuthorizationRequirement { Name = "Delete" };
    }

    public class DocumentAuthorizationHandler  : AuthorizationHandler<OperationAuthorizationRequirement, Document>
    {
        private List<OperationAuthorizationRequirement> GetAllowedOperations(ClaimsPrincipal user, Document document)
        {
            // Add actual logic here... For this sample, we say the user can read and update.
            return new List<OperationAuthorizationRequirement>() { Operations.Read, Operations.Update };       
        }

        protected override void Handle(AuthorizationContext context, OperationAuthorizationRequirement requirement, Document resource)
        {
            var allowedOperations = GetAllowedOperations(context.User,  resource);
            if (allowedOperations.Contains(requirement))
            {
                context.Succeed(requirement);
            }
        }
    }

In this example, the `Document` class is a resource. The requirements are operations on the resource (create, read, update, delete). These are defined by using the **OperationAuthorizationRequirement** class, which is part of the framework. Each operation on the resource is defined as an instance of **OperationAuthorizationRequirement**. The authorization handler derives from **AuthorizationHandler**, strongly typed to the requirement and resource.

To authorize an operation on a `Document`, call **AuthorizeAsync** and pass in the `Document` as the resource, and the operation as the requirement:

        public async Task<IActionResult> Index()
        {
            var resource = new Document();
            if (await authorizationService.AuthorizeAsync(User, resource, Operations.Read))
            {
                return View();
            }
            else
            {
                return new ChallengeResult();
            }
        }

## Requirements as handlers

It may seem redundant to define a requirement of type _T_ and a handler that acts on type _T_. Separating the handler from the requirement offers the most flexibility, but you can also combine the handler and the requirement into a single object.

    public class EmailRequirement : AuthorizationHandler<EmailRequirement>, IAuthorizationRequirement
    {
        protected override void Handle(AuthorizationContext context, EmailRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == ClaimTypes.Email))
            {
                context.Succeed(requirement);
            }
        }
    }

Here, `EmailRequirement` is both a requirement and a handler. Notice that it derives from **AuthorizationHandler** and specifies itself as the requirement type.

ASP.NET 5 has a special built-in authorization handler, called the pass-through handler, that will invoke any requirement that is also a handler.

The pass-through handler is automatically registered when you call **AddAuthorization** on startup. That means you don't need to register `EmailRequirement` as an authorization handler, because the pass-through handler will automatically invoke it.

    var result = await authorizationService.AuthorizeAsync(User, null, new EmailRequirement());
    // This works without registering EmailRequirement.

## Policies

A policy is a collection of requirements that can be registered by name. The following code creates a policy named "Policy1" with two requirements

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("Policy1",
                policy => policy.AddRequirements(new SimpleRequirement(), new EmailRequirement()));
        });
    }

Now you can authorize against this policy by using the policy name:

    var result = await authorizationService.AuthorizeAsync(User, "Policy1");

This code invokes all of the registered authorization handlers, and passes each handler the two requirements in the policy. (Handlers don't need to know about policies, just requirements.) All the requirements in the policy must succeed for authorization to succeed.

## Using Policies with [Authorize]

In MVC 6, the **[Authorize]** attribute can take a policy name:

        [Authorize(Policy="Policy1")]
        public IActionResult Index()
        {
            return View();
        }

You can't use the **[Authorize]** attribute to pass in a resource, but this approach is useful for policies that only examine the user.
