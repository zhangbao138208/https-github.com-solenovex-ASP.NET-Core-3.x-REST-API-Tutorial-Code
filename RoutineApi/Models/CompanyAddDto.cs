using RoutineApi.Entites;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RoutineApi.Models
{
    public class CompanyAddDto
    {
        [Required(ErrorMessage ="{0}该字段是必须的"),Display(Name ="公司名称")]
        [MaxLength(100,ErrorMessage = "{0}该字段最大长度不能超过{1}")]
        public string Name { get; set; }

        [Required(ErrorMessage = "{0}该字段是必须的"), Display(Name = "公司简介")]
        [StringLength(500,MinimumLength =2,ErrorMessage = "{0}该字段长度介于{2}和{1}")]
        public string Introduction { get; set; }
        public ICollection<Employee> Employees { get; set; }
        public string Country { get; set; }
        public string Industry { get; set; }
        public string Product { get; set; }
    }
}
