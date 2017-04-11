﻿using System;
using System.Collections.Generic;
using nscreg.Data.Constants;
using nscreg.Data.Entities;
using FluentValidation;
using nscreg.Resources.Languages;

namespace nscreg.Server.Models.DataSources
{
    public class CreateM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int AllowedOperations { get; set; }
        public IEnumerable<string> AttributesToCheck { get; set; }
        public string Priority { get; set; }
        public string Restrictions { get; set; }
        public string VariablesMapping { get; set; }

        public DataSource GetEntity()
        {
            SourcePriority priority;
            if (!Enum.TryParse(Priority, out priority))
            {
                priority = SourcePriority.NotTrusted;
            }
            return new DataSource
            {
                Name = Name,
                Description = Description,
                Priority = priority,
                AllowedOperations = (DataSourceAllowedOperation)AllowedOperations,
                Restrictions = Restrictions,
                VariablesMapping = VariablesMapping,
                AttributesToCheckArray = AttributesToCheck,
            };
        }
    }

    // ReSharper disable once UnusedMember.Global
    internal class DataSourceCreateMValidator : AbstractValidator<CreateM>
    {
        public DataSourceCreateMValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(nameof(Resource.DataSourceNameIsRequired));

            RuleFor(x => x.AttributesToCheck)
                .NotEmpty()
                .WithMessage(nameof(Resource.DataSourceAttributesToCheckIsRequired));
        }
    }
}
