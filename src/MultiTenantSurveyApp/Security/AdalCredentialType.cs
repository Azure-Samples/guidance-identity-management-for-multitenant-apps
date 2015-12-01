using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantSurveyApp.Security
{
    /// <summary>
    /// Type of credentials that can be stored in <see cref="MultiTenantSurveyApp.Security.AdalCredential"/> instances.
    /// </summary>
    public enum AdalCredentialType
    {
        /// <summary>
        /// A <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential"/> is stored in the <see cref="MultiTenantSurveyApp.Security.AdalCredential"/>.
        /// </summary>
        ClientCredential,

        /// <summary>
        /// A <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.ClientAssertionCertificate"/> is stored in the <see cref="MultiTenantSurveyApp.Security.AdalCredential"/>.
        /// </summary>
        ClientAssertionCertificate
    }
}
