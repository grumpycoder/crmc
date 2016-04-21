using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using wot.ViewModels;

namespace wot
{
    public class DisplayElement
    {
        private static readonly Random Random = new Random();

        public PersonViewModel Person { get; set; }
        public IDisplayLane Lane { get; set; }
        public Label Label { get; set; }
        public Border Border { get; set; }
        public double TotalCanvasWidth { get; set; }
        public double XAxis { get; set; }
        public double YAxis { get; set; }
        public double TotalTime { get; set; }
        private double _currentTime;

        private readonly List<Color> _fontColors = new List<Color>
        {
            Color.FromRgb(205, 238, 207),
            Color.FromRgb(247, 231, 245),
            Color.FromRgb(213, 236, 250),
            Color.FromRgb(246, 244, 207),
            Color.FromRgb(246, 227, 213)
        };

        public DisplayElement(PersonViewModel person, IDisplayLane lane, double canvasWidth)
        {
            Person = person;
            Lane = lane;
            TotalCanvasWidth = canvasWidth;
            Label = CreateLabel(person);
            Border = CreateBorder(Label, lane, person.CurrentDisplayCount);
            GetXAxis();
            GetYAxis();
        }

        private Border CreateBorder(Label label, IDisplayLane lane, int rotationCount)
        {
            var borderName = "border" + Guid.NewGuid().ToString("N").Substring(0, 10);
            var width = lane.GetType() == typeof(KioskDisplayLane) && rotationCount == 0 ? lane.LaneWidth : label.ActualWidth;
            var border = new Border()
            {
                Name = borderName,
                Uid = borderName,
                Child = label,
                Width = width,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            return border;
        }

        private Label CreateLabel(PersonViewModel person)
        {
            var minFont = 20; //TODO: Font sizes from config settings
            var maxFont = 40;
            var color = RandomColor();
            var fontSize = RandomNumber(minFont, maxFont);
            var name = "label" + Guid.NewGuid().ToString("N").Substring(0, 10);
            var label = new Label()
            {
                Content = person.ToString(),
                FontSize = fontSize,
                //FontFamily = new FontFamily(SettingsManager.Configuration.FontFamily),
                HorizontalAlignment = HorizontalAlignment.Center,
                Name = name,
                Tag = name,
                Uid = name,
                Foreground = new SolidColorBrush(color) //TODO: Randomize font color from list
            };

            label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            label.Arrange(new Rect(label.DesiredSize));

            return label;
        }

        private static int RandomNumber(int min, int max)
        {
            if (max <= min) min = max - 1;
            return Random.Next(min, max);
        }

        private Color RandomColor()
        {
            return _fontColors[Random.Next(0, _fontColors.Count)];
        }

        public void GetXAxis()
        {
            var position = RandomNumber(Convert.ToInt32(Lane.LeftMargin), Convert.ToInt32(Lane.RightMargin));
            // Adjustment to keep label from growing outside canvas area
            if (position + Label.ActualWidth > TotalCanvasWidth)
            {
                position = RandomNumber(Convert.ToInt32(Lane.LeftMargin), Convert.ToInt32(TotalCanvasWidth - Label.ActualWidth));
            }
            XAxis = position;
        }

        public void GetYAxis()
        {
            if (Lane.GetType() == typeof(KioskDisplayLane) && Person.IsFirstRun)
            {
                YAxis = 200.0; //TODO: Top Margin offset for kiosk entry.
            }
            else
            {
                YAxis = 0;
            }
        }

        public List<MyAnimation> CreateAnimations()
        {
            var list = new List<MyAnimation>();
            if (Lane.GetType() == typeof(KioskDisplayLane) && Person.IsFirstRun)
            {
                var grow = CreateGrowAnimation(0, 3); //TODO: Grow animation duration config setting
                _currentTime += grow.Duration.TimeSpan.Seconds;
                list.Add(grow);
                var shrink = CreateShrinkAnimation(_currentTime, 3);  //TODO: Shrink animation duration config setting
                list.Add(shrink);
                _currentTime += grow.Duration.TimeSpan.Seconds;
            }
            var timeModifier = 20; //TODO: Update timeModifier from config settings

            var fallDuration = timeModifier / Label.FontSize * 10;
            var fallAnimation = CreateFallAnimation(_currentTime, fallDuration);
            list.Add(fallAnimation);
            TotalTime = _currentTime + fallDuration;
            return list;
        }

        private MyAnimation CreateFallAnimation(double startTime, double duration)
        {
            var fallAnimation = new MyAnimation
            {
                Name = "FallAnimation",
                From = YAxis,
                To = 600, //TODO: This is bottom margin. Could be height of screen or less
                BeginTime = TimeSpan.FromSeconds(startTime),
                Duration = new Duration(TimeSpan.FromSeconds(duration)),
                PropertyPath = new PropertyPath(Window.TopProperty),
                TargetName = Border.Name
                //NOTE: This is how long to go from Y Axis to bottom margin
            };
            return fallAnimation;
        }

        private MyAnimation CreateShrinkAnimation(double startTime, double duration)
        {
            var maxFont = 40;
            var shrinkAnimation = new MyAnimation
            {
                Name = "ShrinkAnimation",
                From = maxFont * 2,
                To = maxFont,
                BeginTime = TimeSpan.FromSeconds(startTime), // time to begin shrinking
                Duration = new Duration(TimeSpan.FromSeconds(duration)),
                PropertyPath = new PropertyPath(Control.FontSizeProperty),
                TargetName = Label.Name
                // total animation takes to shrink
            };
            return shrinkAnimation;
        }

        private MyAnimation CreateGrowAnimation(double startTime, double duration)
        {
            var maxFont = 40;
            var growAnimation = new MyAnimation
            {
                Name = "GrowAnimation",
                From = startTime,
                To = maxFont * 2,
                BeginTime = TimeSpan.FromSeconds(startTime),
                Duration = new Duration(TimeSpan.FromSeconds(duration)),
                PropertyPath = new PropertyPath(Control.FontSizeProperty),
                TargetName = Label.Name,
            };
            return growAnimation;
        }
    }

    public class MyAnimation : DoubleAnimation
    {
        public PropertyPath PropertyPath { get; set; }
        public string TargetName { get; set; }
    }
}