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
    public class ConfigurationService
    {
        public string WebServerUrl { get; }

        public ConfigurationService(string webServerUrl)
        {
            WebServerUrl = webServerUrl;
        }

        public async Task<WallConfiguration> GetConfigurationAsync(ConfigurationMode mode)
        {
            WallConfiguration config = new WallConfiguration();
            using (var client = new HttpClient())
            {
                var startTime = DateTime.Now;
                var path = $"/api/configuration?configurationmode" + Convert.ToInt32(mode); //TODO: Magic string
                var fullPath = WebServerUrl + path;

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync(fullPath);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    config = JsonConvert.DeserializeObject<WallConfiguration>(result); //TODO: Refactor
                }
                else
                {
                    //TODO: Log response error
                    //Log.Error("Error downloading from person repository");
                    //Log.Error("Error: {0}", response.StatusCode);
                }
                //Console.WriteLine($"Downloaded {list.Count}");
                //var totalTime = DateTime.Now.Subtract(startTime);
                //Console.WriteLine($"Total download time {totalTime}");
            }
            return config;
        }
    }
}