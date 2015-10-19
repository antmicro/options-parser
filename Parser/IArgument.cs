namespace Antmicro.OptionsParser
{
    public interface IArgument
    {
        bool HasArgument { get; }
        ElementDescriptor Descriptor { get; set; }
    }
}

