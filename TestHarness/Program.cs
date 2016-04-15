using crmc.data;
using crmc.domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var context = new DataContext();

            var p = new Person()
            {
                Firstname = "Mark",
                Lastname = "Lawrence"
            };
            context.Persons.Add(p);
            var i = context.SaveChanges();

            Console.WriteLine(i);

            Console.WriteLine("Finished");
            Console.ReadLine();
        }
    }
}