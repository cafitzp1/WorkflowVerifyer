using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using WorkflowVerifyer.App.Helpers;

public class DocumentWorkflowItem
{
    public Int32 DocumentWorkflowItemID { get; set; }
    public Int32 ClientID { get; set; }
    public DateTime EmailDate { get; set; }
    public String EmailToAddress { get; set; }
    public String EmailCCAddress { get; set; }
    public String EmailFromAddress { get; set; }
    public String EmailFromName { get; set; }
    public String EmailSubject { get; set; }
    public String EmailBody { get; set; }
    public String EmailBodySearchText { get; set; }
    public Nullable<Int32> DocumentationAnalystID { get; set; }
    public Nullable<Int32> ComplianceAnalystID { get; set; }
    public Nullable<Int32> CompanyID { get; set; }
    public Nullable<Int32> CompanyCertificateID { get; set; }
    public String Notes { get; set; }
    public Int32 DocumentWorkflowStatusID { get; set; }
    public Nullable<Int32> DocumentWorkflowUrgencyID { get; set; }
    public Nullable<DateTime> LastStatusChangeDate { get; set; }
    public Nullable<DateTime> DateCreated { get; set; }
    public String FileName { get; set; }
    public String FileSize { get; set; }

    public DocumentWorkflowItem()
    {
        DocumentWorkflowItemID = -1;
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
    public static List<DocumentWorkflowItem> GetLastestForClient(Int32 a_ClientID, DateTime a_LastRunTime)
    {
        List<DocumentWorkflowItem> l_queue = new List<DocumentWorkflowItem>();
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

                        l_tmp.DocumentWorkflowItemID = Convert.ToInt32(l_rdr["DocumentWorkflowItemID"]);
                        l_tmp.ClientID = Convert.ToInt32(l_rdr["ClientID"]);
                        l_tmp.EmailDate = Convert.ToDateTime(l_rdr["EmailDate"]);
                        l_tmp.EmailToAddress = l_rdr["EmailToAddress"].ToString();
                        l_tmp.EmailFromAddress = l_rdr["EmailFromAddress"].ToString();
                        l_tmp.EmailSubject = l_rdr["EmailSubject"].ToString();
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
                        l_tmp.FileName = l_rdr["FileName"].ToString();
                        l_tmp.FileSize = l_rdr["FileSize"].ToString();

                        l_queue.Add(l_tmp);
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
    public override String ToString()
    {
        const Int32 MAX_LEN = 18;

        String l_DocID = DocumentWorkflowItemID.ToString();
        String l_ClientID = ClientID.ToString();
        String l_EmailDate = EmailDate.ToString("MM/dd/y hh:mm tt");
        String l_EmailFrom = !String.IsNullOrWhiteSpace(EmailFromAddress) && EmailFromAddress.Length >= MAX_LEN ?
            EmailFromAddress.Substring(0, MAX_LEN) :
            EmailFromAddress;
        String l_EmailSubject = !String.IsNullOrWhiteSpace(EmailSubject) && EmailSubject.Length >= MAX_LEN ?
            EmailSubject.Substring(0, MAX_LEN) :
            EmailSubject;
        String l_FileName = !String.IsNullOrWhiteSpace(FileName) && FileName.Length >= MAX_LEN ?
            FileName.Substring(0, MAX_LEN) :
            FileName;
        String l_FileSize = !String.IsNullOrWhiteSpace(FileSize) && FileSize.Length >= MAX_LEN ?
           FileSize.Substring(0, MAX_LEN) :
           FileSize;

        return l_DocID.PadLeft(8) +
            l_ClientID.PadLeft(5) +
            l_EmailDate.PadLeft(MAX_LEN + 2) +
            l_EmailFrom.PadLeft(MAX_LEN + 2) +
            l_EmailSubject.PadLeft(MAX_LEN + 2) +
            l_FileName.PadLeft(MAX_LEN + 2) +
            l_FileSize.PadLeft(MAX_LEN / 2);
    }
}

