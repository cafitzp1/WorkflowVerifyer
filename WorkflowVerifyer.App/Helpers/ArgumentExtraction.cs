using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowVerifyer.App.Helpers;

namespace WorkflowVerifyer.App.Helpers
{
    internal struct ArgumentExtraction
    {
        public static Dictionary<String, Object> ExtractArgValuePairs(String[] a_Args)
        {
            Dictionary<String, Object> l_ArgValuePairs = InitializeArguments();
            l_ArgValuePairs[ArgumentKey.Unrecognized] = new List<String>();

            // extract args
            for(int i = 0; i < a_Args.Length; i++)
            {
                KeyValuePair<String, String> l_ExtractedArgValuePair = ExtractArg(a_Args[i]);
                String l_ExtractedArgKeyToLower = l_ExtractedArgValuePair.Key.ToLower();

                if(l_ArgValuePairs.ContainsKey(l_ExtractedArgKeyToLower))
                {
                    // change value for the arg already in the dictionary
                    l_ArgValuePairs[l_ExtractedArgKeyToLower] = l_ExtractedArgValuePair.Value;
                }
                else // if the key wasn't there, arg was unrecognized; add entire arg to unreconized list of strings
                {
                    (l_ArgValuePairs[ArgumentKey.Unrecognized] as List<String>).Add(l_ExtractedArgValuePair.Key);
                }
            }

            return l_ArgValuePairs;
        }
        private static Dictionary<String, Object> InitializeArguments()
        {
            Dictionary<String, Object> l_ArgValuePairs = new Dictionary<String, Object>();
            List<String> UnrecognizedArgs = null;

            l_ArgValuePairs.Add(ArgumentKey.Unrecognized, UnrecognizedArgs);
            l_ArgValuePairs.Add(ArgumentKey.TimeInterval, 0);
            l_ArgValuePairs.Add(ArgumentKey.Client, 0);
            l_ArgValuePairs.Add(ArgumentKey.Delay, 0);

            return l_ArgValuePairs;
        }
        private static KeyValuePair<String, String> ExtractArg(String a_Arg)
        {
            // extract key from substring before '=' delimitter. If no '=', set entire
            // string to be the key and set object as null (the key will not be recognized, and 
            // will therefore be added to the UnrecognizedArgs list)
            
            String l_Arg = String.Empty;
            String l_Value = String.Empty;
            KeyValuePair<String, String> l_ArgValPair;
            Char l_Delim = '=';
            Int32 l_DelimIndex;
            
            if(!a_Arg.Contains(l_Delim))
            {
                // all args currently require the delimmiter; if not present, return to be added as unrecognized
                l_Arg = a_Arg;
                l_ArgValPair = new KeyValuePair<String, String>(l_Arg, l_Value);
                return l_ArgValPair;
            }

            // delimitter is present
            l_DelimIndex = a_Arg.IndexOf(l_Delim);
            l_Arg = a_Arg.Substring(0, l_DelimIndex);
            l_Value = a_Arg.Substring(l_DelimIndex+1);
            l_ArgValPair = new KeyValuePair<String, String>(l_Arg, l_Value);

            return l_ArgValPair;
        }
        public static Boolean ValidateArgs(Dictionary<String, Object> a_ArgValuePairs)
        {
            // return if there are any unrecognized args
            List<String> l_UnrecognizedArgsList = (a_ArgValuePairs[ArgumentKey.Unrecognized] as List<String>);
            if (l_UnrecognizedArgsList != null && l_UnrecognizedArgsList.Count>0)
            {
                Console.WriteLine($"The following argument(s) provided were invalid:");

                for(int i = 0; i < l_UnrecognizedArgsList.Count; i++)
                {
                    Console.WriteLine($"\t{l_UnrecognizedArgsList[i]}");
                }

                return false;
            }

            // return if any values provided for args are invalid
            foreach(KeyValuePair<String, Object> entry in a_ArgValuePairs)
            {
                if (entry.Key == ArgumentKey.Unrecognized) continue;

                // no longer a list of unrecognized args to worry about; convert item to just <string, string> 
                KeyValuePair<String, String> l_ArgValuePair = new KeyValuePair<string, string>(entry.Key, entry.Value.ToString());

                if(!ValidateArg(l_ArgValuePair))
                {
                    if(l_ArgValuePair.Value==String.Empty)
                    {
                        Console.WriteLine($"Empty value specified for '{l_ArgValuePair.Key}' argument");
                    }
                    else
                    {
                        Console.WriteLine($"Value of '{l_ArgValuePair.Value}' specified for '{l_ArgValuePair.Key}' argument was invalid");
                    }

                    return false;
                }
            }

            return true;
        }
        public static Boolean ValidateArg(KeyValuePair<String, String> a_Arg)
        {
            // route to method handler
            if (a_Arg.Key == ArgumentKey.TimeInterval)
            {
                if (!ValidateTimeIntervalArg(a_Arg.Value)) return false;
            }
            else if (a_Arg.Key == ArgumentKey.Client)
            {
                if (!ValidateClientArg(a_Arg.Value)) return false;
            }
            else if (a_Arg.Key == ArgumentKey.Delay)
            {
                if (!ValidateDelayArg(a_Arg.Value)) return false;
            }

            return true;
        }
        private static Boolean ValidateTimeIntervalArg(String a_Arg)
        {
            // ensure only digits were provided
            if (IsDigitsOnly(a_Arg))
            {
                Int32 l_ArgAsInt32 = Convert.ToInt32(a_Arg);

                // TODO: replace 1000 with 1hr (3600000) // ensure arg is ok as time in ms (between 1 hr and 1 week); default is 0, this if fine
                if ((l_ArgAsInt32 < 1000 || l_ArgAsInt32 > 604800000) && l_ArgAsInt32 != 0)
                {
                    return false;
                }

                // assume time is acceptable if not yet returned
                return true;
            }

            return false;
        }
        private static Boolean ValidateClientArg(String a_Arg)
        {
            if (IsDigitsOnly(a_Arg))
            {
                Int32 l_ArgAsInt32 = Convert.ToInt32(a_Arg);

                // FIXME: set real available clientIDs here like so (query active clients from DB) - remember to include 0
                Int32[] l_ClientIDs = { 0, 7, 36 };

                if (!l_ClientIDs.Contains(l_ArgAsInt32))
                {
                    return false;
                }

                return true;
            }

            return false;
        }
        private static Boolean ValidateDelayArg(String a_Arg)
        {
            if (IsDigitsOnly(a_Arg))
            {
                Int32 l_ArgAsInt32 = Convert.ToInt32(a_Arg);

                // arg is ok as time between 1 ms 1 week)
                if (l_ArgAsInt32 < 0 || l_ArgAsInt32 > 604800000)
                {
                    return false;
                }

                return true;
            }

            return false;
        }
        private static Boolean IsDigitsOnly(String a_Arg)
        {
            // no digits should return false
            if(a_Arg.Length==0) return false;

            foreach (char c in a_Arg)
            {
                if (c < '0' || c > '9')
                    return false;
            }
            return true;
        }
    }
}