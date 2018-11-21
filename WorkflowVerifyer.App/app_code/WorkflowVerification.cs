using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowVerifyer.App.Helpers
{
    public class WorkflowVerification
    {
        public Int32 ClientID { get; }
        public Queue<DocumentWorkflowItem> Workflow { get; set; }
        public DateTime RunTime { get; }
        public List<ItemModification> ItemsModified { get; private set; }
        public Boolean Processed { get; set; }
        public Boolean RunSuccess { get; set; }
        public Int64 ElapsedTime { get; set; }

        public WorkflowVerification(Int32 a_ClientID)
        {
            ClientID = a_ClientID;
            Workflow = new Queue<DocumentWorkflowItem>();
            RunTime = DateTime.Now;
            ItemsModified = new List<ItemModification>();
            Processed = false;
            RunSuccess = false;
            ElapsedTime = 0;
        }
        public WorkflowVerification(Int32 a_ClientID, Queue<DocumentWorkflowItem> a_Workflow)
        {
            ClientID = a_ClientID;
            Workflow = a_Workflow;
            RunTime = DateTime.Now;
            ItemsModified = new List<ItemModification>();
            Processed = false;
            RunSuccess = false;
            ElapsedTime = 0;
        }

        public void AppendAssignments()
        {
            List<ItemModification> l_ItemModifications = new List<ItemModification>();

            // loop through each item group and append data listed for any (assignments)
            //

            // write changes to db
            // ...

            // save the list of modifications
            // ...

            // return the list of modifications
            // ...

            this.ItemsModified.Add(new ItemModification(this.ClientID, 2123123, "Analyst: Unassigned", "Analyst: Connor Fitzpatrick"));
            this.ItemsModified.Add(new ItemModification(this.ClientID, 2234234, "Analyst: Unassigned", "Analyst: Connor Fitzpatrick"));
            this.ItemsModified.Add(new ItemModification(this.ClientID, 2345345, "Analyst: Unassigned", "Analyst: Someone Else"));
            this.ItemsModified.Add(new ItemModification(this.ClientID, 2456456, "Analyst: Unassigned", "Analyst: Connor Fitzpatrick"));
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
        public static DateTime GetLastRunTime()
        {
            DateTime l_Latest = DateTime.Now;

            using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
            {
                using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "WorkflowVerification_GetLatest"))
                {
                    l_conn.Open();
                    using (SqlDataReader l_rdr = l_cmd.ExecuteReader())
                    {
                        if (l_rdr.Read())
                        {
                            l_Latest = Convert.ToDateTime(l_rdr["RunTime"]);
                        }
                    }
                }
            }

            return l_Latest;
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
