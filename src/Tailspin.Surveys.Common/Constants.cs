// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Tailspin.Surveys.Common
{
    public static class Constants
    {
        public static readonly string IssuerFormat = "https://sts.windows.net/{0}/";
        public static readonly string AuthEndpointPrefix = "https://login.microsoftonline.com/{0}";
        public const int DefaultPageSize = 100;
        public const int MaxPageSize = 1000;
    }
}
