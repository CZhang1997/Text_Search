using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multithreaded_Text_Search
{
    class Text
    {
        private long lineNumber;
        private string text;
        public Text(long i, string t)
        {
            lineNumber = i;
            text = t;
        }
        public long getLineNumber()
        {
            return lineNumber;
        }
        public string getText()
        {
            return text;
        }
        public void setLineNumber(int i)
        {
            lineNumber = i;
        }
        public void setText(string t)
        {
            text = t;
        }
    }
}
