using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using wot.ViewModels;

namespace wot
{
    public class DisplayElement
    {
        private static readonly Random Random = new Random();

        public PersonViewModel Person { get; set; }
        public DisplayLane Lane { get; set; }
        public Label Label { get; set; }
        public Border Border { get; set; }

        private List<Color> fontColors = new List<Color>
        {
            Color.FromRgb(205, 238, 207),
            Color.FromRgb(247, 231, 245),
            Color.FromRgb(213, 236, 250),
            Color.FromRgb(246, 244, 207),
            Color.FromRgb(246, 227, 213)
        };

        public DisplayElement(PersonViewModel person, DisplayLane lane)
        {
            Person = person;
            Lane = lane;
            Label = CreateLabel(person);
            Border = CreateBorder(Label, lane, person.RotationCount);
        }

        private Border CreateBorder(Label label, DisplayLane lane, int rotationCount)
        {
            var borderName = "border" + Guid.NewGuid().ToString("N").Substring(0, 10);
            var width = lane.IsKioskDisplay && rotationCount == 0 ? lane.SectionWidth : label.ActualWidth;
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
            var minFont = 10; //TODO: Font sizes from config settings
            var maxFont = 20;
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
                Foreground = new SolidColorBrush(Colors.White) //TODO: Randomize font color from list
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
            return fontColors[new Random().Next(0, fontColors.Count)];
        }
    }
}