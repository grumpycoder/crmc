using crmc.domain;
using System;

namespace web.ViewModels
{
    public class PeopleSearchViewModel : Pager<Person>
    {
        public string AccountId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string EmailAddress { get; set; }
        public string Zipcode { get; set; }
        public bool? IsDonor { get; set; }
        public bool? IsPriority { get; set; }
        public decimal? FuzzyMatchValue { get; set; }
        public DateTime? DateCreated { get; set; }
    }
}