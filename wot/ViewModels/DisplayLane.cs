using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wot.Services;

namespace wot.ViewModels
{
    public class DisplayLane
    {
        public List<PersonViewModel> People { get; set; }
        public List<PersonViewModel> Queue { get; set; }
        public bool IsPriorityLane { get; set; }
        public bool IsKioskLane { get; set; }
        public int LaneNumber { get; set; }
        public double RotationDelay { get; set; }
        public double LeftMargin { get; set; }
        public double RightMargin { get; set; }
        public double SectionWidth { get; set; }

        public double CanvasWidth { get; set; }
        public int TotalLanes { get; set; }

        public DisplayLane(double rotationDelay)
        {
            People = new List<PersonViewModel>();
            Queue = new List<PersonViewModel>();
            RotationDelay = rotationDelay;
        }

        public DisplayLane(double rotationDelay, int laneNumber, double canvasWidth, int totalLanes) : this(rotationDelay)
        {
            LaneNumber = laneNumber;
            CanvasWidth = canvasWidth;
            TotalLanes = totalLanes;
            SetMargins();
        }

        public async Task LoadNamesAsync(int currentCount, int defaultTakeCount, bool priority, string webServerUrl)
        {
            Console.WriteLine($"Loading new names {currentCount}");
            var service = new NameService(webServerUrl);
            People = await service.GetDistinct(currentCount, defaultTakeCount, priority);
        }

        public async Task<List<PersonViewModel>> UpdateQueueAsync(int currentCount, int defaultTakeCount, string webServerUrl)
        {
            Console.WriteLine($"Loading secondary new names {currentCount}");
            var service = new NameService(webServerUrl);
            Queue = await service.GetDistinct(currentCount, defaultTakeCount, IsPriorityLane);
            return Queue;
        }

        public void SetMargins()
        {
            if (IsPriorityLane)
            {
                LeftMargin = 0;
                RightMargin = CanvasWidth;
            }
            else
            {
                SectionWidth = LaneNumber != 0 ? CanvasWidth / TotalLanes : CanvasWidth;
                LeftMargin = LaneNumber != 0 ? SectionWidth * (LaneNumber - 1) : 0;
                RightMargin = LeftMargin + SectionWidth;
            }
        }
    }
}