// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Tailspin.Surveys.Common;
using System;

namespace Tailspin.Surveys.TokenStorage.Redis
{
    /// <summary>
    /// Key to be used for RedisTokenCache. 
    /// We want to always make use of the clientId and UserIdentifier as the key in redis to isolate different apps and users
    /// </summary>
    public class TokenCacheKey
    {
        private string _key;
        private string _uniqueUserId;
        private string _clientId;

        // use this for on behalf of tokens
        public TokenCacheKey(string uniqueUserId, string clientId)
        {
            Guard.ArgumentNotNullOrEmpty("uniqueUserId", uniqueUserId);
            Guard.ArgumentNotNullOrEmpty("clientId", clientId);

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
