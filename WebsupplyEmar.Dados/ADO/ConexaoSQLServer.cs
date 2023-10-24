using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebsupplyEmar.Dados.ADO
{
    public class ConexaoSQLServer
    {
        public SqlConnection Con { get; set; }
        public string ErroConexao { get; set; }


        public ConexaoSQLServer(string Connection)
        {
            try
            {

                CultureInfo culture = new CultureInfo("pt-BR");
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

                Con = new SqlConnection(Connection);
                this.AbreConexao();
            }
            catch (Exception ex)
            {
                ErroConexao = "Conexao => AbreConexao - " + ex.Message;

                throw new Exception(ErroConexao);
            }
        }

        private void AbreConexao()
        {
            if (Con.State == System.Data.ConnectionState.Closed)
            {
                Con.Open();
            }
        }

        private void FechaConexao()
        {
            try
            {
                if (Con.State == System.Data.ConnectionState.Open)
                {
                    Con.Close();
                }
            }
            catch (Exception ex)
            {
                ErroConexao = "Conexao => FechaConexao - " + ex.Message;
                throw new Exception(ErroConexao);
            }
        }

        public SqlDataReader Executa(string query)
        {
            try
            {
                var oCmd = new SqlCommand(query, Con);
                return oCmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public SqlDataReader ExecutaComParametros(string query, List<SqlParameter> parametros)
        {
            try
            {
                var oCmd = new SqlCommand(query, Con);
                oCmd.CommandType = System.Data.CommandType.StoredProcedure;
                if (parametros != null)
                {
                    foreach (SqlParameter item in parametros)
                    {
                        oCmd.Parameters.Add(item);
                    }
                }
                return oCmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public SqlCommand ExecutaComParametrosSemRetorno(string query, List<SqlParameter> parametros)
        {
            try
            {
                var oCmd = new SqlCommand(query, Con);
                oCmd.CommandType = System.Data.CommandType.StoredProcedure;
                if (parametros != null)
                {
                    foreach (SqlParameter item in parametros)
                    {
                        oCmd.Parameters.Add(item);
                    }
                }
                oCmd.ExecuteNonQuery();
                return oCmd;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void ExecutaSemRetorno(string query)
        {
            try
            {
                var oCmd = new SqlCommand(query, Con);
                oCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void Dispose()
        {
            this.FechaConexao();
        }
    }
}
