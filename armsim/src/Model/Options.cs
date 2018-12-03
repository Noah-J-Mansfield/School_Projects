using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Holds the options class for storing and parsing options
/// </summary>


namespace Prototype.Model
{
    /// <summary>
    /// class to pares command line arguments and hold the resulting options
    /// </summary>
    public class Options
    {
        Dictionary<string, string> Opts = new Dictionary<String, String>();
        string[] opts2 = { "--load", "--mem", "--test", "--exec",  "--traceall" };
        public string filename = ""; // location of an elf file
        public int memsize = 32768; // size to make the ram
        public bool flag = false; // Test flag
        public bool exec = false;
        public bool traceall = false;
        public bool bad_args = false; //variable to determine if args are all valid

        //list of exceptable chars in a filename
        public string white_list = "':.\\_-abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";


        public Options(string[] args)

        {
            for (int i = 0; i < args.Length; ++i)
            {
                switch (args[i])
                {
                    case "--load":
                        if (i < args.Length - 1 && Val_str(args[++i]))
                            filename = args[i];
                        else
                        {
                            usage_msg(args[i]);
                        }
                        break;

                    case "--mem":
                        if (i < args.Length - 1)
                        {
                            int size = Val_int(args[++i]);
                            if (size > -1)
                            {
                                memsize = size;
                            }
                            else
                            {
                                usage_msg(args[i]);
                            }
                        }
                        else
                        {
                            usage_msg(args[i]);
                        }
                        break;
                    case "--exec":
                        exec = true;
                        break;
                    case "--test":
                        flag = true;
                        break;
                    case "--traceall":
                        traceall = true;
                        break;
                    default:
                        break;
                }
            }
           //Console.WriteLine("option: filename = " + filename + ", memory size = " + memsize + ", test = " + flag);
        }
        /// <summary>
        /// displays an error message and the proper usage of the program
        /// </summary>
        /// <param name="s">the argument that failed</param>
        public void usage_msg(string s)
        {
            bad_args = true;
           //Console.WriteLine("ERROR " + s + " is an invalid argument");
           //Console.WriteLine("Usage: armsim [ --load elf-file ] [ --mem memory-size ] [ --test ]");
        }

        /// <summary>
        /// checks if the string is in an exceptable format
        /// </summary>
        /// <param name="s">filename to check</param>
        /// <returns></returns>
        public bool Val_str(string s)
        {
            foreach (char c in s)
            {
                if (white_list.Contains(c))
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// tests if string can be converted into a a int smaller than 1,000,000 
        /// </summary>
        /// <param name="i">string containg the int to validate</param>
        /// <returns>-1 uf string was not a valid int, or returns the converted string as a int</returns>
        public int Val_int(string i)
        {
            try
            {
                int size = Convert.ToInt32(i);
                if (size < 1000000)
                    return size;
                else
                    return -1;
            }
            catch (FormatException)
            {
                return -1;
            }
        }
    }

}
