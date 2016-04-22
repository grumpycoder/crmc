using crmc.data;
using crmc.domain;
using System;
using System.Linq;
using System.Web.Http;

namespace web.Controllers
{
    [RoutePrefix("api/configuration")]
    public class ConfigurationController : ApiController
    {
        private readonly DataContext context;

        public ConfigurationController()
        {
            context = new DataContext();
        }

        [HttpGet, Route("{mode:int}")]
        public IHttpActionResult Get(int mode)
        {
            var config = context.WallConfigurations.FirstOrDefault(x => x.ConfigurationMode == (ConfigurationMode)mode && x.Active);
            return Ok(config);
        }
    }
}