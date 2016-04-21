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
        private const int DefaultTakeCount = 25;
        private const string WebServerUrl = "http://localhost:11277";
        private int _currentCount;
        private Canvas _canvas;

        private readonly string AudioFilePath = @"C:\Audio";
        public List<IDisplayLane> Lanes = new List<IDisplayLane>();
        public CancellationToken CancelToken = new CancellationToken();
        public CancellationTokenSource Canceller = new CancellationTokenSource();
        public IHubProxy Hub;

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

            AsyncHelper.FireAndForget(BeginRotaion);
        }

        private async Task BeginRotaion()
        {
            var width = _canvas.ActualWidth;
            // Initialize 4 lanes each for kiosk and general

            //Kiosk Lanes
            for (var i = 1; i < 5; i++)
            {
                //TODO: Refactor out width
                Lanes.Add(new KioskDisplayLane(rotationDelay: 5, laneIndex: i, canvasWidth: width, totalLanes: 4)); //TODO: Kisok delay config setting
            }

            var model = new GeneralDisplayLane(0.2, width); //TODO: rotation delay config setting
            await model.LoadNamesAsync(_currentCount, DefaultTakeCount, false, WebServerUrl); //TODO: Remove dependecy on webserverurl string
            _currentCount += DefaultTakeCount;
            Lanes.Add(model);

            //Priority Lane
            var priorityLane = new PriorityDisplayLane(5, width); //TODO: priority name delay config setting
            await priorityLane.LoadNamesAsync(0, DefaultTakeCount, true, WebServerUrl); //TODO: Remove dependecy on webserverurl string
            Lanes.Add(priorityLane);

            foreach (var lane in Lanes)
            {
                AsyncHelper.FireAndForget(() => DisplayScreenModelAsync(lane), e =>
                {
                    Console.WriteLine($"Error starting loop for lane {lane.LaneIndex}");
                    Debug.WriteLine(e);
                });
            }
        }

        private async Task DisplayScreenModelAsync(IDisplayLane lane)
        {
            while (true)
            {
                foreach (var person in lane.People.ToList())
                {
                    Console.WriteLine($"displaying {lane.LaneIndex} : {person}");

                    if (lane.GetType() == typeof(KioskDisplayLane) && (DateTime.Now >= person.NextDisplayTime))
                    {
                        person.CurrentDisplayCount += 1;
                        await Animate(person, lane);
                    }
                    if (lane.GetType() != typeof(KioskDisplayLane))
                    {
                        await Animate(person, lane);
                    }

                    //TASK: Code smell nested if statement.
                    if (lane.GetType() == typeof(KioskDisplayLane) && person.CurrentDisplayCount == 3)
                    //TODO: Remove person if greater than 3. RotationCount config setting??
                    {
                        lane.People.Remove(person);
                    }
                    if (lane.GetType() != typeof(KioskDisplayLane))
                    {
                        Console.WriteLine($"Current Index: {lane.People.IndexOf(person)}");
                        Console.WriteLine($"Current Percent Used: {Convert.ToDouble(lane.People.IndexOf(person)) / Convert.ToDouble(DefaultTakeCount) * 100}");
                        //if (currentPersonIndex >= lane.People.Count - 2) //TODO: Refresh queue list before end
                        if (lane.People.IndexOf(person) / DefaultTakeCount * 100 >= 90) //TODO: Refresh queue list before end
                        {
                            AsyncHelper.FireAndForget(
                                () => lane.UpdateQueueAsync(_currentCount, DefaultTakeCount, WebServerUrl),
                                e =>
                                {
                                    Console.WriteLine(@"Error updating name queue for general names");
                                    Debug.WriteLine(e);
                                });
                            _currentCount += DefaultTakeCount;
                        }
                    }
                    using (Canceller.Token.Register(Thread.CurrentThread.Abort))
                    {
                        if (lane.GetType() != typeof(KioskDisplayLane)) await Task.Delay(TimeSpan.FromSeconds(lane.RotationDelay), Canceller.Token);
                    }
                    //TODO: This is amount of time before next name displays and begins animation
                }
                if (!lane.Queue.Any())
                    continue; //TODO: If queue list is not populated continue with existing list. Notifiy issue
                if (lane.Queue.Count < DefaultTakeCount)
                    _currentCount = 0;
                //TODO: If current queue count less than DefaultTakeCount assume at end of database list and start over at beginning. Need to same position for next runtime.
                lane.People.Clear();
                lane.People.AddRange(lane.Queue);
                lane.Queue.Clear();
            }
        }

        private async Task<double> Animate(PersonViewModel person, IDisplayLane lane)
        {
            var totalTime = 0.0;
            var width = _canvas.ActualWidth;
            await Dispatcher.InvokeAsync(() =>
            {
                NameScope.SetNameScope(this, new NameScope());
                var storyboard = new Storyboard();

                var displayElement = new DisplayElement(person, lane, width);
                List<MyAnimation> animations = displayElement.CreateAnimations();
                RegisterName(displayElement.Label.Name, displayElement.Label);
                RegisterName(displayElement.Border.Name, displayElement.Border);

                var e = new AnimationEventArgs { TagName = displayElement.Border.Uid };
                storyboard.Completed += (sender, args) => StoryboardOnCompleted(e);

                foreach (var da in animations)
                {
                    Storyboard.SetTargetName(da, da.TargetName);
                    Storyboard.SetTargetProperty(da, da.PropertyPath);
                    storyboard.Children.Add(da);
                }
                totalTime = displayElement.TotalTime;
                var xPosition = displayElement.XAxis;
                var yPosition = displayElement.YAxis;
                Canvas.SetLeft(displayElement.Border, xPosition);
                Canvas.SetTop(displayElement.Border, yPosition);
                _canvas.Children.Add(displayElement.Border);
                _canvas.UpdateLayout();
                storyboard.Begin(this);
            });
            person.NextDisplayTime = DateTime.Now.AddSeconds(totalTime);
            return totalTime;
        }

        private async void KioskEntry(string kiosk, Person person)
        {
            Mapper.CreateMap<Person, PersonViewModel>().ReverseMap(); //TODO: Should be in mapper configuration module

            var lane = Lanes.FirstOrDefault(x => x.LaneIndex == Convert.ToInt16(kiosk) && x.GetType() == typeof(KioskDisplayLane));
            if (lane == null) return;

            var pvm = Mapper.Map<Person, PersonViewModel>(person);

            var waitUntil = await Animate(pvm, lane);
            pvm.NextDisplayTime = DateTime.Now.AddSeconds(waitUntil);
            //await Task.Delay(TimeSpan.FromSeconds(waitUntil), cancelToken);
            lane.People.Add(pvm);
        }

        #region Initializations

        private async Task InitConnectionManager()
        {
            var connection = new HubConnection("http://localhost:11277/signalr"); //TODO: signalr connection from config settings
            Hub = connection.CreateHubProxy("wot");

            connection.Start().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine(@"There was an error opening the connection:{0}",
                        task.Exception?.GetBaseException());
                }
                else
                {
                    Console.WriteLine(@"Connected");
                }
            }).Wait(CancelToken);

            Hub.On<string, Person>("addName", (kiosk, person) => Dispatcher.Invoke(() => KioskEntry(kiosk, person)));
        }

        private async Task InitAudioSettings()
        {
            if (!Directory.GetFiles(AudioFilePath).Any(f => f.EndsWith(".mp3"))) return;
            //TODO: Audio Init
        }

        protected async Task InitDisplay()
        {
            _canvas = WallCanvas;
            _canvas.Height = SystemParameters.PrimaryScreenHeight;
            _canvas.Width = SystemParameters.PrimaryScreenWidth;

            _canvas.UpdateLayout();

            //HACK: Test if needed.
            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 80 });
        }

        #endregion Initializations

        #region Events

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Console.WriteLine(@"Window Closed");
            //Canceller.Cancel();
        }

        private void StoryboardOnCompleted(AnimationEventArgs eventArgs)
        {
            var tagName = eventArgs.TagName;

            foreach (UIElement child in _canvas.Children.Cast<UIElement>().Where(child => tagName == child.Uid))
            {
                child.BeginAnimation(TopProperty, null);
                _canvas.Children.Remove(child);
                return;
            }
        }

        #endregion Events
    }
}