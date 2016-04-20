using AsyncBridge;
using AutoMapper;
using crmc.domain;
using Microsoft.AspNet.SignalR.Client;
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
        private const int DefaultTakeCount = 20;
        private const string WebServerUrl = "http://localhost:11277";
        private int CurrentCount = 0;
        private Canvas canvas;

        private string AudioFilePath = @"C:\Audio";
        public List<DisplayLane> screenModels = new List<DisplayLane>();

        private NameService service;

        public IHubProxy hub;

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

            //Label label = new Label()
            //{
            //    Content = "Loading Names ...",
            //    FontSize = 20,
            //    Foreground = new SolidColorBrush(Colors.White),
            //    Uid = "LoadingLabel"
            //};
            //var vm = screenModels.FirstOrDefault(x => x.LaneNumber == 0);
            //label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            //label.Arrange(new Rect(label.DesiredSize));
            //var border = CreateBorder(label, vm, 0);
            //border.Uid = "border1";
            //var left = canvas.Width / 2 - label.ActualWidth;
            //var top = canvas.Height / 2;

            //Canvas.SetLeft(border, left);
            //Canvas.SetTop(border, top);
            //canvas.Children.Add(border);
            //canvas.UpdateLayout();
            //await BeginRotaion();
            AsyncHelper.FireAndForget(BeginRotaion);

            //canvas.Children.Remove(border);
        }

        private async Task BeginRotaion()
        {
            var width = canvas.ActualWidth;
            // Initialize 4 display screen models
            //Kiosk Lanes
            for (var i = 1; i < 5; i++)
            {
                //var model = new DisplayLane(5, i, width, 4) { IsKioskDisplay = true }; //TODO: Kisok delay config setting
                //screenModels.Add(model);
                screenModels.Add(new DisplayLane(5, i, width, 4) { IsKioskDisplay = true });
            }
            for (var j = 1; j < 5; j++)
            {
                var model = new DisplayLane(2, j, width, 4);
                await model.LoadNamesAsync(CurrentCount, DefaultTakeCount, false, WebServerUrl);
                CurrentCount += 25;
                screenModels.Add(model);
            }
            var priorityModel = new DisplayLane(5);
            await priorityModel.LoadNamesAsync(0, DefaultTakeCount, true, WebServerUrl);
            screenModels.Add(priorityModel);

            foreach (var vm in screenModels)
            {
                AsyncHelper.FireAndForget(() => DisplayScreenModelAsync(vm));
            }
        }

        private async Task DisplayScreenModelAsync(DisplayLane vm)
        {
            CancellationToken cancelToken = new CancellationToken();
            while (true)
            {
                var counter = 0;
                foreach (var person in vm.People.ToList())
                //for (int i = 0; i < vm.People.Count; i++)
                {
                    Console.WriteLine($"displaying {vm.LaneNumber} : {person}");
                    counter++;
                    person.RotationCount += 1;
                    await Animate(person, vm);
                    //TASK: Code smell nested if statement.
                    if (vm.IsKioskDisplay && person.RotationCount == 3) //TODO: Remove person if greater than 3. config setting??
                    {
                        vm.People.Remove(person);
                    }
                    if (!vm.IsKioskDisplay && vm.LaneNumber != 0)
                    {
                        if (counter >= vm.People.Count - 2) //TODO: Refresh queue list before end
                        {
                            if (vm.LaneNumber == 0)
                            {
                                AsyncHelper.FireAndForget(() => vm.UpdateQueueAsync(CurrentCount, DefaultTakeCount, true, WebServerUrl));
                            }
                            else
                            {
                                AsyncHelper.FireAndForget(() => vm.UpdateQueueAsync(CurrentCount, DefaultTakeCount, false, WebServerUrl));
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
                vm.Queue.Clear();
            }
        }

        private async Task<double> Animate(PersonViewModel person, DisplayLane lane)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                NameScope.SetNameScope(this, new NameScope());
                var storyboard = new Storyboard();

                Label label = CreateLabel(person);
                Border border = CreateBorder(label, lane, person.RotationCount);
                RegisterName(label.Name, label);
                RegisterName(border.Name, border);

                var xPosition = lane.RandomizeXAxis(label);
                var yPosition = 0.0;

                var currentTime = 0;
                if (lane.IsKioskDisplay && person.RotationCount == 0)
                {
                    xPosition = lane.LeftMargin;
                    yPosition = lane.GetYAxis(label); //TODO: Update X Axis from config settings

                    var maxFont = 20; //TODO: Font sizes from config settings
                    var growTime = 10; //TODO: Grow time from config settings
                    var growAnimation = new DoubleAnimation
                    {
                        From = 0,
                        To = maxFont * 2,
                        BeginTime = TimeSpan.FromSeconds(0),
                        Duration = new Duration(TimeSpan.FromSeconds(growTime)),
                    };

                    var shrinkTime = 5; //TODO: Shrink time from config settings
                    var pauseTime = 5; //TODO: Pause time from config settings. This is the time to add to the beginning of shrinking
                    var shrinkAnimation = new DoubleAnimation
                    {
                        From = maxFont * 2,
                        To = maxFont,
                        BeginTime = TimeSpan.FromSeconds(growTime + pauseTime), // time to begin shrinking
                        Duration = new Duration(TimeSpan.FromSeconds(shrinkTime)) // total animation takes to shrink
                    };
                    Storyboard.SetTargetName(growAnimation, label.Name);
                    Storyboard.SetTargetProperty(growAnimation, new PropertyPath(FontSizeProperty));
                    Storyboard.SetTargetName(shrinkAnimation, label.Name);
                    Storyboard.SetTargetProperty(shrinkAnimation, new PropertyPath(FontSizeProperty));

                    storyboard.Children.Add(growAnimation);
                    storyboard.Children.Add(shrinkAnimation);

                    currentTime += growTime + shrinkTime;
                }

                var timeModifier = 35; //TODO: Update timeModifier from config settings
                var duration = timeModifier / label.FontSize * 10;

                var fallAnimation = new DoubleAnimation
                {
                    From = yPosition,
                    To = 600, //TODO: This is bottom margin. Could be height of screen or less
                    BeginTime = TimeSpan.FromSeconds(currentTime),
                    Duration = new Duration(TimeSpan.FromSeconds(duration)) //TODO: This is how long to go from 0 to bottom margin
                };
                var e = new AnimationEventArgs { TagName = border.Uid };
                storyboard.Completed += (sender, args) => StoryboardOnCompleted(e);
                Storyboard.SetTargetName(fallAnimation, border.Name);
                Storyboard.SetTargetProperty(fallAnimation, new PropertyPath(TopProperty));
                storyboard.Children.Add(fallAnimation);

                Canvas.SetLeft(border, xPosition);
                Canvas.SetTop(border, yPosition);
                canvas.Children.Add(border);
                canvas.UpdateLayout();

                storyboard.Begin(this);
            });
            return 10; //TODO: Calculate total animation time
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

        private Border CreateBorder(Label label, DisplayLane lane, int rotationCount)
        {
            var quadSize = canvas.Width / 4;
            var borderName = "border" + Guid.NewGuid().ToString("N").Substring(0, 10);
            //var width = random ? label.ActualWidth : quadSize;
            //var width = lane.IsKioskDisplay ? quadSize : label.ActualWidth;
            var width = lane.IsKioskDisplay && rotationCount == 0 ? quadSize : label.ActualWidth;
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

        private async Task InitConnectionManager()
        {
            var connection = new HubConnection("http://localhost:11277/signalr");
            hub = connection.CreateHubProxy("wot");

            connection.Start().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("There was an error opening the connection:{0}",
                                      task.Exception.GetBaseException());
                }
                else
                {
                    Console.WriteLine("Connected");
                }
            }).Wait();

            hub.On<string, Person>("addName", (kiosk, person) => Dispatcher.Invoke(() => KioskEntry(kiosk, person)));
        }

        private async void KioskEntry(string kiosk, Person person)
        {
            Mapper.CreateMap<Person, PersonViewModel>().ReverseMap(); //TODO: Should be in mapper configuration module

            var lane = screenModels.FirstOrDefault(x => x.LaneNumber == Convert.ToInt16(kiosk) && x.IsKioskDisplay);
            if (lane == null) return;

            var pvm = Mapper.Map<Person, PersonViewModel>(person);

            await Animate(pvm, lane);

            lane.People.Add(pvm);
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