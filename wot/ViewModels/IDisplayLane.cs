﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace wot.ViewModels
{
    public interface IDisplayLane
    {
        double CanvasWidth { get; set; }
        bool IsKioskLane { get; set; }
        bool IsPriorityLane { get; set; }
        int LaneIndex { get; set; }
        double LaneWidth { get; set; }
        int TotalLanes { get; set; }
        double LeftMargin { get; set; }
        double RightMargin { get; set; }
        double RotationDelay { get; set; }
        List<PersonViewModel> People { get; set; }
        List<PersonViewModel> Queue { get; set; }

        Task LoadNamesAsync(int currentCount, int defaultTakeCount, bool priority, string webServerUrl);

        Task<List<PersonViewModel>> UpdateQueueAsync(int currentCount, int defaultTakeCount, string webServerUrl);

        void SetMargins();
    }
}