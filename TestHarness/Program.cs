using crmc.data;
using crmc.domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestHarness
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var context = new DataContext();

            var service = new NameService(context);

            IEnumerable<Person> list = service.GetNames(take: 10, skip: 0);

            foreach (var person in list)
            {
                Console.WriteLine(person);
            }
            Console.WriteLine("Finished");
            Console.ReadLine();
        }
    }

    internal class NameService
    {
        public DataContext Context { get; set; }

        public NameService(DataContext context)
        {
            Context = context;
        }

        public IEnumerable<Person> GetNames(int take, int skip)
        {
            return Context.Persons.OrderBy(x => x.SortOrder).Skip(skip).Take(take);
        }
    }
}