using System.Reflection;

namespace Antmicro.OptionsParser
{
    public class PositionalArgument : IArgument
    {
        public PositionalArgument(string value)
        {
            Value = value;
        }
        
        public PositionalArgument(string name, string value)
        {
            Name = name;
            Value = value;
        }
        
        public PositionalArgument(PropertyInfo underlyingProperty)
        {
            this.underlyingProperty = underlyingProperty;
            
            var nameAttribute = underlyingProperty.GetCustomAttribute<NameAttribute>();
            if(nameAttribute != null)
            {
                Name = nameAttribute.LongName;
            }
            else
            {
                Name = char.ToLower(underlyingProperty.Name[0]) + underlyingProperty.Name.Substring(1);
            }
        }

        public ElementDescriptor Descriptor { get; set; }
        
        public string Value 
        { 
            get { return value; }
            set 
            {
                IsSet = true;
                this.value = value;
            }
        }
        
        public bool IsSet { get; private set; }
        
        public bool IsRequired
        {
            get
            {
                if(underlyingProperty == null)
                {
                    return false;
                }
                
                return underlyingProperty.GetCustomAttribute<RequiredAttribute>() != null;
            }
        }
        
        public string Name { get; private set; }
        
        private readonly PropertyInfo underlyingProperty;
        private string value;
    }
}

