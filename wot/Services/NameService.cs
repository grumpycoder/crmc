using AutoMapper;
using crmc.domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using wot.ViewModels;

namespace wot.Services
{
    public class NameService
    {
        public string WebServerUrl { get; }

        public NameService(string webServerUrl)
        {
            WebServerUrl = webServerUrl;
        }

        public async Task<List<PersonViewModel>> GetDistinct(int skip, int take, bool priority)
        {
            Mapper.CreateMap<Person, PersonViewModel>().ReverseMap();

            var list = new List<Person>();
            var list2 = new List<PersonViewModel>();

            using (var client = new HttpClient())
            {
                var startTime = DateTime.Now;
                var path = $"/api/person/distinct?take={take}&skip={skip}&priority={priority}";
                var fullPath = WebServerUrl + path;

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync(fullPath);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    list = JsonConvert.DeserializeObject<List<Person>>(result);
                    list2 = Mapper.Map<List<Person>, List<PersonViewModel>>(list);
                }
                else
                {
                    //TODO: Log response error
                    //Log.Error("Error downloading from person repository");
                    //Log.Error("Error: {0}", response.StatusCode);
                }
                var totalTime = DateTime.Now.Subtract(startTime);
                Console.WriteLine($"Total download time {totalTime}");
            }

            return list2;
        }
    }
}