// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

namespace Tailspin.Surveys.Web.Models
{
    public class AdminGroupViewModel
    {
        [Required]
        public string EmailAddress { get; set; }

        [Required]
        public string GroupAction { get; set; }
    }
}
