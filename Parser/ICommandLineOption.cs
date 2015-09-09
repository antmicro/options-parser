using System;

namespace Antmicro.OptionsParser
{
    public interface ICommandLineOption : IArgument
    {
        bool ParseArgument(string arg);
        char ShortName { get; }
        string LongName { get; }
        string Description { get; }
        bool HasArgument { get; }
        bool IsRequired { get; }
        Type OptionType { get; }
        object Value { get; }
    }
}

