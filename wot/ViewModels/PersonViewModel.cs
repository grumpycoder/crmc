using crmc.domain;
using System;

namespace wot.ViewModels
{
    public class PersonViewModel : Person
    {
        public int CurrentDisplayCount { get; set; }
        public DateTime NextDisplayTime { get; set; }

        public bool IsFirstRun
        {
            get
            {
                return CurrentDisplayCount == 0;
            }
        }
    }
}