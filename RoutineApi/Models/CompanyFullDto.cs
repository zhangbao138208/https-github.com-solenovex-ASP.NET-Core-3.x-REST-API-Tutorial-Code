using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoutineApi.Models
{
    public class CompanyFullDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string Industry { get; set; }
        public string Product { get; set; }
    }
}
