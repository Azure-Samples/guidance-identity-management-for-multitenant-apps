// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tailspin.Surveys.Web.Models
{
    /// <summary>
    /// This class acts as a wrapper to instances of <see cref="HttpResponseMessage"/>. 
    /// This class provides public delegates that help determine the status and 
    /// successfulness of the response message.
    /// </summary>
    public class ApiResult
    {
        public HttpResponseMessage Response { get; set; }

        public virtual int StatusCode => (int?)Response?.StatusCode ?? 0;

        public virtual bool Succeeded => Response?.IsSuccessStatusCode ?? false;
    }

    /// <summary>
    /// This class provides a type specific <see cref="ApiResult"/>. 
    /// The Item property provides the typed payload.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class ApiResult<TKey> : ApiResult
    {
        public virtual TKey Item { get; set; }

        /// <summary>
        /// This static method returns an instance of <see cref="ApiResult&lt;TKey&gt;"/> based on the
        /// values in the <see cref="HttpResponseMessage"/> parameter.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> parameter</param>
        /// <returns>An <see cref="ApiResult&lt;TKey&gt;"/> instance containing the payload in the Item property</returns>
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
