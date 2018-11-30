using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowVerifyer.App.Helpers;

namespace WorkflowVerifyer.App.Helpers
{
    internal struct ArgumentExtraction
    {
        private static Dictionary<String, Object> InitializeArguments()
        {
            Dictionary<String, Object> l_ArgValuePairs = new Dictionary<String, Object>(StringComparer.OrdinalIgnoreCase);
            List<String> UnrecognizedArgs = null;

            l_ArgValuePairs.Add("Unrecognized", UnrecognizedArgs);
            l_ArgValuePairs.Add("TimeInterval", Convert.ToInt32(ConfigurationManager.AppSettings["TimeInterval"]));
            l_ArgValuePairs.Add("Client", Convert.ToInt32(ConfigurationManager.AppSettings["Client"]));
            l_ArgValuePairs.Add("Delay", Convert.ToInt32(ConfigurationManager.AppSettings["Delay"]));
            l_ArgValuePairs.Add("LogToS3", Convert.ToInt32(ConfigurationManager.AppSettings["LogToS3"]));

            return l_ArgValuePairs;
        }
        public static Dictionary<String, Object> ExtractArgValuePairs(String[] a_Args)
        {
            Dictionary<String, Object> l_ArgValuePairs = InitializeArguments();
            l_ArgValuePairs["Unrecognized"] = new List<String>();

            // extract args
            for (int i = 0; i < a_Args.Length; i++)
            {
                KeyValuePair<String, String> l_ExtractedArgValuePair = ExtractArg(a_Args[i]);

                if (l_ArgValuePairs.ContainsKey(l_ExtractedArgValuePair.Key))
                {
                    // change value for the arg already in the dictionary
                    l_ArgValuePairs[l_ExtractedArgValuePair.Key] = l_ExtractedArgValuePair.Value;
                }
                else // if the key wasn't there, arg was unrecognized; add entire arg to unreconized list of strings
                {
                    (l_ArgValuePairs["Unrecognized"] as List<String>).Add(l_ExtractedArgValuePair.Key);
                }
            }

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

            if (!a_Arg.Contains(l_Delim))
            {
                // all args currently require the delimmiter; if not present, return to be added as unrecognized
                l_Arg = a_Arg;
                l_ArgValPair = new KeyValuePair<String, String>(l_Arg, l_Value);
                return l_ArgValPair;
            }

            // delimitter is present
            l_DelimIndex = a_Arg.IndexOf(l_Delim);
            l_Arg = a_Arg.Substring(0, l_DelimIndex);
            l_Value = a_Arg.Substring(l_DelimIndex + 1);
            l_ArgValPair = new KeyValuePair<String, String>(l_Arg, l_Value);

            return l_ArgValPair;
        }
        public static void ValidateArgs(Dictionary<String, Object> a_ArgValuePairs)
        {
            // return if there are any unrecognized args
            List<String> l_UnrecognizedArgsList = (a_ArgValuePairs["Unrecognized"] as List<String>);
            if (l_UnrecognizedArgsList != null && l_UnrecognizedArgsList.Count > 0)
            {
                Console.WriteLine($"The following argument(s) provided were invalid:");

                for (int i = 0; i < l_UnrecognizedArgsList.Count; i++)
                {
                    Console.WriteLine($"\t{l_UnrecognizedArgsList[i]}");
                }

                Environment.Exit(1);
            }

            // return if any values provided for args are invalid
            foreach (KeyValuePair<String, Object> entry in a_ArgValuePairs)
            {
                if (entry.Key == "Unrecognized") continue;

                // no longer a list of unrecognized args to worry about; convert item to just <string, string> 
                KeyValuePair<String, String> l_ArgValuePair = new KeyValuePair<string, string>(entry.Key, entry.Value.ToString());

                if (!ValidateArg(l_ArgValuePair))
                {
                    if (l_ArgValuePair.Value == String.Empty)
                    {
                        Console.WriteLine($"Empty value specified for '{l_ArgValuePair.Key}' argument");
                    }
                    else
                    {
                        Console.WriteLine($"Value of '{l_ArgValuePair.Value}' specified for '{l_ArgValuePair.Key}' argument was invalid");
                    }

                    Environment.Exit(1);
                }
            }
        }
        public static Boolean ValidateArg(KeyValuePair<String, String> a_Arg)
        {
            // route to method handler
            switch (a_Arg.Key)
            {
                case "TimeInterval":
                    return (ValidateTimeIntervalArg(a_Arg.Value)) ? true : false;
                case "Client":
                    return (ValidateClientArg(a_Arg.Value)) ? true : false;
                case "Delay":
                    return (ValidateDelayArg(a_Arg.Value)) ? true : false;
                case "LogToS3":
                    return (ValidateLogToS3Arg(a_Arg.Value)) ? true : false;
            }

            throw new Exception("Argument key not recognized in ValidateArg method");
        }
        private static Boolean ValidateTimeIntervalArg(String a_ArgValue)
        {
            // ensure only digits were provided
            if (IsDigitsOnly(a_ArgValue))
            {
                Int32 l_ArgAsInt32 = Convert.ToInt32(a_ArgValue);

                // TODO: replace 1000 with 1hr (3600000) // ensure arg is ok as time in ms (between 1 hr and 1 week); default is 0, this if fine
                return ((l_ArgAsInt32 < 1000 || l_ArgAsInt32 > 604800000) && l_ArgAsInt32 != 0) ?
                    false :
                    true;
            }

            return false;
        }
        private static Boolean ValidateClientArg(String a_ArgValue)
        {
            return (IsDigitsOrCommasOnly(a_ArgValue)) ? true : false;
        }
        public static List<Int32> ReturnClients(String a_ArgValue)
        {
            List<Int32> l_Clients = new List<Int32>();
            String l_CurrentID = String.Empty;

            for (int i = 0; i < a_ArgValue.Length; i++)
            {
                if (a_ArgValue[i] != ',')
                    l_CurrentID += a_ArgValue[i];

                else if (l_CurrentID.Length > 0)
                {
                    l_Clients.Add(Convert.ToInt32(l_CurrentID));
                    l_CurrentID = String.Empty;
                }
            }

            if (l_CurrentID.Length > 0)
                l_Clients.Add(Convert.ToInt32(l_CurrentID));

            if (l_Clients.Count == 0 || l_Clients[0] == 0)
            {
                Console.WriteLine($"No client(s) specified");
                Environment.Exit(1);
            }

            return l_Clients;
        }
        private static Boolean ValidateDelayArg(String a_ArgValue)
        {
            if (IsDigitsOnly(a_ArgValue))
            {
                Int32 l_ArgAsInt32 = Convert.ToInt32(a_ArgValue);

                // arg is ok as time between 1 ms 1 week)
                return (l_ArgAsInt32 < 0 || l_ArgAsInt32 > 604800000) ?
                    false :
                    true;
            }

            return false;
        }
        private static Boolean ValidateLogToS3Arg(String a_ArgValue)
        {
            if (IsDigitsOnly(a_ArgValue))
            {
                Int32 l_ArgAsInt32 = Convert.ToInt32(a_ArgValue);

                // arg is ok as 0 or 1)
                return (l_ArgAsInt32 == 0 || l_ArgAsInt32 == 1) ? true : false;
            }

            return false;
        }
        private static Boolean IsDigitsOnly(String a_ArgValue)
        {
            // no digits should return false
            if (a_ArgValue.Length == 0) return false;

            foreach (char c in a_ArgValue)
            {
                if (c < '0' || c > '9')
                    return false;
            }
            return true;
        }
        private static Boolean IsDigitsOrCommasOnly(String a_ArgValue)
        {
            // no digits should return false
            if (a_ArgValue.Length == 0) return false;

            foreach (char c in a_ArgValue)
            {
                if ((c < '0' || c > '9') && c != ',')
                    return false;
            }
            return true;
        }
    }
}