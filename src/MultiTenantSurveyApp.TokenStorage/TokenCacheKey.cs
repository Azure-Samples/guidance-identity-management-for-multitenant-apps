// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MultiTenantSurveyApp.TokenStorage
{
    public class TokenCacheKey
    {
        private string _key;
        private string _uniqueUserId;
        private string _clientId;

        // use this for on behalf of tokens
        public TokenCacheKey(string uniqueUserId, string clientId)
        {
            //TODO right now the guard class is not usable here due to circular references. 
            if (String.IsNullOrEmpty(uniqueUserId)) throw new ArgumentNullException("uniqueUserId");
            if (String.IsNullOrEmpty(clientId)) throw new ArgumentNullException("clientId");
            _uniqueUserId = uniqueUserId;
            _clientId = clientId;
            _key = string.Format("UserId:{0}::ClientId:{1}", uniqueUserId, clientId);

        }

        public string Key
        {
            get
            {
                return _key;
            }
        }
        public string UniqueUserId
        {
            get
            {
                return _uniqueUserId;
            }
        }
        public string ClientId
        {
            get
            {
                return _clientId;
            }
        }

        public override string ToString()
        {
            return this.Key;
        }
    }
}
