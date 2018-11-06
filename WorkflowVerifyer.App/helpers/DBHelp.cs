using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace WorkflowVerifyer.App.Helpers
{
    public class DBHelp
    {
        public static SqlConnection CreateSQLConnection()
        {
            SqlConnection l_conn = new SqlConnection();
            l_conn.ConnectionString = ConfigurationManager.ConnectionStrings["LocalSqlServer"].ToString();
            return l_conn;
        }
        public static SqlCommand CreateCommand(SqlConnection a_conn, string a_cmdname)
        {
            SqlCommand l_cmd;
            l_cmd = a_conn.CreateCommand();
            l_cmd.CommandText = a_cmdname;
            l_cmd.CommandType = CommandType.StoredProcedure;
            l_cmd.CommandTimeout = 450;
            return l_cmd;
        }
        public static SqlCommand CreateTextCommand(SqlConnection a_Conn, string a_Cmd)
        {
            SqlCommand l_cmd;
            l_cmd = a_Conn.CreateCommand();
            l_cmd.CommandText = a_Cmd;
            l_cmd.CommandType = CommandType.Text;
            l_cmd.CommandTimeout = 450;
            return l_cmd;
        }
        public static SqlParameter AddReturnParameter(SqlCommand a_cmd, SqlDbType a_dbtype)
        {
            SqlParameter l_sqlparameter = a_cmd.Parameters.Add("@RETURN", a_dbtype);
            l_sqlparameter.Direction = ParameterDirection.ReturnValue;
            return l_sqlparameter;
        }
        public static SqlParameter AddOutputParameter(SqlCommand a_cmd, string a_paramname, SqlDbType a_dbtype)
        {
            SqlParameter l_param = a_cmd.Parameters.AddWithValue(a_paramname, a_dbtype);
            l_param.Direction = ParameterDirection.Output;
            return l_param;
        }
    }
}