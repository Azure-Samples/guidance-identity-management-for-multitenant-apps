// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Tailspin.Surveys.Common
{
    /// <summary>
    /// A static helper class that includes various parameter checking routines.
    /// </summary>
    public static class Guard
    {
        /// <summary>
        /// Throws <see cref="ArgumentNullException"/> if the given argument is null.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value is null.</exception>
        /// <param name="argumentValue">The argument value to test.</param>
        /// <param name="argumentName">The name of the argument to test.</param>
        public static void ArgumentNotNull(object argumentValue, string argumentName)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Throws an exception if the tested string argument is null or an empty string.
        /// </summary>
        /// <exception cref="ArgumentNullException">The string value is null.</exception>
        /// <exception cref="ArgumentException">The string is empty.</exception>
        /// <param name="argumentValue">The argument value to test.</param>
        /// <param name="argumentName">The name of the argument to test.</param>
        public static void ArgumentNotNullOrWhiteSpace(string argumentValue, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(argumentValue))
            {
                throw new ArgumentException($"{argumentName} cannot be null, empty, or only whitespace.");
            }
        }
    }
}
