// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MultiTenantSurveyApp.Models
{
    public class ApiResult
    {
        public HttpResponseMessage Response { get; set; }

        public virtual int StatusCode => (int?)Response?.StatusCode ?? 0;

        public virtual bool Succeeded => Response?.IsSuccessStatusCode ?? false;
    }

    public class ApiResult<TKey> : ApiResult
    {
        public virtual TKey Item { get; set; }

        public static async Task<ApiResult<TKey>> FromResponseAsync(HttpResponseMessage response)
        {
            var result = new ApiResult<TKey> { Response = response };
            if (result.Succeeded)
            {
                var body = await response.Content.ReadAsStringAsync();
                result.Item = JsonConvert.DeserializeObject<TKey>(body);
            }

            return result;
        }
    }
}
