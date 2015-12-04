using System.Security.Cryptography.X509Certificates;

namespace Tailspin.Surveys.Common.Configuration
{
    public class AsymmetricEncryptionOptions
    {
        public AsymmetricEncryptionOptions()
        {
            StoreName = StoreName.My;
            StoreLocation = StoreLocation.CurrentUser;
            ValidationRequired = false;
        }
        public string CertificateThumbprint { get; set; }
        public StoreName StoreName { get; set; }
        public StoreLocation StoreLocation { get; set; }
        public bool ValidationRequired { get; set; }
    }
}
