using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using WorkflowVerifyer.App.Helpers;

/// <summary>
/// NOT a replication of Certus.Core.DocumentWorkflowItem.
/// This class is an adaptation which only serves the 
/// functionailty of this program. Only the data necessary 
/// for this app's processes has been is included here.
/// </summary>
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

    #region Properties
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
    #endregion

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
        throw new Exception("Method functionaility may not yet be compatible with this program");

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
    public static Queue<DocumentWorkflowItem> GetLastestForClient(Int32 a_ClientID, DateTime a_LastRunTime)
    {
        Queue<DocumentWorkflowItem> l_queue = new Queue<DocumentWorkflowItem>();
        DataTable l_results = new DataTable();
        using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
        {
            using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "DocumentWorkflowItem_GetLastestForClient"))
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
                        l_tmp.DocumentationAnalystID = Convert.ToInt32(l_rdr["DocumentationAnalystID"]);
                        l_tmp.ComplianceAnalystID = Convert.ToInt32(l_rdr["ComplianceAnalystID"]);
                        l_tmp.CompanyID = Convert.ToInt32(l_rdr["CompanyID"]);
                        l_tmp.CompanyCertificateID = Convert.ToInt32(l_rdr["CompanyCertificateID"]);
                        l_tmp.Notes = l_rdr["Notes"].ToString();
                        l_tmp.DocumentWorkflowStatusID = Convert.ToInt32(l_rdr["DocumentWorkflowStatusID"]);
                        l_tmp.DocumentWorkflowUrgencyID = Convert.ToInt32(l_rdr["DocumentWorkflowUrgencyID"]);
                        l_tmp.LastStatusChangeDate = Convert.ToDateTime(l_rdr["LastStatusChangeDate"]);
                        // l_tmp.DateCreated = l_rdr["DateCreated"];
                        // l_tmp.IsDirty = l_rdr["IsDirty"];

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
    public void Save()
    {
        throw new Exception("Method functionaility may not yet be compatible with this program");

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
}
