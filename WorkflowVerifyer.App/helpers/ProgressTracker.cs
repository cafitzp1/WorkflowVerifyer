using System;
using System.Text;

namespace WorkflowVerifyer.App.Helpers
{
    internal struct ProgressTracker
    {
        public static String ShowProgress(Int32 percentage)
        {
            String progressBar = "+--------------------+";

            for(Int32 i = 0, j = 1; i < percentage; i++)
            {
                if (i%5 == 0 && i>0)
                {
                    StringBuilder sb = new StringBuilder(progressBar);
                    sb[j] = '=';
                    progressBar = sb.ToString();
                    j++;
                }
            }

            return progressBar;
        }
        public static String EmptyProgressBar()
        {
            return "+--------------------+";
        }
    }
}