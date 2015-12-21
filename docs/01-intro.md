# Introduction to identity management in multitenant SaaS applications

## What is multitenancy?

A _tenant_ is a group of users. In a SaaS applicaton, the tenant is a subscriber or customer of the application. _Multitenancy_ is an architecture where multiple tenants share the same physical instance of the app. Although tenants share physical resources (such as VMs or storage), each tenant gets its own logical instance of the app.

Tenants can have multiple users. This is what makes an application multi-_tenant_ as opposed to just multi-_user_. Often, application data is shared among the users within a tenant, but not with other tenants. Configuration settings might also be applied per tenant.

![Multitenant](media/intro/multitenant.png)

_Example: Tailspin sells subscriptions to its SaaS application. Contoso and Fabrikam sign up for the app. Employees of Contoso can log into the app and access Contoso’s data, but not Fabrikam’s data, and vice-versa._

Compare this architecture with a single-tenant architecture, where each tenant has a dedicated physical instance. In a single-tenant architecture, you add tenants by spinning up new instances of the app.

![Single tenant](media/intro/single-tenant.png)

### Multitenancy and horizontal scaling

To achieve scale in the cloud, it’s common to add more physical instances. This is known as _horizontal scaling_ or _scaling out_. Consider a web app. To handle more traffic, you can add more server VMs and put them behind a load balancer. Each VM runs a separate physical instance of the web app.

![Load balancing a web site](media/intro/load-balancing.png)

The crucial point is that any request can be routed to any instance. Together, the system functions as a single logical instance. You can tear down a VM or spin up a new VM, without affecting users.

In this architecture, each physical instance is multi-tenant, and you scale by adding more instances. If one instance goes down, it should not affect any tenant.

## Identity management in a multitenant app

Goals:

- Users sign in with their organization credentials.
- Users within the same organization are all part of the same tenant.
- When a user signs in, the application knows which tenant the user belongs to.

_Example: Alice, an employee at Contoso, navigates to the application in her browser and clicks the “Log in” button. She is redirected to a login screen where she enters her corporate credentials (username and password). At this point, she is logged into the app as alice@contoso.com._

In this guindance, we consider the following scenarios for organizational sign-in, depending on where the customer stores their user profiles:

-	[Azure Active Directory](https://azure.microsoft.com/en-us/documentation/services/active-directory/) (Azure AD).
    - This scenario includes Office365 and Dynamics CRM tenants.
-	On-premise Active Directory (AD). In this case, there are two options:
    - The customer uses [Azure AD Connect](https://azure.microsoft.com/en-us/documentation/articles/active-directory-aadconnect/) to sync their on-premise AD with Azure AD. This is the simplest option.
    - The SaaS provider federates with the customer's AD through Active Directory Federation Services (AD FS). See [Federating with a customer's AD FS](09-adfs.md).
