using crmc.domain;
using System;

namespace wot.ViewModels
{
    public class PersonViewModel : Person
    {
        public int RotationCount { get; set; }
        public DateTime NextDisplayTime { get; set; }
        public DateTime LastDisplayTime { get; set; }
    }
}