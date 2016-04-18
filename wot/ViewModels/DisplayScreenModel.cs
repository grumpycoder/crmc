using crmc.domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wot.Services;

namespace wot.ViewModels
{
    public class DisplayScreenModel
    {
        public List<PersonViewModel> People { get; set; }
        public List<PersonViewModel> Queue { get; set; }
        public bool IsKioskDisplay { get; set; }
        public int Section { get; set; }
        public double RotationDelay { get; set; }

        public DisplayScreenModel(double rotationDelay)
        {
            RotationDelay = rotationDelay;
        }

        public DisplayScreenModel(double rotationDelay, int section) : this(rotationDelay)
        {
            Section = section;
        }

        public async Task LoadNamesAsync(int currentCount, int defaultTakeCount, bool priority, string webServerUrl)
        {
            Console.WriteLine($"Loading primary new names {currentCount}");
            var service = new NameService(webServerUrl);
            People = await service.GetDistinct(currentCount, defaultTakeCount, priority);
        }

        public async Task<List<PersonViewModel>> UpdateQueueAsync(int currentCount, int defaultTakeCount, bool priority, string webServerUrl)
        {
            Console.WriteLine($"Loading secondary new names {currentCount}");
            var service = new NameService(webServerUrl);
            Queue = await service.GetDistinct(currentCount, defaultTakeCount, priority);
            return Queue;
        }
    }
}