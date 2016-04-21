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
        
            AsyncHelper.FireAndForget(BeginRotaion);

        }

        private async Task BeginRotaion()
        {
            var width = canvas.ActualWidth;
            // Initialize 4 lanes each for kiosk and general

            //Kiosk Lanes
            for (var i = 1; i < 5; i++)
            {
                //TODO: Refactor out width
                Lanes.Add(new DisplayLane(5, i, width, 4) { IsKioskLane = true }); //TODO: Kisok delay config setting
            }

            //General Lanes
            for (var j = 1; j < 2; j++)
            {
                var model = new DisplayLane(2, j, width, 4); //TODO: rotation delay config setting
                await model.LoadNamesAsync(_currentCount, DefaultTakeCount, false, WebServerUrl); //TODO: Remove dependecy on webserverurl string
                _currentCount += DefaultTakeCount;
                Lanes.Add(model);
            }

            //Priority Lane
            //var priorityLane = new DisplayLane(5, 0, width, 4) { IsPriorityLane = true }; //TODO: priority name delay config setting
            //priorityLane.SetMargins();
            //await priorityLane.LoadNamesAsync(0, DefaultTakeCount, true, WebServerUrl); //TODO: Remove dependecy on webserverurl string
            //Lanes.Add(priorityLane);

            foreach (var lane in Lanes)
            {
                AsyncHelper.FireAndForget(() => DisplayScreenModelAsync(lane), e =>
                {
                    Console.WriteLine($"Error starting loop for lane {lane.LaneNumber}");
                    Debug.WriteLine(e);
                });
            }
        }

        private async Task DisplayScreenModelAsync(DisplayLane lane)
        {
            while (true)
            {
                var currentPersonIndex = 0;
                foreach (var person in lane.People.ToList())
                {
                    Console.WriteLine($"displaying {lane.LaneNumber} : {person}");
                    currentPersonIndex++;

                    if (lane.IsKioskLane && (DateTime.Now >= person.NextDisplayTime))
                    {
                        person.CurrentDisplayCount += 1;
                        var timeToCompleteAnimation = await Animate(person, lane);
                        person.NextDisplayTime = DateTime.Now.AddSeconds(timeToCompleteAnimation);
                        Console.WriteLine($"Displaying {person} in {timeToCompleteAnimation} seconds");
                    }
                    if (!lane.IsKioskLane)
                    {
                        await Animate(person, lane);
                    }

                    //TASK: Code smell nested if statement.
                    if (lane.IsKioskLane && person.CurrentDisplayCount == 3)
                    //TODO: Remove person if greater than 3. RotationCount config setting??
                    {
                        lane.People.Remove(person);
                    }
                    if (!lane.IsKioskLane)
                    {
                        if (currentPersonIndex >= lane.People.Count - 2) //TODO: Refresh queue list before end
                        {
                            AsyncHelper.FireAndForget(
                                () => lane.UpdateQueueAsync(_currentCount, DefaultTakeCount, WebServerUrl),
                                e =>
                                {
                                    Console.WriteLine("Error updating name queue for general names");
                                    Debug.WriteLine(e);
                                });
                            _currentCount += DefaultTakeCount;
                        }
                    }
                    using (Canceller.Token.Register(Thread.CurrentThread.Abort))
                    {
                        if (!lane.IsKioskLane) await Task.Delay(TimeSpan.FromSeconds(lane.RotationDelay), Canceller.Token);
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

        private async Task<double> Animate(PersonViewModel person, DisplayLane lane)
        {
            var totalTime = 0.0;
            var width = canvas.ActualWidth;
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
                canvas.Children.Add(displayElement.Border);
                canvas.UpdateLayout();
                storyboard.Begin(this);
            });
            return totalTime; 
        }

        private async void KioskEntry(string kiosk, Person person)
        {
            Mapper.CreateMap<Person, PersonViewModel>().ReverseMap(); //TODO: Should be in mapper configuration module

            var lane = Lanes.FirstOrDefault(x => x.LaneNumber == Convert.ToInt16(kiosk) && x.IsKioskLane);
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