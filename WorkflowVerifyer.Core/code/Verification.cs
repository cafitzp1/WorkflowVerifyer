using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowVerifyer.Core
{
    public class Verification
    {
        public String ClientID { get; }
        public DateTime RunTime { get; }
        public List<ItemModification> ItemsModified { get; private set; }
        public Boolean RunSuccess { get; set; }
        public Int64 ElapsedTime { get; set; }

        public Verification(String a_ClientID)
        {
            ClientID = a_ClientID;
            RunTime = DateTime.Now;
            ItemsModified = new List<ItemModification>();
            RunSuccess = false;
            ElapsedTime = 0;
        }
        public override String ToString()
        {
            if(RunSuccess)
            {
                return $"Client={ClientID}, RunTime={RunTime}, Success={RunSuccess}, ItemsModified={ItemsModified.Count}"; 
            }
            else
            {
                return $"Client={ClientID}, RunTime={RunTime}, Success={RunSuccess}"; 
            }
        }
    }
}
