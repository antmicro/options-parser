using System;

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
                return value;
            }
            set 
            {
                this.value = (T)value;
                var parsed = Parsed;
                if(parsed != null)
                {
                    parsed(this, this.value);
                }
            }
        }
        
        public event Action<CommandLineOption<T>, T> Parsed;
        
        private T value;
    }

    public class CommandLineOption : ICommandLineOption, IEquatable<CommandLineOption>
    {
        public CommandLineOption(char shortName, string longName, Type type)
        {
            ShortName = shortName;
            LongName = longName;
            OptionType = type;

            HasArgument = (OptionType != typeof(bool));
            Delimiter = ';';
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

        public virtual object Value { get; set; }

        public virtual bool HasArgument { get; protected set; }

        public bool IsRequired { get; protected set; }

        public ElementDescriptor Descriptor { get; set; }
        
        public char Delimiter { get; set; }
        
        public int MaxElements { get; set; }

        protected CommandLineOption()
        {
        }
    }
}

