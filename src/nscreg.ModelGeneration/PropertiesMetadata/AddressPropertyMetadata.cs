using nscreg.Data.Entities;

namespace nscreg.ModelGeneration.PropertiesMetadata
{
    /// <summary>
    /// Класс метаданные свойств адреса
    /// </summary>
    public class AddressPropertyMetadata :PropertyMetadataBase
    {
        public AddressPropertyMetadata(string name, bool isRequired, Address value, string groupName = null, string localizeKey = null)
            : base(name, isRequired, localizeKey, groupName)
        {
            Value = value;
        }

        public Address Value { get; set; }

        public override PropertyType Selector => PropertyType.Addresses;
    }

}