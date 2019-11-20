using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLUpdateQueryGenerator
{
    public class Person : IWithKey
    {
        public Guid Id { get; set; }
        public int Age { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Person person = new Person();
            person.Id = Guid.NewGuid();
            person.Name = "Alesha";
            person.Age = 40;
            person.Surname = "Popovich";


            UnitOfWork<Person> unitOfWork = new UnitOfWork<Person>();
            unitOfWork.Add(person);

            Person updatedPerson = new Person();
            updatedPerson.Name = "Zmey";
            updatedPerson.Surname = "Tugarin";
            updatedPerson.Age = 4000;
            updatedPerson.Id = person.Id;

            string str = unitOfWork.Update(updatedPerson);
            Console.WriteLine(str);
            Console.ReadLine();
        }
    }
}
