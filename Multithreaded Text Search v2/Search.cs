using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

/**
 * @Author: Churong Zhang
 * @Email: churong.zhang@utdallas.edu
 * @Date:  4/10/2020
 * This class is for processing file
 */

namespace Multithreaded_Text_Search
{
    
    class Search
    {
        public Search()
        {
        }
        
        public long getLineCount(string filename)
        {   // get the number of line in a file
            long count = 0l;
            StreamReader reader = new StreamReader(filename);
            while (reader.ReadLine() != null)
            {
                count++;
            }
            reader.Close();
            return count;
            
        }


        // scan file starting at line "start" and end at "end"
        // return the line number and line text as a dictionart pair
        public Dictionary<long, String> search(string filename, string keyword, long start, long end)
        {
            // start at line 1
            long lineNumber = 1;
            // create the dictionary
            Dictionary<long, string> pair = new Dictionary<long, string>();
            StreamReader reader = new StreamReader(filename);
            keyword = keyword.ToLower();
            // skip the lines until the program reach the line at start
            while (lineNumber < start && reader.ReadLine() != null)
            {
                lineNumber++;
            }
            string line;    // store the line that was read in
            while((line = reader.ReadLine()) != null && lineNumber < end)
            {
                // change the line to lower case and check if it contains the keyword
                string line2 = line.ToLower();
                if(line2.Contains(keyword))
                {   // if do add this pair to the dictionary
                    pair.Add(lineNumber, line);
                    //System.Threading.Thread.Sleep(1);
                }
                // System wait to make it more fun
                System.Threading.Thread.Sleep(1);
                lineNumber++;
            }
            // close the reader and return the dictionary
            reader.Close();
            return pair;
        }

        

    }
}
