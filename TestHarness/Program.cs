using crmc.data;
using crmc.domain;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wot.Services;

namespace TestHarness
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var context = new DataContext();

            //NameTest();
            //HubTest();
            //ConfigurationTest();

            Console.WriteLine("Finished");
            Console.ReadLine();
        }

        private static void ConfigurationTest()
        {
            var connection = new HubConnection("http://localhost:11277/signalr");
            var hub = connection.CreateHubProxy("wot");
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

            WallConfiguration config = new WallConfiguration()
            {
                KioskDisplayRecycleCount = 3,
                GeneralRotationDelay = 0.15,
                PriorityRotationDelay = 5,
                MinFontSize = 10,
                MaxFontSize = 20,
                KioskEntryTopMargin = 200,
                GrowAnimationDuration = 3,
                ShrinkAnimationDuration = 3,
                FallAnimationDurationTimeModifier = 25,
                ScreenBottomMargin = 600
            };

            hub.Invoke("configurationChange", config).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("There was an error calling send: {0}",
                        task.Exception.GetBaseException());
                }
                else
                {
                    Console.WriteLine("changed config");
                }
            });
        }

        private static void HubTest()
        {
            var connection = new HubConnection("http://localhost:11277/signalr");
            var hub = connection.CreateHubProxy("wot");
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

            hub.On<string, string>("addName", (kiosk, message) =>
            {
                Console.WriteLine("From Hub: {0} : {1}", kiosk, message);
            });

            while (true)
            {
                Console.WriteLine("Enter name:");
                var line = Console.ReadLine();
                if (line == "exit")
                {
                    break;
                }

                var person = new Person() { Firstname = line.Split(new char[] { ' ' })[0], Lastname = line.Split(new char[] { ' ' })[1] };
                hub.Invoke("addName", "1", person).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine("There was an error calling send: {0}",
                            task.Exception.GetBaseException());
                    }
                    else
                    {
                        Console.WriteLine("Added name: {0}", line);
                    }
                });
            }
        }

        private static async void NameTest()
        {
            var service = new NameService("http://localhost:11277");

            IEnumerable<Person> list = await service.GetDistinct(10, 0, false);

            foreach (var person in list)
            {
                Console.WriteLine(person);
            }
        }
    }
}