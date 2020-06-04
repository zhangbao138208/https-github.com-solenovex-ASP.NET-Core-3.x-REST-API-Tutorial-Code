using RoutineApi.Attributes;
using RoutineApi.Entites;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RoutineApi.Models
{
   // [EmployeeNoDifferentFromName(ErrorMessage ="员工编号和名字不能相同")]
    public class EmployeeAddDto:IValidatableObject
    {
        [Required(ErrorMessage = "{0}该字段是必须的"), Display(Name = "员工编号")]
        [MaxLength(10, ErrorMessage = "{0}该字段最大长度不能超过{1}")]
        public string EmployeeNo { get; set; }
        [MaxLength(50, ErrorMessage = "{0}该字段最大长度不能超过{1}")]
        [Required(ErrorMessage = "{0}该字段是必须的"), Display(Name = "名")]
        public string FirstName { get; set; }

        [MaxLength(50, ErrorMessage = "{0}该字段最大长度不能超过{1}")]
        [Required(ErrorMessage = "{0}该字段是必须的"), Display(Name = "姓")]
        public string LastName { get; set; }
        [Display(Name = "性别")]
        public Gender Gender { get; set; }
        [Display(Name = "生日")]
        public DateTime DateBirthday { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FirstName==LastName)
            {
               yield return new ValidationResult("姓和名不能一样",new [] {nameof(LastName),nameof(FirstName) });
            }
            
        }
    }
}
