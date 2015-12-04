// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Tailspin.Surveys.Web.Security
{
    /// <summary>
    /// Exception occurs when token in TokenCache is expired/ missing.
    /// </summary>
    public class AuthenticationException : Exception
    {
        /// <summary>
        /// Initializes with defaults.
        /// </summary>
        public AuthenticationException()
        {
        }

        /// <summary>
        /// Initializes with a specified error message.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public AuthenticationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes with a specified error 
        /// message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">
        /// <para>The exception that is the cause of the current exception. If the innerException parameter 
        /// is not a null reference, the current exception is raised in a catch block that handles the inner exception.</para>
        /// </param>
        public AuthenticationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
