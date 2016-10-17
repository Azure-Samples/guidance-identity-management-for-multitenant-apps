// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.AspNetCore.Authentication;
using Tailspin.Surveys.Common;

namespace Tailspin.Surveys.Web.Security
{
    /// <summary>
    /// Extension methods for the ASP.NET BaseControlCOntext.
    /// </summary>
    internal static class BaseControlContextExtensions
    {
        /// <summary>
        /// Extension method to see if the current process flow is the sign up process.
        /// </summary>
        /// <param name="context">BaseControlContext from ASP.NET.</param>
        /// <returns>true if the user is signing up a tenant, otherwise, false.</returns>
        internal static bool IsSigningUp(this BaseControlContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));

            // Note - Due to https://github.com/aspnet/Security/issues/546, we cannot get to the authentication properties
            // from the context in the RedirectToAuthenticationEndpoint event to check for sign up.  This bug is currently
            // slated to be fixed in the RC2 timeframe.  When this is fixed, remove the workaround that checks the HttpContext

            string signupValue;
            object obj;
            // Check the HTTP context and convert to string
            if (context.HttpContext.Items.TryGetValue("signup", out obj))
            {
                signupValue = (string)obj;
            }
            else
            {
                // It's not in the HTTP context, so check the authentication ticket.  If it's not there, we aren't signing up.
                if ((context.Ticket == null) ||
                    (!context.Ticket.Properties.Items.TryGetValue("signup", out signupValue)))
                {
                    return false;
                }
            }

            // We have found the value, so see if it's valid
            bool isSigningUp;
            if (!bool.TryParse(signupValue, out isSigningUp))
            {
                // The value for signup is not a valid boolean, throw                
                throw new InvalidOperationException($"'{signupValue}' is an invalid boolean value");
            }

            return isSigningUp;
        }
    }
}
