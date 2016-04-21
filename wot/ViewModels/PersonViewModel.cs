using crmc.domain;
using System;

namespace wot.ViewModels
{
    public class PersonViewModel : Person
    {
        public int CurrentDisplayCount { get; set; }
        public DateTime NextDisplayTime { get; set; }
    }
}