using crmc.domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using wot.Services;
using wot.ViewModels;

namespace wot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int DefaultTakeCount = 25;
        private const string WebServerUrl = "http://localhost:11277";
        private int CurrentCount = 0;
        private Canvas canvas;

        private string AudioFilePath = @"C:\Audio";
        private List<DisplayScreenModel> screenModels = new List<DisplayScreenModel>();

        private NameService service;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            service = new NameService(WebServerUrl);

            await InitDisplay();
            await InitAudioSettings();
            await InitConnectionManager();

            Label label = new Label()
            {
                Content = "Loading Names ...",
                FontSize = 20,
                Foreground = new SolidColorBrush(Colors.White),
                Uid = "LoadingLabel"
            };
            label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            label.Arrange(new Rect(label.DesiredSize));
            var border = CreateBorder(label, false);
            var left = canvas.Width / 2 - label.ActualWidth;
            var top = canvas.Height / 2;

            Canvas.SetLeft(border, left);
            Canvas.SetTop(border, top);
            canvas.Children.Add(border);
            canvas.UpdateLayout();
            await BeginRotaion();
            canvas.Children.Remove(border);
        }

        private async Task BeginRotaion()
        {
            // Initialize 4 display screen models
            for (var i = 1; i < 5; i++)
            {
                var model = new DisplayScreenModel(1, i);
                await model.LoadNamesAsync(CurrentCount, DefaultTakeCount, false, WebServerUrl);
                CurrentCount += 25;
                screenModels.Add(model);
            }
            for (var i = 1; i < 5; i++)
            {
                var model = new DisplayScreenModel(1, i) { IsKioskDisplay = true }; //TODO: Kisok delay config setting
                screenModels.Add(model);
            }
            var priorityModel = new DisplayScreenModel(5);
            await priorityModel.LoadNamesAsync(0, DefaultTakeCount, true, WebServerUrl);
            screenModels.Add(priorityModel);

            var cancelToken = new CancellationToken();
            foreach (var vm in screenModels)
            {
                await Task.Factory.StartNew(() => DisplayScreenModelAsync(vm), cancelToken);
            }
        }

        private async Task DisplayScreenModelAsync(DisplayScreenModel vm)
        {
            CancellationToken cancelToken = new CancellationToken();
            while (true)
            {
                var counter = 0;
                foreach (var person in vm.People)
                {
                    Console.WriteLine($"displaying {vm.Section} : {person}");
                    counter++;
                    person.RotationCount += 1;
                    await Animate(person, vm);
                    //TASK: Code smell nested if statement.
                    if (vm.IsKioskDisplay && person.RotationCount == 3)
                    {
                        vm.People.Remove(person); //TODO: Remove person if greater than 3. config setting??
                    }
                    else
                    {
                        if (counter >= vm.People.Count - 5) //TODO: Refresh queue list before end
                        {
                            if (vm.Section == 0)
                            {
                                await vm.UpdateQueueAsync(CurrentCount, DefaultTakeCount, true, WebServerUrl);
                            }
                            else
                            {
                                await vm.UpdateQueueAsync(CurrentCount, DefaultTakeCount, false, WebServerUrl);
                            }
                            CurrentCount += DefaultTakeCount;
                        }
                    }
                    await Task.Delay(TimeSpan.FromSeconds(vm.RotationDelay), cancelToken); //TODO: This is amount of time before next name displays and begins animation
                }
                if (!vm.Queue.Any()) continue; //TODO: If queue list is not populated continue with existing list. Notifiy issue
                if (vm.Queue.Count < DefaultTakeCount) CurrentCount = 0; //TODO: If current queue count less than DefaultTakeCount assume at end of database list and start over at beginning. Need to same position for next runtime.
                vm.People.Clear();
                vm.People.AddRange(vm.Queue);
            }
        }

        private async Task Animate(Person person, DisplayScreenModel vm)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                NameScope.SetNameScope(this, new NameScope());
                var storyboard = new Storyboard();

                Label label = CreateLabel(person);
                Border border = CreateBorder(label, true);
                RegisterName(label.Name, label);
                RegisterName(border.Name, border);

                var leftPosition = GetLeftPosition(label, vm);
                var fallAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 600, //TODO: This is bottom margin. Could be height of screen or less
                    BeginTime = TimeSpan.FromSeconds(0),
                    Duration = new Duration(TimeSpan.FromSeconds(20)) //TODO: This is how long to go from 0 to bottom margin
                };
                var e = new AnimationEventArgs { TagName = border.Uid };
                storyboard.Completed += (sender, args) => StoryboardOnCompleted(e);
                Storyboard.SetTargetName(fallAnimation, border.Name);
                Storyboard.SetTargetProperty(fallAnimation, new PropertyPath(TopProperty));
                storyboard.Children.Add(fallAnimation);

                Canvas.SetLeft(border, leftPosition);
                Canvas.SetTop(border, 0);
                canvas.Children.Add(border);
                canvas.UpdateLayout();

                storyboard.Begin(this);
            });
        }

        private int GetLeftPosition(Label label, DisplayScreenModel vm)
        {
            var sectionWidth = canvas.Width / 4;
            var leftMargin = sectionWidth * (vm.Section - 1);
            var rightMargin = leftMargin + sectionWidth;
            var position = RandomNumber(Convert.ToInt32(leftMargin), Convert.ToInt32(rightMargin));
            if (vm.Section == 0)
            {
                position = RandomNumber(Convert.ToInt32(0), Convert.ToInt32(canvas.Width));
            }
            if (position + label.ActualWidth > canvas.Width)
            {
                position = RandomNumber(Convert.ToInt32(leftMargin), Convert.ToInt32((canvas.Width - label.ActualWidth)));
            }
            return position;
        }

        private int RandomNumber(int min, int max)
        {
            if (max <= min) min = max - 1;
            return Random.Next(min, max);
        }

        private static readonly Random Random = new Random();

        public class AnimationEventArgs : EventArgs
        {
            public string TagName { get; set; }
        }

        private void StoryboardOnCompleted(AnimationEventArgs eventArgs)
        {
            var tagName = eventArgs.TagName;

            foreach (UIElement child in canvas.Children.Cast<UIElement>().Where(child => tagName == child.Uid))
            {
                child.BeginAnimation(TopProperty, null);
                canvas.Children.Remove(child);
                return;
            }
        }

        private Border CreateBorder(Label label, bool random)
        {
            //var quadSize = 200;
            var quadSize = canvas.Width / 4;
            var borderName = "border" + Guid.NewGuid().ToString("N").Substring(0, 10);
            var width = random ? label.ActualWidth : quadSize;
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

        private Label CreateLabel(Person person)
        {
            var labelFontSize = 20;
            var name = "label" + Guid.NewGuid().ToString("N").Substring(0, 10);
            var label = new Label()
            {
                Content = person.ToString(),
                FontSize = labelFontSize,
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

        private async Task InitConnectionManager()
        {
            //TODO: InitConnectionManager
        }

        private async Task InitAudioSettings()
        {
            if (!Directory.GetFiles(AudioFilePath).Any(f => f.EndsWith(".mp3"))) return;
            //TODO: Audio Init
        }

        protected async Task InitDisplay()
        {
            canvas = WallCanvas;
            canvas.Height = SystemParameters.PrimaryScreenHeight;
            canvas.Width = SystemParameters.PrimaryScreenWidth;

            canvas.UpdateLayout();

            //HACK: Test if needed.
            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 80 });
        }
    }
}