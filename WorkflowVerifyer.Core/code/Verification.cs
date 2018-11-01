using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
        public static DataTable GetAll(Boolean a_ActiveOnly)
        {
            DataTable l_results = new DataTable();
            using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
            {
                using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "WorkflowVerification_GetAll"))
                {
                    l_cmd.Parameters.AddWithValue("@a_ActiveOnly", a_ActiveOnly);
                    using (SqlDataAdapter l_adapter = new SqlDataAdapter(l_cmd))
                    {
                        l_conn.Open();
                        l_adapter.Fill(l_results);
                    }
                }
            }
            return l_results;
        }
        public override String ToString()
        {
            if (RunSuccess)
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
