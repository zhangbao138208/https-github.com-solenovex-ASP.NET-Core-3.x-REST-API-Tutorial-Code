using RoutineApi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RoutineApi.Attributes
{
    public class EmployeeNoDifferentFromNameAttribute:ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var employeeAdd = (EmployeeAddDto)validationContext.ObjectInstance;
            if (employeeAdd.EmployeeNo==employeeAdd.FirstName)
            {
                return new ValidationResult(ErrorMessage,new[] { nameof(EmployeeAddDto)});
            }
            return  ValidationResult.Success;
        }
    }
}
