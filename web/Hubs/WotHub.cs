using crmc.domain;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading.Tasks;

namespace web.Hubs
{
    [HubName("wot")]
    public class WotHub : Hub
    {
        public Task AddName(string kiosk, Person person)
        {
            return Clients.All.addName(kiosk, person);
        }

        public Task ConfigurationChange(WallConfiguration config)
        {
            return Clients.All.configurationChange(config);
        }
    }
}