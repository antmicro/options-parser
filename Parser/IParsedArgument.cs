namespace Antmicro.OptionsParser
{
    public interface IParsedArgument
    {
        IFlag Flag { get; }
        bool HasArgument { get; }
        ElementDescriptor Descriptor { get; set; }
        object Value { get; }
    }
}

