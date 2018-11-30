using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowVerifyer.App.Helpers;

namespace WorkflowVerifyer.App.Helpers
{
    public class ItemModification
    {
        public DocumentWorkflowItem Item { get; set; }
        public String Column { get; set; }
        public Object OldData { get; set; }
        public Object NewData { get; set; }

        public ItemModification(DocumentWorkflowItem a_DocumentWorkflowItem)
        {
            Item = a_DocumentWorkflowItem;
            Column = String.Empty;
            OldData = String.Empty;
            NewData = String.Empty;
        }

        public ItemModification(DocumentWorkflowItem a_DocumentWorkflowItem, String a_Column, Object a_OldData, Object a_NewData)
        {
            Item = a_DocumentWorkflowItem;
            Column = a_Column;
            OldData = a_OldData;
            NewData = a_NewData;
        }

        public Boolean WriteToDB()
        {
            try
            {
                using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
                {
                    using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "DocumentWorkflowItem_Modify"))
                    {
                        l_cmd.Parameters.AddWithValue("@a_DocumentWorkflowItemID", Item.DocumentWorkflowItemID);
                        l_cmd.Parameters.AddWithValue("@a_DocumentationAnalystID", Item.DocumentationAnalystID);
                        l_cmd.Parameters.AddWithValue("@a_ComplianceAnalystID", Item.ComplianceAnalystID);
                        l_cmd.Parameters.AddWithValue("@a_Notes", Item.Notes);
                        l_cmd.Parameters.AddWithValue("@a_DocumentWorkflowStatusID", Item.DocumentWorkflowStatusID);

                        l_conn.Open();
                        l_cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public override String ToString()
        {
            // TODO: remove placeholder 0s for prod
            String l_DocID = "000" + Item.DocumentWorkflowItemID.ToString();

            return $"- {Item.ClientID}: <{l_DocID}> {Column}: {OldData} -> {Column}: {NewData}";
            //       - 36: <0001830> DocumentationAnalystID: NULL -> DocumentationAnalystID: 1
        }
    }
}