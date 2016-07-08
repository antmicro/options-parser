using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using System.Text;

namespace Antmicro.OptionsParser
{
    public class OptionsParser
    {
        public OptionsParser() : this(null)
        {
        }

        public OptionsParser(ParserConfiguration configuration)
        {
            values = new List<PositionalArgument>();
            options = new HashSet<ICommandLineOption>();
            parsedOptions = new List<ICommandLineOption>();
            unexpectedArguments = new List<IArgument>();
            this.configuration = configuration ?? new ParserConfiguration();
        }

        public CommandLineOption<T> WithOption<T>(char shortName)
        {
            var option = new CommandLineOption<T>(shortName);
            options.Add(option);
            return option;
        }

        public CommandLineOption<T> WithOption<T>(string longName)
        {
            var option = new CommandLineOption<T>(longName);
            options.Add(option);
            return option;
        }

        public CommandLineOption<T> WithOption<T>(char shortName, string longName)
        {
            var option = new CommandLineOption<T>(shortName, longName);
            options.Add(option);
            return option;
        }
        
        public void WithValue(string name)
        {
            values.Add(new PositionalArgument(name, null));
        }

        /// <summary>
        /// Parses arguments provided in command line based on configuration described in type T.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <param name="option">Configuration.</param>
        /// <returns>True if parsing was sucessful and 'help' option was not detected. False when 'help' was encountered.</returns>
        public bool Parse<T>(T option, string[] args)
        {
            helpProvider = HelpOption.CreateInstance<T>();
            helpProvider.CustomFooterGenerator = configuration.CustomFooterGenerator;
            helpProvider.CustomOptionEntryHelpGenerator = configuration.CustomOptionEntryHelpGenerator;
            helpProvider.CustomUsageLineGenerator = configuration.CustomUsageLineGenerator;
            
            foreach(var property in typeof(T).GetProperties())
            {
                var positionalAttribute = property.GetCustomAttribute<PositionalArgumentAttribute>();
                if(positionalAttribute != null)
                {
                    var argument = new PositionalArgument(property);
                    if(values.Count > positionalAttribute.Position)
                    {
                        values.Insert(positionalAttribute.Position, argument);
                    }
                    else
                    {
                        values.Add(argument);
                    }
                }
                else
                {
                    options.Add(new CommandLineOption(option, property));
                }
            }

            if(option is IValidatedOptions)
            {
                customValidationMethod = ((IValidatedOptions)option).Validate;
            }

            if(configuration.GenerateHelp)
            {
                options.Add(helpProvider);
            }

            InnerParse(args);

            foreach(var property in typeof(T).GetProperties())
            {
                var attribute = property.GetCustomAttribute<PositionalArgumentAttribute>();
                if(attribute != null && attribute.Position < values.Count)
                {
                    property.SetValue(option, values[attribute.Position].Value);
                }
            }

            return Validate();
        }

        /// <summary>
        /// Parses arguments provided in command line.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>True if parsing was sucessful and 'help' option was not detected. False when 'help' was encountered.</returns>
        public bool Parse(string[] args)
        {
            helpProvider = HelpOption.CreateInstance();
            InnerParse(args);
            return Validate();
        }

        public string RecreateUnparsedArguments()
        {
            var escapeMarkerDetected = false;
            var bldr = new StringBuilder();
            for(int i = 0; i < parsedArgs.Length; i++)
            {
                var arg = parsedArgs[i];
                if(!escapeMarkerDetected && arg == Tokenizer.EscapeMarker)
                {
                    escapeMarkerDetected = true;
                    continue;
                }
                
                var shift = 0;
                var pOpts = ParsedOptions.Cast<IArgument>().Union(Values).Where(x => x.Descriptor.Index == i).OrderBy(y => y.Descriptor.LocalPosition).ToList();
                foreach (var pOpt in pOpts)
                {
                    arg = arg.Remove(pOpt.Descriptor.LocalPosition - shift, pOpt.Descriptor.Length);
                    shift += pOpt.Descriptor.Length;
                    
                    if(pOpt.HasArgument && pOpt.AcceptsArgument)
                    {
                        // skip next argument as it was parsed by this option
                        i++;
                    }
                }

                if(arg != "-" && arg.Length > 0)
                {
                    arg = arg.Replace(@"""", @"\""");
                    if(arg.Contains(" "))
                    {
                        arg = string.Format("\"{0}\"", arg);
                    }
                    
                    bldr.Append(arg).Append(' ');
                }
            }
            
            // trim last space
            if(bldr.Length > 0 && bldr[bldr.Length - 1] == ' ')
            {
                bldr.Remove(bldr.Length - 1, 1);
            }
            return bldr.ToString();
        }

        public IEnumerable<ICommandLineOption> Options { get { return options; } }
        public IEnumerable<ICommandLineOption> ParsedOptions { get { return parsedOptions; } }
        public IEnumerable<IArgument> UnexpectedArguments { get { return unexpectedArguments; } }
        public IEnumerable<PositionalArgument> Values { get { return values; } }

        private void InnerParse(string[] args)
        {
            parsedArgs = args;

            var tokenizer = new Tokenizer(args);
            while(!tokenizer.Finished)
            {
                var token = tokenizer.ReadNextToken();
                if(token is PositionalArgumentToken)
                {
                    if(currentValuesCount < values.Count())
                    {
                        values[currentValuesCount].Descriptor = token.Descriptor;
                        values[currentValuesCount++].Value = ((PositionalArgumentToken)token).Value;
                    }
                    else
                    {
                        var arg = new PositionalArgument(((PositionalArgumentToken)token).Value) { Descriptor = token.Descriptor };
                        unexpectedArguments.Add(arg);
                    }
                }
                else if(token is LongNameToken)
                {
                    var foundOption = options.SingleOrDefault(x => x.LongName == ((LongNameToken)token).Name);
                    if(foundOption != null)
                    {
                        if(foundOption.AcceptsArgument)
                        {
                            tokenizer.MarkPosition();
                            if(foundOption.ParseArgument(tokenizer.ReadUntilTheEndOfString()))
                            {
                                tokenizer.MoveToTheNextString();
                            }
                            else
                            {
                                tokenizer.ResetPosition();
                            }
                        }

                        foundOption.Descriptor = token.Descriptor.WithLengthChangedBy(2); // -- prefix
                        if(foundOption.OptionType == typeof(bool))
                        {
                            foundOption.Value = true;
                        }
                        parsedOptions.Add(foundOption);
                    }
                    else
                    {
                        unexpectedArguments.Add(new CommandLineOption(Tokenizer.NullCharacter, ((LongNameToken)token).Name, typeof(void)));
                    }
                }
                else if(token is ShortNameToken)
                {
                    var foundOption = options.SingleOrDefault(x => x.ShortName == ((ShortNameToken)token).Name);
                    if(foundOption != null)
                    {
                        int additionalLength = 0;
                        if(foundOption.AcceptsArgument)
                        {
                            tokenizer.MarkPosition();
                            var argumentString = tokenizer.ReadUntilTheEndOfString();
                            if(argumentString == string.Empty)
                            {
                                // it means that the value is separated by a whitespace
                                tokenizer.MoveToTheNextString();
                                argumentString = tokenizer.ReadUntilTheEndOfString();
                            }
                            if(argumentString != null)
                            {
                                additionalLength = argumentString.Length;
                                if(!foundOption.ParseArgument(argumentString))
                                {
                                    tokenizer.ResetPosition();
                                }
                            }
                        }

                        foundOption.Descriptor = token.Descriptor.WithLengthChangedBy(additionalLength);
                        if(foundOption.OptionType == typeof(bool))
                        {
                            foundOption.Value = true;
                        }
                        parsedOptions.Add(foundOption);
                    }
                    else
                    {
                        unexpectedArguments.Add(new CommandLineOption(((ShortNameToken)token).Name, null, typeof(void)));
                    }
                }
            }
        }

        private bool Validate()
        {
            var forceHelp = false;
            var isHelpSelected = parsedOptions.Contains(helpProvider);
            try
            {
                var missingValue = values.FirstOrDefault(x => x.IsRequired && !x.IsSet);
                if(missingValue != null)
                {
                    throw new ValidationException(string.Format("Required value '{0}' is missing.", missingValue.Name));
                }
                
                var requiredOptions = options.Where(x => x.IsRequired);
                foreach(var requiredOption in requiredOptions)
                {
                    if(!parsedOptions.Contains(requiredOption))
                    {
                        throw new ValidationException(string.Format("Required option '{0}' is missing.", requiredOption.LongName ?? requiredOption.ShortName.ToString()));
                    }
                }

                foreach(var parsed in parsedOptions)
                {
                    if(!parsed.HasArgument && parsed.AcceptsArgument)
                    {
                        throw new ValidationException(string.Format("Option '{0}' requires parameter of type '{1}'", parsed.LongName ?? parsed.ShortName.ToString(), parsed.OptionType.Name));
                    }
                }

                if(customValidationMethod != null)
                {
                    string errorMessage;
                    if(!customValidationMethod(out errorMessage))
                    {
                        throw new ValidationException(errorMessage);
                    }
                }
                
                if(!configuration.AllowUnexpectedArguments && unexpectedArguments.Any())
                {
                    throw new ValidationException(string.Format("Unexpected options detected: {0}", RecreateUnparsedArguments()));
                }
                
            } 
            catch(ValidationException e)
            {
                if(configuration.ThrowValidationException)
                {
                    throw;
                }

                if(!isHelpSelected)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine();
                }
                forceHelp = true;
            }

            if(isHelpSelected || forceHelp)
            {
                // help option is special case - we should present help and set flag
                helpProvider.PrintHelp(this);
                return false;
            }

            return true;
        }

        private string[] parsedArgs;
        private readonly ParserConfiguration configuration;
        private readonly List<ICommandLineOption> parsedOptions;
        private readonly List<IArgument> unexpectedArguments;
        private readonly List<PositionalArgument> values;
        private readonly HashSet<ICommandLineOption> options;
        private CustomValidationMethod customValidationMethod;
        private int currentValuesCount;
        private HelpOption helpProvider;
    }

    internal delegate bool CustomValidationMethod(out string errorMessage);
}

