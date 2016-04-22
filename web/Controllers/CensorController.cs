using crmc.data;
using crmc.domain;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web.Http;

namespace web.Controllers
{
    [RoutePrefix("api/censor")]
    public class CensorController : ApiController
    {
        private readonly DataContext context;

        public CensorController()
        {
            context = new DataContext();
        }

        public IHttpActionResult Get()
        {
            var list = context.Censors.ToList();
            return Ok(list);
        }

        public IHttpActionResult Get(string search)
        {
            var list = context.Censors.Where(x => search == null || x.Word.Contains(search)).ToList();
            return Ok(list);
        }

        public IHttpActionResult Put(Censor censor)
        {
            context.Censors.AddOrUpdate(censor);
            context.SaveChanges();
            return Ok(censor);
        }

        public IHttpActionResult Post(Censor censor)
        {
            context.Censors.Add(censor);
            context.SaveChanges();
            return Ok(censor);
        }

        public IHttpActionResult Delete(int id)
        {
            var censor = context.Censors.Find(id);
            if (censor != null)
            {
                context.Censors.Remove(censor);
                context.SaveChanges();
            }
            return Ok();
        }
    }
}