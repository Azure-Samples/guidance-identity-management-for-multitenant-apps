// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Tailspin.Surveys.Common
{
    /// <summary>
    /// utility class to find certs and export them into byte arrays
    /// </summary>
    public static class CertificateUtility
    {
        /// <summary>
        /// Finds the cert having thumbprint supplied from store location supplied
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="storeLocation"></param>
        /// <param name="thumbprint"></param>
        /// <param name="validationRequired"></param>
        /// <returns>X509Certificate2</returns>
        public static X509Certificate2 FindCertificateByThumbprint(StoreName storeName, StoreLocation storeLocation, string thumbprint, bool validationRequired)
        {
            Guard.ArgumentNotNullOrWhiteSpace(thumbprint, nameof(thumbprint));

            var store = new X509Store(storeName, storeLocation);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                var col = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validationRequired);
                if (col == null || col.Count == 0)
                {
                    throw new ArgumentException("certificate was not found in store");
                }

                return col[0];
            }
            finally
            {
#if NET451
                // IDisposable not implemented in NET451
                store.Close();
#else
                // Close is private in DNXCORE, but Dispose calls close internally
                store.Dispose();
#endif
            }
        }

        /// <summary>
        ///Finds the cert having thumbprint supplied defaulting to the personal store of currrent user. 
        /// </summary>
        /// <param name="thumbprint"></param>
        /// <param name="validateCertificate"></param>
        /// <returns>X509Certificate2</returns>
        public static X509Certificate2 FindCertificateByThumbprint(string thumbprint, bool validateCertificate)
        {
            return FindCertificateByThumbprint(StoreName.My, StoreLocation.CurrentUser, thumbprint, validateCertificate);
        }

        /// <summary>
        /// Exports the cert supplied into a byte arrays and secures it with a randomly generated password. 
        ///</summary>
        /// <param name="cert"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static byte[] ExportCertificateWithPrivateKey(X509Certificate2 cert, out string password)
        {
            Guard.ArgumentNotNull(cert, nameof(cert));
            password = Convert.ToBase64String(Encoding.Unicode.GetBytes(Guid.NewGuid().ToString("N")));
            return cert.Export(X509ContentType.Pkcs12, password);
        }
    }
}

