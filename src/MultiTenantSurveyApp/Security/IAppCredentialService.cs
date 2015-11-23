using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading.Tasks;

namespace MultiTenantSurveyApp.Security
{
    public interface IAppCredentialService
    {
        Task<ClientAssertionCertificate> GetAsymmetricCredentials();
    }
}
