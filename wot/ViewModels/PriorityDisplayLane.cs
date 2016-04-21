using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wot.Services;

namespace wot.ViewModels
{
    public class PriorityDisplayLane : IDisplayLane
    {
        public List<PersonViewModel> People { get; set; }
        public List<PersonViewModel> Queue { get; set; }
        public int LaneIndex { get; set; }
        public double RotationDelay { get; set; }
        public double LeftMargin { get; set; }
        public double RightMargin { get; set; }
        public double LaneWidth { get; set; }

        public double CanvasWidth { get; set; }
        public int TotalLanes { get; set; }

        public PriorityDisplayLane(double rotationDelay)
        {
            People = new List<PersonViewModel>();
            Queue = new List<PersonViewModel>();
            RotationDelay = rotationDelay;
        }

        public PriorityDisplayLane(double rotationDelay, double canvasWidth) : this(rotationDelay)
        {
            CanvasWidth = canvasWidth;
            SetMargins();
        }

        public async Task LoadNamesAsync(int currentCount, int defaultTakeCount, string webServerUrl)
        {
            Console.WriteLine($"Loading new names {currentCount}");
            var service = new NameService(webServerUrl);
            People = await service.GetDistinct(currentCount, defaultTakeCount, true);
        }

        public async Task<List<PersonViewModel>> UpdateQueueAsync(int currentCount, int defaultTakeCount, string webServerUrl)
        {
            Console.WriteLine($"Loading secondary new names {currentCount}");
            var service = new NameService(webServerUrl);
            Queue = await service.GetDistinct(currentCount, defaultTakeCount, true);
            return Queue;
        }

        public void SetMargins()
        {
            LeftMargin = 0;
            RightMargin = CanvasWidth;
        }
    }
}