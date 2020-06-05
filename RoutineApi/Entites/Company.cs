using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoutineApi.Entites
{
    public class Company
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Introduction { get; set; }
       
        public ICollection<Employee> Employees { get; set; }
        public string Country { get; set; }
        public string Product { get; set; }
        public string Industry { get; set; }

    }
}
