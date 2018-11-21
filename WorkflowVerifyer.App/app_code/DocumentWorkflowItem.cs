using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using WorkflowVerifyer.App.Helpers;

/// <summary>
/// NOT a replication of Certus.Core.DocumentWorkflowItem.
/// This class is an adaptation which only serves the 
/// functionailty of this program. Only the data necessary 
/// for this app's processes has been is included here.
/// </summary>
public class DocumentWorkflowItem
{
    private Nullable<Int32> DocumentWorkflowItemID { get; set; }
    private Int32 ClientID { get; set; }
    private DateTime EmailDate { get; set; }
    private String EmailToAddress { get; set; }
    private String EmailCCAddress { get; set; }
    private String EmailFromAddress { get; set; }
    private String EmailFromName { get; set; }
    private String EmailSubject { get; set; }
    private String EmailBody { get; set; }
    private String EmailBodySearchText { get; set; }
    private Nullable<Int32> DocumentationAnalystID { get; set; }
    private Nullable<Int32> ComplianceAnalystID { get; set; }
    private Nullable<Int32> CompanyID { get; set; }
    private Nullable<Int32> CompanyCertificateID { get; set; }
    private String Notes { get; set; }
    private Int32 DocumentWorkflowStatusID { get; set; }
    private Nullable<Int32> DocumentWorkflowUrgencyID { get; set; }
    private Nullable<DateTime> LastStatusChangeDate { get; set; }
    private Nullable<DateTime> DateCreated { get; set; }
    private String FileName { get; set; }
    private String FileSize { get; set; }

    public DocumentWorkflowItem()
    {
        DocumentWorkflowItemID = new Nullable<Int32>();
        ClientID = 0;
        EmailDate = DateTime.MinValue;
        EmailToAddress = "";
        EmailCCAddress = "";
        EmailFromAddress = "";
        EmailFromName = "";
        EmailSubject = "";
        EmailBody = "";
        EmailBodySearchText = "";
        DocumentationAnalystID = new Nullable<Int32>();
        ComplianceAnalystID = new Nullable<Int32>();
        CompanyID = new Nullable<Int32>();
        CompanyCertificateID = new Nullable<Int32>();
        Notes = "";
        DocumentWorkflowStatusID = 0;
        DocumentWorkflowUrgencyID = new Nullable<Int32>();
        LastStatusChangeDate = new Nullable<DateTime>();
        DateCreated = new Nullable<DateTime>();
        FileName = "";
        FileSize = "";
    }
    public DocumentWorkflowItem(Int32 a_DocumentWorkflowItemID)
    {
        using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
        {
            using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "DocumentWorkflowItem_Get"))
            {
                l_cmd.Parameters.AddWithValue("@a_DocumentWorkflowItemID", a_DocumentWorkflowItemID);
                l_conn.Open();
                using (SqlDataReader l_rdr = l_cmd.ExecuteReader())
                {
                    if (l_rdr.Read())
                    {
                        DocumentWorkflowItemID = Convert.ToInt32(l_rdr["DocumentWorkflowItemID"]);
                        ClientID = Convert.ToInt32(l_rdr["ClientID"]);
                        EmailDate = Convert.ToDateTime(l_rdr["EmailDate"]);
                        EmailToAddress = l_rdr["EmailToAddress"].ToString();
                        EmailCCAddress = l_rdr["EmailCCAddress"].ToString();
                        EmailFromAddress = l_rdr["EmailFromAddress"].ToString();
                        EmailFromName = l_rdr["EmailFromName"].ToString();
                        EmailSubject = l_rdr["EmailSubject"].ToString();
                        EmailBody = l_rdr["EmailBody"].ToString();
                        EmailBodySearchText = l_rdr["EmailBodySearchText"].ToString();
                        if (!l_rdr.IsDBNull(l_rdr.GetOrdinal("DocumentationAnalystID")))
                            DocumentationAnalystID = Convert.ToInt32(l_rdr["DocumentationAnalystID"]);
                        if (!l_rdr.IsDBNull(l_rdr.GetOrdinal("ComplianceAnalystID")))
                            ComplianceAnalystID = Convert.ToInt32(l_rdr["ComplianceAnalystID"]);
                        if (!l_rdr.IsDBNull(l_rdr.GetOrdinal("CompanyID")))
                            CompanyID = Convert.ToInt32(l_rdr["CompanyID"]);
                        if (!l_rdr.IsDBNull(l_rdr.GetOrdinal("CompanyCertificateID")))
                            CompanyCertificateID = Convert.ToInt32(l_rdr["CompanyCertificateID"]);
                        Notes = l_rdr["Notes"].ToString();
                        DocumentWorkflowStatusID = Convert.ToInt32(l_rdr["DocumentWorkflowStatusID"]);
                        if (!l_rdr.IsDBNull(l_rdr.GetOrdinal("DocumentWorkflowUrgencyID")))
                            DocumentWorkflowUrgencyID = Convert.ToInt32(l_rdr["DocumentWorkflowUrgencyID"]);
                        if (!l_rdr.IsDBNull(l_rdr.GetOrdinal("LastStatusChangeDate")))
                            LastStatusChangeDate = Convert.ToDateTime(l_rdr["LastStatusChangeDate"]);
                        DateCreated = Convert.ToDateTime(l_rdr["DateCreated"]);
                        FileName = String.Empty;
                        FileSize = String.Empty;
                    }
                }
            }
        }
    }
    public static Queue<DocumentWorkflowItem> GetLastestForClient(Int32 a_ClientID, DateTime a_LastRunTime)
    {
        Queue<DocumentWorkflowItem> l_queue = new Queue<DocumentWorkflowItem>();
        DataTable l_results = new DataTable();
        using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
        {
            using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "DocumentWorkflowItem_GetLatestForClient"))
            {
                l_cmd.Parameters.AddWithValue("@a_ClientID", a_ClientID);
                l_cmd.Parameters.AddWithValue("@a_LastRunTime", a_LastRunTime);
                l_conn.Open();
                using (SqlDataReader l_rdr = l_cmd.ExecuteReader())
                {
                    while (l_rdr.Read())
                    {
                        DocumentWorkflowItem l_tmp = new DocumentWorkflowItem();

                        l_tmp.DocumentWorkflowItemID = Convert.ToInt32(l_rdr["DocumentWorkflowItem"]);
                        l_tmp.ClientID = Convert.ToInt32(l_rdr["ClientID"]);
                        l_tmp.EmailDate = Convert.ToDateTime(l_rdr["EmailDate"]);
                        l_tmp.EmailToAddress = l_rdr["EmailToAddress"].ToString();
                        l_tmp.EmailCCAddress = l_rdr["EmailCCAddress"].ToString();
                        l_tmp.EmailFromAddress = l_rdr["EmailFromAddress"].ToString();
                        l_tmp.EmailFromName = l_rdr["EmailFromName"].ToString();
                        l_tmp.EmailSubject = l_rdr["EmailSubject"].ToString();
                        l_tmp.EmailBody = l_rdr["EmailBody"].ToString();
                        l_tmp.EmailBodySearchText = l_rdr["EmailBodySearchText"].ToString();
                        if (!l_rdr.IsDBNull(l_rdr.GetOrdinal("DocumentationAnalystID")))
                            l_tmp.DocumentationAnalystID = Convert.ToInt32(l_rdr["DocumentationAnalystID"]);
                        if (!l_rdr.IsDBNull(l_rdr.GetOrdinal("ComplianceAnalystID")))
                            l_tmp.ComplianceAnalystID = Convert.ToInt32(l_rdr["ComplianceAnalystID"]);
                        if (!l_rdr.IsDBNull(l_rdr.GetOrdinal("CompanyID")))
                            l_tmp.CompanyID = Convert.ToInt32(l_rdr["CompanyID"]);
                        if (!l_rdr.IsDBNull(l_rdr.GetOrdinal("CompanyCertificateID")))
                            l_tmp.CompanyCertificateID = Convert.ToInt32(l_rdr["CompanyCertificateID"]);
                        l_tmp.Notes = l_rdr["Notes"].ToString();
                        l_tmp.DocumentWorkflowStatusID = Convert.ToInt32(l_rdr["DocumentWorkflowStatusID"]);
                        if (!l_rdr.IsDBNull(l_rdr.GetOrdinal("DocumentWorkflowUrgencyID")))
                            l_tmp.DocumentWorkflowUrgencyID = Convert.ToInt32(l_rdr["DocumentWorkflowUrgencyID"]);
                        l_tmp.LastStatusChangeDate = Convert.ToDateTime(l_rdr["LastStatusChangeDate"]);
                        l_tmp.FileName = l_rdr["FileName"].ToString();
                        l_tmp.FileSize = l_rdr["FileSize"].ToString();

                        l_queue.Enqueue(l_tmp);
                    }
                }
            }
        }
        return l_queue;
    }
    public static DataTable GetAttachments(Int32 a_DocumentWorkflowItemID)
    {
        throw new Exception("Method functionaility may not yet be compatible with this program");

        DataTable l_results = new DataTable();
        using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
        {
            using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "DocumentWorkflowIteGetAttachments"))
            {
                l_cmd.Parameters.AddWithValue("@a_DocumentWorkflowItemID", a_DocumentWorkflowItemID);
                using (SqlDataAdapter l_adapter = new SqlDataAdapter(l_cmd))
                {
                    l_conn.Open();
                    l_adapter.Fill(l_results);
                }
            }
        }
        return l_results;
    }
    public void Save()
    {
        throw new Exception("Method functionaility may not yet be compatible with this program");

        /* Need to make sure only the necessary fields are being modified.
         * Might need some overloaded method signatures to handle the different
         * item modifications.
         * */

        using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
        {
            using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "DocumentWorkflowIteSave"))
            {
                if (DocumentWorkflowItemID.HasValue)
                    l_cmd.Parameters.AddWithValue("@a_DocumentWorkflowItemID", DocumentWorkflowItemID.Value);
                l_cmd.Parameters.AddWithValue("@a_ClientID", ClientID);
                l_cmd.Parameters.AddWithValue("@a_EmailDate", EmailDate);
                l_cmd.Parameters.AddWithValue("@a_EmailToAddress", EmailToAddress);
                if (EmailCCAddress.Length > 0)
                    l_cmd.Parameters.AddWithValue("@a_EmailCCAddress", EmailCCAddress);
                l_cmd.Parameters.AddWithValue("@a_EmailFromAddress", EmailFromAddress);
                if (EmailFromName.Length > 0)
                    l_cmd.Parameters.AddWithValue("@a_EmailFromName", EmailFromName);
                l_cmd.Parameters.AddWithValue("@a_EmailSubject", EmailSubject);
                l_cmd.Parameters.AddWithValue("@a_EmailBody", EmailBody);
                l_cmd.Parameters.AddWithValue("@a_EmailBodySearchText", EmailBodySearchText);
                if (DocumentationAnalystID.HasValue)
                    l_cmd.Parameters.AddWithValue("@a_DocumentationAnalystID", DocumentationAnalystID);
                if (ComplianceAnalystID.HasValue)
                    l_cmd.Parameters.AddWithValue("@a_ComplianceAnalystID", ComplianceAnalystID);
                if (CompanyID.HasValue)
                    l_cmd.Parameters.AddWithValue("@a_CompanyID", CompanyID);
                if (CompanyCertificateID.HasValue)
                    l_cmd.Parameters.AddWithValue("@a_CompanyCertificateID", CompanyCertificateID);
                if (Notes.Length > 0)
                    l_cmd.Parameters.AddWithValue("@a_Notes", Notes);
                l_cmd.Parameters.AddWithValue("@a_LastStatusChangeDate", LastStatusChangeDate);
                l_cmd.Parameters.AddWithValue("@a_DocumentWorkflowStatusID", DocumentWorkflowStatusID);
                if (DocumentWorkflowUrgencyID.HasValue)
                    l_cmd.Parameters.AddWithValue("@a_DocumentWorkflowUrgencyID", DocumentWorkflowUrgencyID);
                l_conn.Open();
                DocumentWorkflowItemID = Convert.ToInt32(l_cmd.ExecuteScalar());
            }
        }
    }
    public override String ToString()
    {
        Regex checkLength = new Regex(@"^.{18}", RegexOptions.Compiled);

        return DocumentWorkflowItemID.ToString().PadLeft(10) +
        ClientID.ToString().PadLeft(5) +
        EmailDate.ToString("MM/dd/y hh:mm tt").PadLeft(18) +
        checkLength.Replace(EmailFromAddress, String.Empty).PadLeft(20) +
        checkLength.Replace(EmailSubject, String.Empty).PadLeft(20) +
        checkLength.Replace(FileName, String.Empty).PadLeft(20) +
        checkLength.Replace(FileSize, String.Empty).PadLeft(20);
    }
}

