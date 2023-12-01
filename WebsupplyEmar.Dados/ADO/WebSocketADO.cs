using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebsupplyEmar.Dados.ADO
{
    public class WebSocketADO
    {
        public static bool GERA_LOG(string Connection, string cLog)
        {
            ConexaoSQLServer Conn = new ConexaoSQLServer(Connection);

            string NomeProcedure = "SP_WEBSOCKETS_LOGS_INS";

            List<SqlParameter> parametros = new List<SqlParameter>();
            parametros.Add(new SqlParameter("@cLog", cLog));

            try
            {
                Conn.ExecutaComParametros(NomeProcedure, parametros);
                Conn.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                Conn.Dispose();

                return false;
            }
        }
    }
}
