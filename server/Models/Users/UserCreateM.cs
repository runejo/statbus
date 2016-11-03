﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Server.Data.Defaults;
using Server.Helpers;

namespace Server.Models.Users
{
    public class UserCreateM
    {
        [Required]
        public string Login { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }

        [Required, PrintableString]
        public string Name { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        public UserStatuses Status { get; set; }

        [Required]
        public IEnumerable<string> AssignedRoles { get; set; }

        public string Description { get; set; }
    }
}
