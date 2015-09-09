namespace Antmicro.OptionsParser
{
    public class PositionalArgument : IArgument
    {
        public PositionalArgument(string value)
        {
            Value = value;
        }

        public ElementDescriptor Descriptor { get; set; }
        public string Value { get; private set; }
    }
}

