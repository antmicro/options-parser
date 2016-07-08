using System;
using System.Linq;
using System.Reflection;

namespace Antmicro.OptionsParser
{
    public class CommandLineOption<T> : CommandLineOption
    {
        public CommandLineOption(char shortName, string longName) : base(shortName, longName, typeof(T))
        {
        }

        public CommandLineOption(string longName) : base(Tokenizer.NullCharacter, longName, typeof(T))
        {
        }

        public CommandLineOption(char shortName) : base(shortName, null, typeof(T))
        {
        }
        
        public override object Value 
        {
            get 
            {
                return base.Value;
            }
            set 
            {
                base.Value = value;

                var parsed = Parsed;
                if(parsed != null)
                {
                    parsed(this, (T)base.Value);
                }
            }
        }
        
        public event Action<CommandLineOption<T>, T> Parsed;
    }

    public class CommandLineOption : ICommandLineOption, IEquatable<CommandLineOption>
    {
        public CommandLineOption(char shortName, string longName, Type type) : this()
        {
            ShortName = shortName;
            LongName = longName;
            OptionType = type;

            AcceptsArgument = (OptionType != typeof(bool));
        }

        public CommandLineOption(object source, PropertyInfo pinfo) : this()
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

            underlyingProperty = pinfo;
            this.source = source;

            IsRequired = (pinfo.GetCustomAttribute<RequiredAttribute>() != null);
            AcceptsArgument = (pinfo.PropertyType != typeof(bool));

            var defaultValueAttribute = pinfo.GetCustomAttribute<DefaultValueAttribute>();
            if(defaultValueAttribute != null)
            {
                if(OptionType != defaultValueAttribute.DefaultValue.GetType())
                {
                    throw new ArgumentException(string.Format("Default value for option '{0}' is of unexpected type.", LongName ?? ShortName.ToString()));
                }
                HasDefaultValue = true;
                SetValue(defaultValueAttribute.DefaultValue);
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

        public virtual bool ParseArgument(string arg)
        {
            object parsedValue;
            if(OptionType.IsArray)
            {
                var values = (MaxElements > 0) ? arg.Split(new[] { Delimiter }, MaxElements) : arg.Split(Delimiter); 
                var array = Array.CreateInstance(OptionType.GetElementType(), values.Length);
                
                for(int i = 0; i < values.Length; i++)
                {
                    if(!ParseHelper.TryParse(values[i], OptionType.GetElementType(), out parsedValue))
                    {
                        return false;
                    }
                    array.SetValue(parsedValue, i);
                }
                Value = array;
            }
            else
            {
                if(!ParseHelper.TryParse(arg, OptionType, out parsedValue))
                {
                    return false;
                }
                Value = parsedValue;
            }
            
            HasArgument = true;
            return true;
        }

        public bool Equals(CommandLineOption other)
        {
            return (ShortName == other.ShortName && LongName == other.LongName);
        }

        public Type OptionType { get; protected set; }

        public char ShortName { get; protected set; }

        public string LongName { get; protected set; }

        public string Description { get; set; }

        public bool AcceptsArgument { get; protected set; }
        
        public bool HasArgument { get; protected set; }

        public bool IsRequired { get; protected set; }

        public ElementDescriptor Descriptor { get; set; }
        
        public char Delimiter { get; set; }
        
        public int MaxElements { get; set; }

        public bool HasDefaultValue { get; private set; }

        public virtual object Value
        {
            get
            {
                return value;
            }

            set
            {
                SetValue(value);
            }
        }

        private CommandLineOption()
        {
            Delimiter = ';';
        }

        private void SetValue(object v)
        {
            value = v;
            if(underlyingProperty != null && source != null)
            {
                underlyingProperty.SetValue(source, value);
            }
        }

        private readonly PropertyInfo underlyingProperty;
        private readonly object source;
        private object value;
    }
}

