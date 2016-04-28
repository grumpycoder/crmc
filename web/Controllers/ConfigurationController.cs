using crmc.data;
using crmc.domain;
using System;
using System.Data.Entity.Migrations;
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

        //[HttpGet, Route("{mode:int}")]
        //public IHttpActionResult Get(int mode)
        //{
        //    var config = context.WallConfigurations.FirstOrDefault(x => x.ConfigurationMode == (ConfigurationMode)mode && x.Active);
        //    return Ok(config);
        //}

        [HttpGet]
        public IHttpActionResult Get()
        {
            var list = context.WallConfigurations.FirstOrDefault();
            return Ok(list);
        }

        public IHttpActionResult Put(WallConfiguration config)
        {
            context.WallConfigurations.AddOrUpdate(config);
            context.SaveChanges();
            return Ok(config);
        }

        [HttpGet]
        public IHttpActionResult Get()
        {
            var list = context.WallConfigurations.FirstOrDefault();
            return Ok(list);
        }

        public IHttpActionResult Put(WallConfiguration config)
        {
            context.WallConfigurations.AddOrUpdate(config);
            context.SaveChanges();
            return Ok(config);
        }
    }
}