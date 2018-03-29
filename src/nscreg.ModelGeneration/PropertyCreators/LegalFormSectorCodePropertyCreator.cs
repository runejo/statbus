using System.ComponentModel.DataAnnotations;
using System.Reflection;
using nscreg.ModelGeneration.PropertiesMetadata;
using nscreg.ModelGeneration.Validation;
using nscreg.Utilities.Attributes;
using nscreg.Utilities.Extensions;

namespace nscreg.ModelGeneration.PropertyCreators
{
    /// <summary>
    ///     Класс создатель свойства сектора кода правовой формы собственности
    /// </summary>
    public class LegalFormSectorCodePropertyCreator : PropertyCreatorBase
    {
        public LegalFormSectorCodePropertyCreator(IValidationEndpointProvider validationEndpointProvider) : base(
            validationEndpointProvider)
        {
        }

        /// <summary>
        ///     Метод проверки на создание свойства сектора кода правовой формы собственности
        /// </summary>
        public override bool CanCreate(PropertyInfo propInfo)
        {
            return propInfo.IsDefined(typeof(SearchComponentAttribute));
        }

        /// <summary>
        ///     Метод создатель свойства сектора кода правовой формы собственности
        /// </summary>
        public override PropertyMetadataBase Create(PropertyInfo propInfo, object obj, bool writable,
            bool mandatory = false)
        {
            return new LegalFormSectorCodePropertyMetadata(
                propInfo.Name,
                mandatory || !propInfo.PropertyType.IsNullable(),
                GetAtomicValue<int?>(propInfo, obj),
                GetOpder(propInfo),
                propInfo.GetCustomAttribute<DisplayAttribute>()?.GroupName,
                writable: writable);
        }
    }
}
