using System;

namespace crmc.domain
{
    public class Person
    {
        public int Id { get; set; }
        public string AccountId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string EmailAddress { get; set; }
        public string Zipcode { get; set; }
        public bool? IsDonor { get; set; }
        public bool? IsPriority { get; set; }
        public decimal? FuzzyMatchValue { get; set; }
        public Guid? SortOrder { get; set; }
        public DateTime DateCreated { get; set; }

        public override string ToString()
        {
            return $"{Firstname} {Lastname}";
        }
    }
}