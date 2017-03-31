﻿using nscreg.Utilities;
using nscreg.Utilities.Extensions;

namespace nscreg.ModelGeneration
{
    public abstract class PropertyMetadataBase
    {
        protected PropertyMetadataBase(string name, bool isRequired, string localizeKey = null)
        {
            LocalizeKey = localizeKey ?? name;
            Name = name.LowerFirstLetter();
            IsRequired = isRequired;
        }

        public string Name { get; set; }

        public bool IsRequired { get; set; }

        public abstract PropertyType Selector { get; }

        public string LocalizeKey { get; set; }

        public enum PropertyType
        {
            Boolean = 0,
            DateTime,
            Float,
            Integer,
            MultiReference,
            Reference,
            String,
            Activities,
            Addresses
        }
    }
}