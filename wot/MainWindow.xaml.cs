using AsyncBridge;
using AutoMapper;
using crmc.domain;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using wot.ViewModels;

namespace wot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const int DefaultTakeCount = 20;
        private const string WebServerUrl = "http://localhost:11277";
        private int _currentCount;
        private Canvas canvas;

        private readonly string AudioFilePath = @"C:\Audio";
        public List<DisplayLane> Lanes = new List<DisplayLane>();
        public CancellationToken cancelToken = new CancellationToken();
        public CancellationTokenSource Canceller = new CancellationTokenSource();
        public IHubProxy hub;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
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
            //label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            //label.Arrange(new Rect(label.DesiredSize));

            //var borderName = "border1";
            //var borderwidth = label.ActualWidth;
            //var border = new Border()
            //{
            //    Name = borderName,
            //    Uid = borderName,
            //    Child = label,
            //    Width = borderwidth,
            //    HorizontalAlignment = HorizontalAlignment.Center
            //};
            //var left = canvas.Width / 2 - label.ActualWidth;
            //var top = canvas.Height / 2;

            //Canvas.SetLeft(border, left);
            //Canvas.SetTop(border, top);
            //canvas.Children.Add(border);
            //canvas.UpdateLayout();

            AsyncHelper.FireAndForget(BeginRotaion);

            //await Task.Delay(5000, cancelToken);
            //canvas.Children.Remove(border);
        }

        private async Task BeginRotaion()
        {
            var width = canvas.ActualWidth;
            // Initialize 4 display screen models
            //Kiosk Lanes
            for (var i = 1; i < 5; i++)
            {
                //TODO: Refactor out width
                Lanes.Add(new DisplayLane(5, i, width, 4) { IsKioskDisplay = true }); //TODO: Kisok delay config setting
            }
            //General Lanes
            for (var j = 1; j < 5; j++)
            {
                var model = new DisplayLane(2, j, width, 4); //TODO: rotation delay config setting
                await model.LoadNamesAsync(_currentCount, DefaultTakeCount, false, WebServerUrl); //TODO: Remove dependecy on webserverurl string
                _currentCount += 25;
                Lanes.Add(model);
            }
            //Priority Lane
            var priorityLane = new DisplayLane(5); //TODO: priority name delay config setting
            await priorityLane.LoadNamesAsync(0, DefaultTakeCount, true, WebServerUrl); //TODO: Remove dependecy on webserverurl string
            Lanes.Add(priorityLane);

            foreach (var lane in Lanes)
            {
                AsyncHelper.FireAndForget(() => DisplayScreenModelAsync(lane), e =>
                {
                    Console.WriteLine($"Error starting loop for lane {lane.LaneNumber}");
                    Debug.WriteLine(e);
                });
            }
        }

        private async Task DisplayScreenModelAsync(DisplayLane vm)
        {
            while (true)
            {
                var counter = 0;
                foreach (var person in vm.People.ToList())
                {
                    Console.WriteLine($"displaying {vm.LaneNumber} : {person}");
                    counter++;
                    person.RotationCount += 1;
                    await Animate(person, vm);
                    //TASK: Code smell nested if statement.
                    if (vm.IsKioskDisplay && person.RotationCount == 3)
                    //TODO: Remove person if greater than 3. config setting??
                    {
                        vm.People.Remove(person);
                    }
                    if (!vm.IsKioskDisplay && vm.LaneNumber != 0)
                    {
                        if (counter >= vm.People.Count - 2) //TODO: Refresh queue list before end
                        {
                            if (vm.LaneNumber == 0)
                            {
                                AsyncHelper.FireAndForget(
                                    () => vm.UpdateQueueAsync(_currentCount, DefaultTakeCount, true, WebServerUrl),
                                    e =>
                                    {
                                        Console.WriteLine("Error updating name queue for priority names");
                                        Debug.WriteLine(e);
                                    });
                            }
                            else
                            {
                                AsyncHelper.FireAndForget(
                                    () => vm.UpdateQueueAsync(_currentCount, DefaultTakeCount, false, WebServerUrl),
                                    e =>
                                    {
                                        Console.WriteLine("Error updating name queue for general names");
                                        Debug.WriteLine(e);
                                    });
                            }
                            _currentCount += DefaultTakeCount;
                        }
                    }
                    using (Canceller.Token.Register(Thread.CurrentThread.Abort))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(vm.RotationDelay), Canceller.Token);
                    }
                    //TODO: This is amount of time before next name displays and begins animation
                }
                if (!vm.Queue.Any())
                    continue; //TODO: If queue list is not populated continue with existing list. Notifiy issue
                if (vm.Queue.Count < DefaultTakeCount)
                    _currentCount = 0;
                //TODO: If current queue count less than DefaultTakeCount assume at end of database list and start over at beginning. Need to same position for next runtime.
                vm.People.Clear();
                vm.People.AddRange(vm.Queue);
                vm.Queue.Clear();
            }
        }

        private async Task<double> Animate(PersonViewModel person, DisplayLane lane)
        {
            var totalTime = 0.0;
            await Dispatcher.InvokeAsync(() =>
            {
                NameScope.SetNameScope(this, new NameScope());
                var storyboard = new Storyboard();

                var displayElement = new DisplayElement(person, lane);
                RegisterName(displayElement.Label.Name, displayElement.Label);
                RegisterName(displayElement.Border.Name, displayElement.Border);

                var xPosition = lane.RandomizeXAxis(displayElement.Label);
                var yPosition = 0.0;
                var currentTime = 0;
                if (lane.IsKioskDisplay && person.RotationCount == 0)
                {
                    xPosition = lane.LeftMargin;
                    yPosition = lane.GetYAxis(displayElement.Label); //TODO: Update X Axis from config settings

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
                    var pauseTime = 5;
                    //TODO: Pause time from config settings. This is the time to add to the beginning of shrinking
                    var shrinkAnimation = new DoubleAnimation
                    {
                        From = maxFont * 2,
                        To = maxFont,
                        BeginTime = TimeSpan.FromSeconds(growTime + pauseTime), // time to begin shrinking
                        Duration = new Duration(TimeSpan.FromSeconds(shrinkTime)) // total animation takes to shrink
                    };
                    Storyboard.SetTargetName(growAnimation, displayElement.Label.Name);
                    Storyboard.SetTargetProperty(growAnimation, new PropertyPath(FontSizeProperty));
                    Storyboard.SetTargetName(shrinkAnimation, displayElement.Label.Name);
                    Storyboard.SetTargetProperty(shrinkAnimation, new PropertyPath(FontSizeProperty));

                    storyboard.Children.Add(growAnimation);
                    storyboard.Children.Add(shrinkAnimation);

                    currentTime += growTime + shrinkTime;
                }

                var timeModifier = 35; //TODO: Update timeModifier from config settings
                var duration = timeModifier / displayElement.Label.FontSize * 10;

                var fallAnimation = new DoubleAnimation
                {
                    From = yPosition,
                    To = 600, //TODO: This is bottom margin. Could be height of screen or less
                    BeginTime = TimeSpan.FromSeconds(currentTime),
                    Duration = new Duration(TimeSpan.FromSeconds(duration))
                    //TODO: This is how long to go from 0 to bottom margin
                };
                var e = new AnimationEventArgs { TagName = displayElement.Border.Uid };
                storyboard.Completed += (sender, args) => StoryboardOnCompleted(e);
                Storyboard.SetTargetName(fallAnimation, displayElement.Border.Name);
                Storyboard.SetTargetProperty(fallAnimation, new PropertyPath(TopProperty));
                storyboard.Children.Add(fallAnimation);

                Canvas.SetLeft(displayElement.Border, xPosition);
                Canvas.SetTop(displayElement.Border, yPosition);
                canvas.Children.Add(displayElement.Border);
                canvas.UpdateLayout();

                totalTime = currentTime + duration;
                storyboard.Begin(this);
            });
            return totalTime; //TODO: Calculate total animation time
        }

        private async void KioskEntry(string kiosk, Person person)
        {
            Mapper.CreateMap<Person, PersonViewModel>().ReverseMap(); //TODO: Should be in mapper configuration module

            var lane = Lanes.FirstOrDefault(x => x.LaneNumber == Convert.ToInt16(kiosk) && x.IsKioskDisplay);
            if (lane == null) return;

            var pvm = Mapper.Map<Person, PersonViewModel>(person);

            var waitUntil = await Animate(pvm, lane);

            await Task.Delay(TimeSpan.FromSeconds(waitUntil), cancelToken);
            lane.People.Add(pvm);
        }

        #region Initializations

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
            }).Wait(cancelToken);

            hub.On<string, Person>("addName", (kiosk, person) => Dispatcher.Invoke(() => KioskEntry(kiosk, person)));
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
            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 80 });
        }

        #endregion Initializations

        #region Events

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Window Closed");
            //Canceller.Cancel();
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

        #endregion Events
    }
}