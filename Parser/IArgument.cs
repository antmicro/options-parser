namespace Antmicro.OptionsParser
{
    public interface IArgument
    {
        bool HasArgument { get; }
        bool AcceptsArgument { get; }
        ElementDescriptor Descriptor { get; set; }
    }
}

