using System.Reflection;
using System.Linq;
using System;

namespace Antmicro.OptionsParser
{
    public class AutomaticCommandLineOption : CommandLineOption
    {
        public AutomaticCommandLineOption(PropertyInfo pinfo)
        {
            var nameAttribute = pinfo.GetCustomAttribute<NameAttribute>();
            if(nameAttribute != null)
            {
                ShortName = nameAttribute.ShortName;
                LongName = nameAttribute.LongName;
            }
            else
            {
                ShortName = char.ToLower(pinfo.Name.ElementAt(0));
                LongName = ShortName + pinfo.Name.Substring(1);
            }

            OptionType = pinfo.PropertyType;

            UnderlyingProperty = pinfo;

            IsRequired = (pinfo.GetCustomAttribute<RequiredAttribute>() != null);
            HasArgument = (pinfo.PropertyType != typeof(bool));

            var defaultValueAttribute = pinfo.GetCustomAttribute<DefaultValueAttribute>();
            if(defaultValueAttribute != null)
            {
                if(OptionType != defaultValueAttribute.DefaultValue.GetType())
                {
                    throw new ArgumentException(string.Format("Default value for option '{0}' is of unexpected type.", LongName ?? ShortName.ToString()));
                }
                HasDefaultValue = true;
                Value = defaultValueAttribute.DefaultValue;
            }

            var descriptionAttribute = pinfo.GetCustomAttribute<DescriptionAttribute>();
            if(descriptionAttribute != null)
            {
                Description = descriptionAttribute.Value;
            }
            
            if(OptionType.IsArray)
            {
                var numberOfElementsAttribute = pinfo.GetCustomAttribute<NumberOfElementsAttribute>();
                if(numberOfElementsAttribute != null)
                {
                    MaxElements = numberOfElementsAttribute.Max;
                }
                var delimiterAttribute = pinfo.GetCustomAttribute<DelimiterAttribute>();
                if(delimiterAttribute != null)
                {
                    Delimiter = delimiterAttribute.Delimiter;
                }
            }
        }

        public bool HasDefaultValue { get; private set; }

        public PropertyInfo UnderlyingProperty { get; private set; }
    }
}

