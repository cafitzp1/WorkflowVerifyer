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
        public List<DocumentWorkflowItem> Workflow { get; set; }
        public DateTime RunTime { get; }
        public List<ItemModification> ItemsModified { get; private set; }
        public Boolean Processed { get; set; }
        public Boolean ChangesMade { get; set; }
        public Boolean RunSuccess { get; set; }
        public Int64 ElapsedTime { get; set; }

        public WorkflowVerification(Int32 a_ClientID)
        {
            ClientID = a_ClientID;
            Workflow = new List<DocumentWorkflowItem>();
            RunTime = DateTime.Now;
            ItemsModified = new List<ItemModification>();
            Processed = false;
            ChangesMade = false;
            RunSuccess = false;
            ElapsedTime = 0;
        }
        public WorkflowVerification(Int32 a_ClientID, List<DocumentWorkflowItem> a_Workflow)
        {
            ClientID = a_ClientID;
            Workflow = a_Workflow;
            RunTime = DateTime.Now;
            ItemsModified = new List<ItemModification>();
            Processed = false;
            ChangesMade = false;
            RunSuccess = false;
            ElapsedTime = 0;
        }

        public void AppendAssignments()
        {
            List<ItemModification> l_ItemModifications = new List<ItemModification>();
            Int32 l_FirstInGroupComAnalystID = -1;
            Int32 l_FirstInGroupDocAnalystID = -1;

            // set the first IDs
            if (Workflow != null && Workflow.Count > 0)
            {
                DocumentWorkflowItem l_FirstItem = Workflow[0];

                if (l_FirstItem != null && l_FirstItem.ComplianceAnalystID > 0)
                    l_FirstInGroupComAnalystID = (Int32)l_FirstItem.ComplianceAnalystID;
                if (l_FirstItem != null && l_FirstItem.DocumentationAnalystID > 0)
                    l_FirstInGroupDocAnalystID = (Int32)l_FirstItem.DocumentationAnalystID;
            }

            // loop through each item group and append data listed for any (assignments)
            for (Int32 i = 0, j = 1; i < Workflow.Count && j < Workflow.Count; j++)
            {
                /*
                 * i sticks to the first item in a group while j prods the rest
                 * when j hits a new group, i iterates up back to be even with j
                 * i passes each item and sees if it needs anything added or changed
                 * */

                // check if items are of the same group
                if (Workflow[j].EmailDate == Workflow[i].EmailDate &&
                    Workflow[j].EmailFromAddress == Workflow[i].EmailFromAddress)
                {
                    // if we need an id
                    if (l_FirstInGroupComAnalystID == -1)
                    {
                        // if id is present
                        if (Workflow[j].ComplianceAnalystID != null && Workflow[j].ComplianceAnalystID != 0)
                        {
                            l_FirstInGroupComAnalystID = (Int32)Workflow[j].ComplianceAnalystID;
                        }
                    }
                    if (l_FirstInGroupDocAnalystID == -1)
                    {
                        if (Workflow[j].DocumentationAnalystID != null && Workflow[j].DocumentationAnalystID != 0)
                        {
                            l_FirstInGroupDocAnalystID = (Int32)Workflow[j].DocumentationAnalystID;
                        }
                    }
                }
                else
                {
                    Int32 l_NewComAnalystID = l_FirstInGroupComAnalystID;
                    Int32 l_NewDocAnalystID = l_FirstInGroupDocAnalystID;

                    // j is not in i's group; bring i up to the next item group
                    while (i != j)
                    {
                        Int32 l_ItemID = Workflow[i].DocumentWorkflowItemID;

                        // fill id if missing and if we found one from the group
                        if (l_NewComAnalystID != -1 && (Workflow[i].ComplianceAnalystID == null || Workflow[i].ComplianceAnalystID < 0))
                        {

                            String l_OldComAnalystID = (Workflow[i].ComplianceAnalystID != null && Workflow[i].ComplianceAnalystID != -1) ?
                                Workflow[i].ComplianceAnalystID.ToString() : "NULL";
                            Workflow[i].ComplianceAnalystID = l_NewComAnalystID;
                            ItemsModified.Add(new ItemModification(Workflow[i], "ComplianceAnalystID", l_OldComAnalystID, l_NewComAnalystID));
                            ChangesMade = true;

                            // change the workflow status id too if we are adding an analyst id
                            if (Workflow[i].DocumentWorkflowStatusID != 3)
                            {
                                String l_OldWorkflowStatusID = Workflow[i].DocumentWorkflowStatusID.ToString();
                                Workflow[i].DocumentWorkflowStatusID = 3;
                                ItemsModified.Add(new ItemModification(Workflow[i], "DocumentWorkflowStatusID", l_OldWorkflowStatusID, 3));
                                Workflow[i].DocumentWorkflowStatusID = 3;
                            }
                        }

                        if (l_NewDocAnalystID != -1 && (Workflow[i].DocumentationAnalystID == null || Workflow[i].DocumentationAnalystID < 0))
                        {
                            String l_OldVal = (Workflow[i].DocumentationAnalystID != null && Workflow[i].DocumentationAnalystID != -1) ?
                                Workflow[i].DocumentationAnalystID.ToString() : "NULL";
                            Workflow[i].DocumentationAnalystID = l_NewDocAnalystID;
                            ItemsModified.Add(new ItemModification(Workflow[i], "DocumentationAnalystID", l_OldVal, l_NewDocAnalystID));
                            ChangesMade = true;

                            if (Workflow[i].DocumentWorkflowStatusID != 2)
                            {
                                String l_OldWorkflowStatusID = Workflow[i].DocumentWorkflowStatusID.ToString();
                                Workflow[i].DocumentWorkflowStatusID = 2;
                                ItemsModified.Add(new ItemModification(Workflow[i], "DocumentWorkflowStatusID", l_OldWorkflowStatusID, 2));
                                Workflow[i].DocumentWorkflowStatusID = 2;
                            }
                        }

                        i++;
                    }

                    // store ids for the next group to check
                    l_FirstInGroupComAnalystID = (Workflow[j] != null && Workflow[j].ComplianceAnalystID > 0) ?
                        (Int32)Workflow[j].ComplianceAnalystID : -1;
                    l_FirstInGroupDocAnalystID = (Workflow[j] != null && Workflow[j].DocumentationAnalystID > 0) ?
                        (Int32)Workflow[j].DocumentationAnalystID : -1;
                }
            }
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
