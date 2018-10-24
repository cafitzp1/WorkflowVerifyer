using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowVerifyer.App
{
    internal class Verification
    {
        protected readonly String m_ClientID;
        protected readonly DateTime m_RunTime;
        private List<ItemModification> m_ItemsModified;
        private Boolean m_RunSuccess;
        private Int64 m_ElapsedTime;

        public String ClientID { get => m_ClientID; }
        public DateTime RunTime { get => m_RunTime; }
        public List<ItemModification> ItemsModified { get => m_ItemsModified; set => m_ItemsModified = value; }
        public Boolean RunSuccess { get => m_RunSuccess; set => m_RunSuccess = value; }
        public Int64 ElapsedTime { get => m_ElapsedTime; set => m_ElapsedTime = value; }

        public Verification(String a_ClientID)
        {
            m_ClientID = a_ClientID;
            m_RunTime = DateTime.Now;
            m_ItemsModified = new List<ItemModification>();
            m_RunSuccess = false;
            m_ElapsedTime = 0;
        }

        public override String ToString()
        {
            if(m_RunSuccess)
            {
                return $"Client={m_ClientID}, RunTime={m_RunTime}, Success={m_RunSuccess}, ItemsModified={m_ItemsModified.Count}"; 
            }
            else
            {
                return $"Client={m_ClientID}, RunTime={m_RunTime}, Success={m_RunSuccess}"; 
            }
        }
    }
}
