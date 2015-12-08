// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tailspin.Surveys.Data.DataModels
{
    public class Survey
    {

        public Survey() : this(string.Empty)
        {
        }
        public Survey(string title)
        {
            Title = title;
        }
        public int Id { get; set; }

        [Required(ErrorMessage = "* You must provide a Title for the survey.")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        public int OwnerId { get; set; }

        public int TenantId { get; set; }

        public bool Published{ get; set; }

        // Navigation properties
        public User Owner { get; set; }
        public virtual ICollection<SurveyContributor> Contributors { get; set; } 
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<ContributorRequest> Requests { get; set; }

    }
}
