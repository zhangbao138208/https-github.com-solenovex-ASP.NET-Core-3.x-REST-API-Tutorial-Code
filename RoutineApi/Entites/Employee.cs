﻿using System;

namespace RoutineApi.Entites
{
    public class Employee
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string EmployeeNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        public DateTime DateBirthday { get; set; }
        public Company Company { get; set; }

    }
}