using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace WebsupplyEmar.Dados.ADO
{
    public class ConexaoMySQL
    {
        public MySqlConnection Con { get; set; }
        public string ErroConexao { get; set; }


        public ConexaoMySQL(string Connection)
        {
            try
            {

                CultureInfo culture = new CultureInfo("pt-BR");
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

                Con = new MySqlConnection(Connection);
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

        public MySqlDataReader Executa(string query)
        {
            try
            {
                var oCmd = new MySqlCommand(query, Con);
                return oCmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public MySqlDataReader ExecutaComParametros(string query, List<MySqlParameter> parametros)
        {
            try
            {
                var oCmd = new MySqlCommand(query, Con);
                oCmd.CommandType = System.Data.CommandType.StoredProcedure;
                if (parametros != null)
                {
                    foreach (MySqlParameter item in parametros)
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

        public MySqlCommand ExecutaComParametrosSemRetorno(string query, List<MySqlParameter> parametros)
        {
            try
            {
                var oCmd = new MySqlCommand(query, Con);
                oCmd.CommandType = System.Data.CommandType.StoredProcedure;
                if (parametros != null)
                {
                    foreach (MySqlParameter item in parametros)
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
                var oCmd = new MySqlCommand(query, Con);
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
