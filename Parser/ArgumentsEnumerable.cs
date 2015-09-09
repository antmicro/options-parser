using System.Collections.Generic;
using System.Collections;

namespace Antmicro.OptionsParser
{
    public class ArgumentsEnumerable : IEnumerable<string>
    {
        public ArgumentsEnumerable(string[] args)
        {
            this.args = args;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return new ArgumentsEnumerator(args);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private string[] args;

        private class ArgumentsEnumerator : IEnumerator<string>
        {
            public ArgumentsEnumerator(string[] args)
            {
                this.args = args;
            }

            public bool MoveNext()
            {
                return (position++ < args.Length);
            }

            public void Reset()
            {
                position = 0;
            }
            
            public void Dispose()
            {
            }

            object IEnumerator.Current { get { return Current; } }

            public string Current { get { return args[position]; } }

            private int position;
            private readonly string[] args;
        }
    }
}

