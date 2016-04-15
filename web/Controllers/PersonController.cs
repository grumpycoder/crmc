using crmc.data;
using System.Linq;
using System.Web.Http;

namespace web.Controllers
{
    [RoutePrefix("api/person")]
    public class PersonController : ApiController
    {
        private readonly DataContext context;

        public PersonController()
        {
            context = new DataContext();
        }

        [HttpGet]
        public IHttpActionResult Get()
        {
            var list = context.Persons.OrderBy(x => x.SortOrder).Skip(10).Take(10);
            return Ok(list);
        }

        [HttpGet]
        public IHttpActionResult Get(int take, int skip)
        {
            var list = context.Persons.OrderBy(x => x.SortOrder).Skip(skip).Take(take);
            return Ok(list);
        }

        [HttpGet, Route("Distinct")]
        public IHttpActionResult GetDistinct(int take, int skip)
        {
            var list = context.Persons.OrderBy(x => x.SortOrder).Skip(skip).Take(take).GroupBy(x => new { x.Lastname, x.Firstname });
            return Ok(list);
        }

        [HttpGet, Route("Count")]
        public IHttpActionResult Count()
        {
            var count = context.Persons.Count();
            return Ok(count);
        }

        [HttpGet, Route("CountDistinct")]
        public IHttpActionResult CountDistinct()
        {
            var count = context.Persons.GroupBy(x => new { x.Lastname, x.Firstname }).Count();
            return Ok(count);
        }
    }
}