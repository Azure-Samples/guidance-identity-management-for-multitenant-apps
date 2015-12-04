// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Tailspin.Surveys.Common.Configuration;

namespace Tailspin.Surveys.Data.Configuration
{
    public class ConfigurationOptions
    {
        public ConfigurationOptions()
        {
            Data = new DatabaseOptions();
            KeyVault = new KeyVaultOptions();
        }
        public string ClientId { get; set; }
        public DatabaseOptions Data { get; set; }

        public KeyVaultOptions KeyVault { get; set; }
    }
}
