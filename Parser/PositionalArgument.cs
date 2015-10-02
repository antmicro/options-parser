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
        
        public object Value 
        { 
            get { return value; }
            set 
            {
                var valueAsString = value as string;
                if(underlyingProperty != null && valueAsString != null)
                {
                    object res;
                    if(!ParseHelper.TryParse(valueAsString, underlyingProperty.PropertyType, out res))
                    {
                        return;
                    }
                    
                    this.value = res;
                }
                else
                {
                    this.value = value;
                }
                
                IsSet = true;
            }
        }
        
        public bool IsSet { get; private set; }
        
        public bool IsRequired
        {
            get
            {
                return underlyingProperty != null && underlyingProperty.GetCustomAttribute<RequiredAttribute>() != null;
            }
        }
        
        public string Name { get; private set; }
        
        private readonly PropertyInfo underlyingProperty;
        private object value;
    }
}

