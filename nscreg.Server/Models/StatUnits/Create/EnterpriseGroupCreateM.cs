﻿using System;
using System.ComponentModel.DataAnnotations;

namespace nscreg.Server.Models.StatUnits.Create
{
    public class EnterpriseGroupCreateM : IStatUnitM
    {
        public int StatId { get; set; }

        [DataType(DataType.Date)]
        public DateTime StatIdDate { get; set; }

        public int TaxRegId { get; set; }

        [DataType(DataType.Date)]
        public DateTime TaxRegDate { get; set; }

        public int ExternalId { get; set; }
        public int ExternalIdType { get; set; }

        [DataType(DataType.Date)]
        public DateTime ExternalIdDate { get; set; }

        public string DataSource { get; set; }

        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }
        public AddressM Address { get; set; }
        public int PostalAddressId { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string TelephoneNo { get; set; }

        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        [DataType(DataType.Url)]
        public string WebAddress { get; set; }

        public string EntGroupType { get; set; }

        [DataType(DataType.Date)]
        public DateTime RegistrationDate { get; set; }

        public string RegistrationReason { get; set; }

        [DataType(DataType.Date)]
        public DateTime LiqDateStart { get; set; }

        [DataType(DataType.Date)]
        public DateTime LiqDateEnd { get; set; }

        public string LiqReason { get; set; }
        public string SuspensionStart { get; set; }
        public string SuspensionEnd { get; set; }
        public string ReorgTypeCode { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReorgDate { get; set; }

        public string ReorgReferences { get; set; }
        public AddressM ActualAddress { get; set; }
        public string ContactPerson { get; set; }
        public int Employees { get; set; }
        public int EmployeesFte { get; set; }

        [DataType(DataType.Date)]
        public DateTime EmployeesYear { get; set; }

        [DataType(DataType.Date)]
        public DateTime EmployeesDate { get; set; }

        public string Turnover { get; set; }

        [DataType(DataType.Date)]
        public DateTime TurnoverYear { get; set; }

        [DataType(DataType.Date)]
        public DateTime TurnoveDate { get; set; }

        public string Status { get; set; }

        [DataType(DataType.Date)]
        public DateTime StatusDate { get; set; }

        public string Notes { get; set; }
    }
}