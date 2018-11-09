using System;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using WorkflowVerifyer.App.Helpers;

public class DocumentWorkflowItem
{
    private Nullable<Int32> m_DocumentWorkflowItemID;
    private Int32 m_ClientID;
    private DateTime m_EmailDate;
    private String m_EmailToAddress;
    private String m_EmailCCAddress;
    private String m_EmailFromAddress;
    private String m_EmailFromName;
    private String m_EmailSubject;
    private String m_EmailBody;
    private String m_EmailBodySearchText;
    private Nullable<Int32> m_DocumentationAnalystID;
    private Nullable<Int32> m_ComplianceAnalystID;
    private Nullable<Int32> m_CompanyID;
    private Nullable<Int32> m_CompanyCertificateID;
    private String m_Notes;
    private Int32 m_DocumentWorkflowStatusID;
    private Nullable<Int32> m_DocumentWorkflowUrgencyID;
    private Nullable<DateTime> m_LastStatusChangeDate;
    private Nullable<DateTime> m_DateCreated;
    private Boolean m_IsDirty;

    public Nullable<Int32> DocumentWorkflowItemID
    {
        get => m_DocumentWorkflowItemID;
        set => m_DocumentWorkflowItemID = value;
    }
    public Int32 ClientID
    {
        get => m_ClientID;
        set => m_ClientID = value;
    }
    public DateTime EmailDate
    {
        get => m_EmailDate;
        set => m_EmailDate = value;
    }
    public String EmailToAddress
    {
        get => m_EmailToAddress;
        set => m_EmailToAddress = value;
    }
    public String EmailCCAddress
    {
        get => m_EmailCCAddress;
        set => m_EmailCCAddress = value;
    }
    public String EmailFromAddress
    {
        get => m_EmailFromAddress;
        set => m_EmailFromAddress = value;
    }
    public String EmailFromName
    {
        get => m_EmailFromName;
        set => m_EmailFromName = value;
    }
    public String EmailSubject
    {
        get => m_EmailSubject;
        set => m_EmailSubject = value;
    }
    public String EmailBody
    {
        get => m_EmailBody;
        set => m_EmailBody = value;
    }
    public String EmailBodySearchText
    {
        get => m_EmailBodySearchText;
        set => m_EmailBodySearchText = value;
    }
    public Nullable<Int32> DocumentationAnalystID
    {
        get => m_DocumentationAnalystID;
        set
        {
            if (!Nullable.Equals(value, m_DocumentationAnalystID))
            {
                m_IsDirty = true;
                m_DocumentationAnalystID = value;
            }
        }
    }
    public Nullable<Int32> ComplianceAnalystID
    {
        get => m_ComplianceAnalystID;
        set
        {
            if (!Nullable.Equals(value, m_ComplianceAnalystID))
            {
                m_IsDirty = true;
                m_ComplianceAnalystID = value;
            }
        }
    }
    public Nullable<Int32> CompanyID
    {
        get => m_CompanyID;
        set
        {
            if (!Nullable.Equals(value, m_CompanyID))
            {
                m_IsDirty = true;
                m_CompanyID = value;
            }
        }
    }
    public Nullable<Int32> CompanyCertificateID
    {
        get => m_CompanyCertificateID;
        set
        {
            if (!Nullable.Equals(value, m_CompanyCertificateID))
            {
                m_IsDirty = true;
                m_CompanyCertificateID = value;
            }
        }
    }
    public String Notes
    {
        get => m_Notes;
        set
        {
            if (m_Notes != value)
            {
                m_IsDirty = true;
                m_Notes = value;
            }
        }
    }
    public Int32 DocumentWorkflowStatusID
    {
        get => m_DocumentWorkflowStatusID;
        set
        {
            if (!Nullable.Equals(value, m_DocumentWorkflowStatusID))
            {
                m_IsDirty = true;
                m_DocumentWorkflowStatusID = value;
                m_LastStatusChangeDate = DateTime.Now;
            }
        }
    }
    public Nullable<Int32> DocumentWorkflowUrgencyID
    {
        get => m_DocumentWorkflowUrgencyID;
        set
        {
            if (!Nullable.Equals(value, m_DocumentWorkflowUrgencyID))
            {
                m_IsDirty = true;
                m_DocumentWorkflowUrgencyID = value;
            }
        }
    }
    public Nullable<DateTime> LastStatusChangeDate
    {
        get => m_LastStatusChangeDate;
        set
        {
            if (!Nullable.Equals(value, m_LastStatusChangeDate))
            {
                m_IsDirty = true;
                m_LastStatusChangeDate = value;
            }
        }
    }
    public Nullable<DateTime> DateCreated
    {
        get => m_DateCreated;
    }

    public DocumentWorkflowItem()
    {
        m_DocumentWorkflowItemID = new Nullable<Int32>();
        m_ClientID = 0;
        m_EmailDate = DateTime.MinValue;
        m_EmailToAddress = "";
        m_EmailCCAddress = "";
        m_EmailFromAddress = "";
        m_EmailFromName = "";
        m_EmailSubject = "";
        m_EmailBody = "";
        m_EmailBodySearchText = "";
        m_DocumentationAnalystID = new Nullable<Int32>();
        m_ComplianceAnalystID = new Nullable<Int32>();
        m_CompanyID = new Nullable<Int32>();
        m_CompanyCertificateID = new Nullable<Int32>();
        m_Notes = "";
        m_DocumentWorkflowStatusID = 0;
        m_DocumentWorkflowUrgencyID = new Nullable<Int32>();
        m_LastStatusChangeDate = new Nullable<DateTime>();
        m_DateCreated = new Nullable<DateTime>();
        m_IsDirty = true;
    }

    public DocumentWorkflowItem(Int32 a_DocumentWorkflowItemID) : this()
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
                        m_DocumentWorkflowItemID = Convert.ToInt32(l_rdr["DocumentWorkflowItemID"]);
                        m_ClientID = Convert.ToInt32(l_rdr["ClientID"]);
                        m_EmailDate = Convert.ToDateTime(l_rdr["EmailDate"]);
                        m_EmailToAddress = l_rdr["EmailToAddress"].ToString();
                        m_EmailCCAddress = l_rdr["EmailCCAddress"].ToString();
                        m_EmailFromAddress = l_rdr["EmailFromAddress"].ToString();
                        m_EmailFromName = l_rdr["EmailFromName"].ToString();
                        m_EmailSubject = l_rdr["EmailSubject"].ToString();
                        m_EmailBody = l_rdr["EmailBody"].ToString();
                        m_EmailBodySearchText = l_rdr["EmailBodySearchText"].ToString();
                        if (!l_rdr.IsDBNull(l_rdr.GetOrdinal("DocumentationAnalystID")))
                            m_DocumentationAnalystID = Convert.ToInt32(l_rdr["DocumentationAnalystID"]);
                        if (!l_rdr.IsDBNull(l_rdr.GetOrdinal("ComplianceAnalystID")))
                            m_ComplianceAnalystID = Convert.ToInt32(l_rdr["ComplianceAnalystID"]);
                        if (!l_rdr.IsDBNull(l_rdr.GetOrdinal("CompanyID")))
                            m_CompanyID = Convert.ToInt32(l_rdr["CompanyID"]);
                        if (!l_rdr.IsDBNull(l_rdr.GetOrdinal("CompanyCertificateID")))
                            m_CompanyCertificateID = Convert.ToInt32(l_rdr["CompanyCertificateID"]);
                        m_Notes = l_rdr["Notes"].ToString();
                        m_DocumentWorkflowStatusID = Convert.ToInt32(l_rdr["DocumentWorkflowStatusID"]);
                        if (!l_rdr.IsDBNull(l_rdr.GetOrdinal("DocumentWorkflowUrgencyID")))
                            m_DocumentWorkflowUrgencyID = Convert.ToInt32(l_rdr["DocumentWorkflowUrgencyID"]);

                        if (!l_rdr.IsDBNull(l_rdr.GetOrdinal("LastStatusChangeDate")))
                            m_LastStatusChangeDate = Convert.ToDateTime(l_rdr["LastStatusChangeDate"]);
                        m_DateCreated = Convert.ToDateTime(l_rdr["DateCreated"]);
                        m_IsDirty = false;
                    }
                }
            }
        }
    }

    public void Save()
    {
        if (m_IsDirty)
        {
            using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
            {
                using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "DocumentWorkflowItem_Save"))
                {
                    if (m_DocumentWorkflowItemID.HasValue)
                        l_cmd.Parameters.AddWithValue("@a_DocumentWorkflowItemID", m_DocumentWorkflowItemID.Value);
                    l_cmd.Parameters.AddWithValue("@a_ClientID", m_ClientID);
                    l_cmd.Parameters.AddWithValue("@a_EmailDate", m_EmailDate);
                    l_cmd.Parameters.AddWithValue("@a_EmailToAddress", m_EmailToAddress);
                    if (m_EmailCCAddress.Length > 0)
                        l_cmd.Parameters.AddWithValue("@a_EmailCCAddress", m_EmailCCAddress);
                    l_cmd.Parameters.AddWithValue("@a_EmailFromAddress", m_EmailFromAddress);
                    if (m_EmailFromName.Length > 0)
                        l_cmd.Parameters.AddWithValue("@a_EmailFromName", m_EmailFromName);
                    l_cmd.Parameters.AddWithValue("@a_EmailSubject", m_EmailSubject);
                    l_cmd.Parameters.AddWithValue("@a_EmailBody", m_EmailBody);
                    l_cmd.Parameters.AddWithValue("@a_EmailBodySearchText", m_EmailBodySearchText);
                    if (m_DocumentationAnalystID.HasValue)
                        l_cmd.Parameters.AddWithValue("@a_DocumentationAnalystID", m_DocumentationAnalystID);
                    if (m_ComplianceAnalystID.HasValue)
                        l_cmd.Parameters.AddWithValue("@a_ComplianceAnalystID", m_ComplianceAnalystID);
                    if (m_CompanyID.HasValue)
                        l_cmd.Parameters.AddWithValue("@a_CompanyID", m_CompanyID);
                    if (m_CompanyCertificateID.HasValue)
                        l_cmd.Parameters.AddWithValue("@a_CompanyCertificateID", m_CompanyCertificateID);
                    if (m_Notes.Length > 0)
                        l_cmd.Parameters.AddWithValue("@a_Notes", m_Notes);
                    l_cmd.Parameters.AddWithValue("@a_LastStatusChangeDate", m_LastStatusChangeDate);
                    l_cmd.Parameters.AddWithValue("@a_DocumentWorkflowStatusID", m_DocumentWorkflowStatusID);
                    if (m_DocumentWorkflowUrgencyID.HasValue)
                        l_cmd.Parameters.AddWithValue("@a_DocumentWorkflowUrgencyID", m_DocumentWorkflowUrgencyID);
                    l_conn.Open();
                    m_DocumentWorkflowItemID = Convert.ToInt32(l_cmd.ExecuteScalar());
                }
            }
            m_IsDirty = false;
        }
    }

    // public void InsertFile(string a_FileName, string a_FileMIME, byte[] a_FileBytes, bool a_SendToExtractor)
    // {
    //     if (m_DocumentWorkflowItemID.HasValue)
    //     {
    //         var l_CertusFile = new CertusFile();
    //         l_CertusFile.FileName = a_FileName;
    //         l_CertusFile.FileMIME = a_FileMIME;
    //         l_CertusFile.ClientID = m_ClientID;
    //         l_CertusFile.Save(a_FileBytes);

    //         if (l_CertusFile.CertusFileID.HasValue)
    //         {
    //             using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
    //             {
    //                 using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "DocumentWorkflowItemAttachment_Insert"))
    //                 {
    //                     l_cmd.Parameters.AddWithValue("@a_DocumentWorkflowItemID", m_DocumentWorkflowItemID.Value);
    //                     l_cmd.Parameters.AddWithValue("@a_CertusFileID", l_CertusFile.CertusFileID);
    //                     l_cmd.Parameters.AddWithValue("@a_AttachmentName", l_CertusFile.FileName);
    //                     l_conn.Open();
    //                     l_cmd.ExecuteNonQuery();
    //                 }
    //             }
    //             if (a_SendToExtractor)
    //                 l_CertusFile.ExtractWorkflowItem();
    //         }
    //     }
    // }

    // public static void RenameFile(Int32 a_CertusFileID, string a_NewFileName)
    // {
    //     CertusFile l_File = new CertusFile(a_CertusFileID);
    //     if (l_File.FileName != a_NewFileName)
    //     {
    //         l_File.FileName = a_NewFileName;
    //         l_File.Save();
    //     }
    // }

    public static DataRow GetForView(Int32 a_DocumentWorkflowItemID)
    {
        DataTable l_results = new DataTable();
        using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
        {
            using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "DocumentWorkflowItem_GetForView"))
            {
                l_cmd.Parameters.AddWithValue("@a_DocumentWorkflowItemID", a_DocumentWorkflowItemID);
                using (SqlDataAdapter l_adapter = new SqlDataAdapter(l_cmd))
                {
                    l_conn.Open();
                    l_adapter.Fill(l_results);
                }
            }
        }

        if (l_results.Rows.Count > 0)
            return l_results.Rows[0];
        else
            return null;
    }

    public static Int32 GetByCertusFileID(Int32 a_CertusFileID)
    {
        DataTable l_results = new DataTable();
        using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
        {
            using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "DocumentWorkflowItem_GetByCertusFileID"))
            {
                l_cmd.Parameters.AddWithValue("@a_CertusFileID", a_CertusFileID);
                using (SqlDataAdapter l_adapter = new SqlDataAdapter(l_cmd))
                {
                    l_conn.Open();
                    l_adapter.Fill(l_results);
                }
            }
        }
        if (l_results.Rows.Count == 1)
            return Convert.ToInt32(l_results.Rows[0]["DocumentWorkflowItemID"]);
        else
            return 0;
    }

    public static DataTable GetRecordsWithOpenItemsByClient(Int32 a_ClientID)
    {
        DataTable l_results = new DataTable();
        using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
        {
            using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "DocumentWorkflowItem_GetRecordsWithOpenItemsByClient"))
            {
                l_cmd.Parameters.AddWithValue("@a_ClientID", a_ClientID);
                using (SqlDataAdapter l_adapter = new SqlDataAdapter(l_cmd))
                {
                    l_conn.Open();
                    l_adapter.Fill(l_results);
                }
            }
        }
        return l_results;
    }

    public static DataTable GetAttachments(Int32 a_DocumentWorkflowItemID)
    {
        DataTable l_results = new DataTable();
        using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
        {
            using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "DocumentWorkflowItem_GetAttachments"))
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

    // public static Int32 SaveEmailBody(Int32 a_DocumentWorkflowItemID, string a_FileName)
    // {
    //     DocumentWorkflowItem l_DocumentWorkFlowItem = new DocumentWorkflowItem(a_DocumentWorkflowItemID);

    //     string l_EmailBody = "";
    //     byte[] l_FileBytes = Util.GetEmailContent(a_DocumentWorkflowItemID);
    //     if (l_FileBytes != null)
    //         l_EmailBody = Encoding.Default.GetString(l_FileBytes);

    //     string l_EmailFrom = l_DocumentWorkFlowItem.EmailFromAddress;
    //     string l_EmailSent = l_DocumentWorkFlowItem.EmailDate.ToString();
    //     string l_EmailTo = l_DocumentWorkFlowItem.EmailToAddress;
    //     string l_EmailCC = l_DocumentWorkFlowItem.EmailCCAddress;
    //     string l_EmailSubject = l_DocumentWorkFlowItem.EmailSubject;

    //     string l_EmailHeader = "<table><tr><td width=\"12%\" style=\"font-weight: bold;vertical-align:top\">From:</td><td>" + l_EmailFrom + "</td></tr>";
    //     l_EmailHeader = l_EmailHeader + "<tr><td width=\"12%\" style=\"font-weight: bold;text-align:left;vertical-align:top\">Sent:</td><td>" + l_EmailSent + "</td></tr>";
    //     l_EmailHeader = l_EmailHeader + "<tr><td width=\"12%\" style=\"font-weight: bold;text-align:left;vertical-align:top\">To:</td><td>" + l_EmailTo + "</td></tr>";
    //     if (l_EmailCC.Length > 0)
    //         l_EmailHeader = l_EmailHeader + "<tr><td width=\"12%\" style=\"font-weight: bold;text-align:left;vertical-align:top\">Cc:</td><td>" + l_EmailCC + "</td></tr>";
    //     l_EmailHeader = l_EmailHeader + "<tr><td width=\"12%\" style=\"font-weight: bold;text-align:left;vertical-align:top\">Subject:</td><td>" + l_EmailSubject + "</td></tr></table><br />";
    //     l_EmailHeader = l_EmailHeader + "<hr style=\"border-bottom-style: solid;\"><br /><br />";

    //     HtmlToPdf l_HtmltoPdf = new HtmlToPdf();
    //     l_HtmltoPdf.SerialNumber = ConfigurationManager.AppSettings("HiQPdfSerialNumber");
    //     l_HtmltoPdf.Document.PageSize = PdfPageSize.A4;
    //     l_HtmltoPdf.Document.PageOrientation = PdfPageOrientation.Portrait;
    //     l_HtmltoPdf.Document.Margins = new PdfMargins(20);
    //     l_HtmltoPdf.Document.Margins.Top = 30;
    //     l_HtmltoPdf.Document.FitPageWidth = true;

    //     l_EmailBody = l_EmailBody.TrimStart();
    //     if (l_EmailBody.StartsWith("<"))
    //         l_EmailBody = l_EmailHeader + l_EmailBody;
    //     else
    //     {
    //         l_EmailBody = l_EmailBody.Replace(Constants.vbLf, "<br />");
    //         l_EmailBody = l_EmailHeader + "<p>" + l_EmailBody + "</p>";
    //     }

    //     CertusFile l_CertusFile = new CertusFile();
    //     l_CertusFile.ClientID = l_DocumentWorkFlowItem.ClientID;
    //     if ((a_FileName.EndsWith(".pdf")))
    //         l_CertusFile.FileName = a_FileName;
    //     else
    //         l_CertusFile.FileName = a_FileName + ".pdf";
    //     l_CertusFile.FileMIME = "application/pdf";
    //     l_CertusFile.Save(l_HtmltoPdf.ConvertHtmlToMemory(l_EmailBody, null/* TODO Change to default(_) if this is not a reference type */));

    //     return l_CertusFile.CertusFileID;
    // }
}
