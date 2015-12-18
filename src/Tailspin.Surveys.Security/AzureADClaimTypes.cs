using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tailspin.Surveys.Security
{
    public static class AzureADClaimTypes
    {
        public const string TenantId = "http://schemas.microsoft.com/identity/claims/tenantid";
        public const string ObjectId = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public const string Name = "name";
    }
}
