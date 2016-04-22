using crmc.domain;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using wot.Extensions;
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
        private readonly WallConfiguration _configuration;

        private readonly List<Color> _fontColors = new List<Color>
        {
            Color.FromRgb(205, 238, 207),
            Color.FromRgb(247, 231, 245),
            Color.FromRgb(213, 236, 250),
            Color.FromRgb(246, 244, 207),
            Color.FromRgb(246, 227, 213)
        };

        //TODO: Refactor out Configuration
        public DisplayElement(PersonViewModel person, IDisplayLane lane, double canvasWidth, WallConfiguration configuration)
        {
            _configuration = configuration;
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
            var width = lane.GetType() == typeof(KioskDisplayLane) && Person.IsFirstRun ? lane.LaneWidth : label.ActualWidth;
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
            var minFont = _configuration.MinFontSize;
            var maxFont = _configuration.MaxFontSize;
            var color = RandomColor();
            var fontSize = RandomNumber(minFont, maxFont);
            var name = "label" + Guid.NewGuid().ToString("N").Substring(0, 10);
            var label = new Label()
            {
                Content = person.ToString(),
                //TODO: Refactor complex expression
                FontSize = Lane.GetType() == typeof(KioskDisplayLane) && person.IsFirstRun ? maxFont : fontSize,
                //FontFamily = new FontFamily(SettingsManager.Configuration.FontFamily),
                HorizontalAlignment = HorizontalAlignment.Center,
                Name = name,
                Tag = name,
                Uid = name,
                Foreground = new SolidColorBrush(color)
            };

            label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            label.Arrange(new Rect(label.DesiredSize));
            if (Lane.GetType() != typeof(KioskDisplayLane)) return label;
            if (!(label.ActualWidth > Lane.LaneWidth)) return label;

            Console.WriteLine($"Label to large {Person}");
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
            var position = RandomNumber(Lane.LeftMargin.ToInt(), (Lane.RightMargin - Label.ActualWidth).ToInt());
            XAxis = position;
        }

        public void GetYAxis()
        {
            if (Lane.GetType() == typeof(KioskDisplayLane) && Person.IsFirstRun)
            {
                YAxis = _configuration.KioskEntryTopMargin;
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
                var grow = CreateGrowAnimation(0, _configuration.GrowAnimationDuration);
                _currentTime += grow.Duration.TimeSpan.Seconds;
                list.Add(grow);
                var shrink = CreateShrinkAnimation(_currentTime, _configuration.ShrinkAnimationDuration);
                list.Add(shrink);
                _currentTime += grow.Duration.TimeSpan.Seconds;
            }
            var timeModifier = _configuration.FallAnimationDurationTimeModifier;

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
                To = _configuration.ScreenBottomMargin, //NOTE: Screen bottom margin where names fall off
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
            var maxFont = 20;
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
            var maxFont = 20;
            //TODO: reset position based on full size label for kiosk
            Label.FontSize = 0.1;
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