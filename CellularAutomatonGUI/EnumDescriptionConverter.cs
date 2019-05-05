using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Data;

namespace CellularAutomatonGUI
{
    public class EnumDescriptionConverter : IValueConverter
    {
        private string GetEnumDescription(Enum enumObject)
        {
            FieldInfo fieldInfo = enumObject.GetType().GetField(enumObject.ToString());

            object[] attributes = fieldInfo.GetCustomAttributes(false);

            if (attributes.Length == 0)
                return enumObject.ToString();
            else
            {
                DescriptionAttribute descriptionAttribute = attributes[0] as DescriptionAttribute;
                return descriptionAttribute.Description;
            }
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Enum myEnum = (Enum)value;
            string description = GetEnumDescription(myEnum);
            return description;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Empty;
        }
    }
}