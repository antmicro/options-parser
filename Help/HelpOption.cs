using System;
using System.Text;

namespace Antmicro.OptionsParser
{
    public class HelpOption : CommandLineOption<bool>
    {
        public static HelpOption CreateInstance<T>()
        {
            var appInfo = new ApplicationInfo();
            appInfo.GetInfo(typeof(T));
            return new HelpOption(appInfo);
        }

        public void PrintHelp(OptionsParser parser)
        {
            if(!string.IsNullOrWhiteSpace(appInfo.ApplicationName))
            {
                Console.Write(appInfo.ApplicationName);
                Console.WriteLine(string.IsNullOrWhiteSpace(appInfo.ApplicationVersion) ? string.Empty : " " + appInfo.ApplicationVersion);

                if(!string.IsNullOrWhiteSpace(appInfo.ApplicationCopyrights))
                {
                    Console.WriteLine(appInfo.ApplicationCopyrights);
                }
                Console.WriteLine();
            }

            var valuesBuilder = new StringBuilder();
            foreach(var value in parser.Values)
            {
                if(value.IsRequired)
                {
                    valuesBuilder.Append(' ').Append(value.Name);
                }
                else
                {
                    valuesBuilder.AppendFormat(" [{0}]", value.Name);
                }
            }
            
            var usageLine = string.Format(UsageLineFormat, appInfo.ApplicationBinaryName, valuesBuilder);
            Console.WriteLine(CustomUsageLineGenerator != null
                ? CustomUsageLineGenerator(usageLine)
                : usageLine);

            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine();

            foreach(var option in parser.Options)
            {
                Console.WriteLine(GenerateOptionHelpEntry(option));
                Console.WriteLine();
            }

            if(CustomFooterGenerator != null)
            {
                Console.WriteLine(CustomFooterGenerator());
            }
        }
        
        public Func<ICommandLineOption, string> CustomOptionEntryHelpGenerator { get; set; }

        public Func<string, string> CustomUsageLineGenerator { get; set; }

        public Func<string> CustomFooterGenerator { get; set; }

        public const string UsageLineFormat = "usage: {0} [options]{1}";

        private static string GenerateOptionHelpEntry(ICommandLineOption option)
        {
            var optionBuilder = new StringBuilder("  ");
            if(option.ShortName != Tokenizer.NullCharacter)
            {
                optionBuilder.AppendFormat("-{0}", option.ShortName);
            }
            if(option.LongName != null)
            {
                if(option.ShortName != Tokenizer.NullCharacter)
                {
                    optionBuilder.Append(", ");
                }

                optionBuilder.AppendFormat("--{0}", option.LongName);
            }
            optionBuilder.Append(' ', Math.Max(0, 30 - optionBuilder.Length));
            optionBuilder.Append(option.OptionType.Name.ToUpper());

            if(option.IsRequired)
            {
                optionBuilder.Append(" (required)");
            }

            optionBuilder.Append(' ', 3);

            optionBuilder.Append(option.Description);

            return optionBuilder.ToString();
        }

        private HelpOption(ApplicationInfo info) : base('h', "help")
        {
            Description = "Display this help page.";
            appInfo = info;
        }

        private readonly ApplicationInfo appInfo;
    }
}

