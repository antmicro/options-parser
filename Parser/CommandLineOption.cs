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
    }

    public class CommandLineOption : ICommandLineOption, IEquatable<CommandLineOption>
    {
        public CommandLineOption(char shortName, string longName, Type type)
        {
            ShortName = shortName;
            LongName = longName;
            OptionType = type;

            HasArgument = (OptionType != typeof(bool));
        }

        public virtual bool ParseArgument(string arg)
        {
            object val;
            var result = ParseHelper.TryParse(arg, OptionType, out val);
            if(result)
            {
                Value = val;
            }
            return result;
        }

        public bool Equals(CommandLineOption other)
        {
            return (ShortName == other.ShortName && LongName == other.LongName);
        }

        public Type OptionType { get; protected set; }

        public char ShortName { get; protected set; }

        public string LongName { get; protected set; }

        public string Description { get; protected set; }

        public object Value { get; protected set; }

        public virtual bool HasArgument { get; protected set; }

        public bool IsRequired { get; protected set; }

        public ElementDescriptor Descriptor { get; set; }

        protected CommandLineOption()
        {
        }
    }
}

