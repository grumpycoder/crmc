using crmc.domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using wot.Services;

namespace wot.ViewModels
{
    public class DisplayLane
    {
        public List<PersonViewModel> People { get; set; }
        public List<PersonViewModel> Queue { get; set; }
        public bool IsKioskDisplay { get; set; }
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

        public async Task<List<PersonViewModel>> UpdateQueueAsync(int currentCount, int defaultTakeCount, bool priority, string webServerUrl)
        {
            Console.WriteLine($"Loading secondary new names {currentCount}");
            var service = new NameService(webServerUrl);
            Queue = await service.GetDistinct(currentCount, defaultTakeCount, priority);
            return Queue;
        }

        private void SetMargins()
        {
            SectionWidth = TotalLanes != 0 ? CanvasWidth / TotalLanes : CanvasWidth;
            LeftMargin = TotalLanes != 0 ? SectionWidth * (LaneNumber - 1) : 0;
            RightMargin = LeftMargin + SectionWidth;
        }

        public double RandomizeXAxis(Label label)
        {
            var position = RandomNumber(Convert.ToInt32(LeftMargin), Convert.ToInt32(RightMargin));
            if (LaneNumber == 0)
            {
                position = RandomNumber(Convert.ToInt32(0), Convert.ToInt32(CanvasWidth));
            }
            if (position + label.ActualWidth > CanvasWidth)
            {
                position = RandomNumber(Convert.ToInt32(LeftMargin), Convert.ToInt32(CanvasWidth - label.ActualWidth));
            }
            return position;
        }

        public double GetYAxis(Label label)
        {
            return 200.0;
        }

        private int RandomNumber(int min, int max)
        {
            if (max <= min) min = max - 1;
            return Random.Next(min, max);
        }

        private static readonly Random Random = new Random();
    }
}