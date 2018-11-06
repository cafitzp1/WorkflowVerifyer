using System;

namespace WorkflowVerifyer.App.Helpers
{
    /// <summary>
    /// A class for returning error metadata
    /// </summary>
    public class SystemErrorLog
    {
        public Nullable<Int32> SystemErrorLogID { get; private set; }
        public String ErrorID { get; set; }
        public String ErrorDescription { get; set; }
        public String ErrorPageURL { get; private set; }
        public Nullable<DateTime> DateOccurred { get; private set; }
        public String UserNote { get; private set; }
        public Nullable<Int32> SystemUserID { get; private set; }

        /// <summary>
        /// Creates a SystemErrorLog Object
        /// </summary>
        public SystemErrorLog()
        {
            SystemErrorLogID = new Nullable<Int32>();
            ErrorID = "";
            ErrorDescription = "";
            ErrorPageURL = "";
            DateOccurred = new Nullable<DateTime>();
            UserNote = "";
            SystemUserID = new Nullable<Int32>();
        }

        /// <summary>
        /// Method to create and return a SystemErrorLog Object
        /// </summary>
        /// <param name="a_Exception"></param>
        public static SystemErrorLog ReturnErrorLog(Exception a_Exception)
        {
            SystemErrorLog l_SystemErrorLog = new SystemErrorLog();

            l_SystemErrorLog.ErrorID = Util.GenerateRandomAlphaNumericString(8);
            l_SystemErrorLog.ErrorDescription = "Exception: " + a_Exception.Message + ". Stack Trace: " + a_Exception.StackTrace;

            return l_SystemErrorLog;
        }
    }
}