using System;
using System.Reflection;

namespace Antmicro.OptionsParser
{
    public interface IFlag
    {
        bool AcceptsArgument { get; }
        char ShortName { get; }
        string LongName { get; }
        string Description { get; }
        bool IsRequired { get; }
        char Delimiter { get; }
        Type OptionType { get; }
        object DefaultValue { get; }
        int MaxElements { get; }
        PropertyInfo UnderlyingProperty { get; }
    }
}

