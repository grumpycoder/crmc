using crmc.data;
using crmc.domain;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web.Http;
using web.Helpers;
using web.ViewModels;

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
            var list = context.Persons.OrderBy(x => x.SortOrder).Skip(10).Take(10).ToList();
            return Ok(list);
        }

        [HttpGet]
        public IHttpActionResult Get(int take, int skip, bool priority)
        {
            var list = context.Persons.Where(x => x.IsPriority == priority).OrderByDescending(x => x.DateCreated).Skip(skip).Take(take).ToList();
            return Ok(list);
        }

        [HttpPost, Route("Search")]
        public IHttpActionResult Search(PeopleSearchViewModel vm)
        {
            var page = vm.Page.GetValueOrDefault(0);
            var pageSize = vm.PageSize.GetValueOrDefault(10);
            var skipRows = (page - 1) * pageSize;

            var pred = PredicateBuilder.True<Person>();
            if (vm.IsDonor != null) pred = pred.And(p => p.IsDonor == vm.IsDonor);
            if (vm.IsPriority != null) pred = pred.And(p => p.IsPriority == vm.IsPriority);
            if (vm.FuzzyMatchValue != null) pred = pred.And(p => p.FuzzyMatchValue >= vm.FuzzyMatchValue);
            if (!string.IsNullOrWhiteSpace(vm.AccountId)) pred = pred.And(p => p.AccountId.Contains(vm.AccountId));
            if (!string.IsNullOrWhiteSpace(vm.Firstname)) pred = pred.And(p => p.Firstname.Contains(vm.Firstname));
            if (!string.IsNullOrWhiteSpace(vm.Lastname)) pred = pred.And(p => p.Lastname.Contains(vm.Lastname));
            if (!string.IsNullOrWhiteSpace(vm.EmailAddress)) pred = pred.And(p => p.EmailAddress.Contains(vm.EmailAddress));
            if (!string.IsNullOrWhiteSpace(vm.Zipcode)) pred = pred.And(p => p.Zipcode.StartsWith(vm.Zipcode));
            if (vm.DateCreated != null) pred = pred.And(p => p.DateCreated >= vm.DateCreated);

            List<Person> list;
            if (vm.AllRecords)
            {
                list = context.Persons.AsQueryable()
                    .Where(pred)
                    .OrderBy(x => x.Id)
                    .ToList();
            }
            else
            {
                list = context.Persons
                    .OrderBy(x => x.DateCreated)
                    .Where(pred)
                    .Skip(skipRows)
                    .Take(pageSize).ToList();
            }
            var totalCount = context.Persons.Count();
            var filterCount = context.Persons.Where(pred).Count();
            var totalPages = (int)Math.Ceiling((decimal)filterCount / pageSize);

            vm.TotalCount = totalCount;
            vm.FilteredCount = filterCount;
            vm.TotalPages = totalPages;

            vm.Items = list;
            return Ok(vm);
        }

        [HttpGet, Route("Distinct")]
        public IHttpActionResult GetDistinct(int take, int skip, bool priority)
        {
            //var list = context.Persons.OrderBy(x => x.SortOrder).Skip(skip).Take(take).GroupBy(x => new { x.Lastname, x.Firstname }).Select(grp => grp.FirstOrDefault()).ToList();
            var list = context.Persons.Where(x => x.IsPriority == priority).OrderBy(x => x.SortOrder).Skip(skip).Take(take);
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

        public IHttpActionResult Post(Person person)
        {
            context.Persons.Add(person);
            context.SaveChanges();
            return Ok(person);
        }

        public IHttpActionResult Put(Person person)
        {
            person.DateCreated = DateTime.Now;
            context.Persons.AddOrUpdate(person);
            context.SaveChanges();
            return Ok(person);
        }

        public IHttpActionResult Delete(int id)
        {
            var person = context.Persons.Find(id);
            if (person != null)
            {
                context.Persons.Remove(person);
                context.SaveChanges();
            }
            return Ok("Deleted successfully");
        }

        [HttpGet, Route("Stat")]
        public IHttpActionResult Stat()
        {
            var totalCount = context.Persons.Count();
            var todayCount = context.Persons.Count(x => x.DateCreated >= DbFunctions.TruncateTime(DateTime.Now));
            var monthCount = context.Persons.Count(x => x.DateCreated.Month == DateTime.Now.Month);

            var visitorStat = new VisitorStatViewModel()
            {
                TotalCount = totalCount,
                TodayCount = todayCount,
                MonthCount = monthCount
            };
            return Ok(visitorStat);
        }
    }
}