using System;

namespace WorkflowVerifyer.App.Helpers
{
    internal struct ArgumentKey
    {
        public static String Unrecognized { get => "unrecognized"; }
        public static String TimeInterval { get => "timeinterval"; }
        public static String Client { get => "client"; }
        public static String Delay { get => "delay"; }
    }
}