using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebsupplyEmar.Dominio.Dto;
using WebsupplyEmar.Dados.ADO;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace WebsupplyEmar.Dados.ADO
{
    public class EmarADO
    {
        public static EmarAmbienteResponseDto CONSULTA_AMBIENTE_ARQUIVOS(string Connection, string cCGCMatriz, string cAmbiente, string cTabela)
        {
            EmarAmbienteResponseDto resultEmarAmbiente = new EmarAmbienteResponseDto();

            // Faz a consutla dos dados da Solicitação de Pagamento
            ConexaoSQLServer Conn = new ConexaoSQLServer(Connection);

            string NomeProcedure = "SP_EMAR_AMBIENTE_SEL";

            List<SqlParameter> parametros = new List<SqlParameter>();
            parametros.Add(new SqlParameter("@cCGCMatriz", cCGCMatriz));
            parametros.Add(new SqlParameter("@cAmbiente", cAmbiente));
            parametros.Add(new SqlParameter("@cTabela", cTabela));

            using (var reader = Conn.ExecutaComParametros(NomeProcedure, parametros))
            {
                while (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        resultEmarAmbiente.COD_AMBIENTE_ARQUIVOS = (int)reader["COD_AMBIENTE_ARQUIVOS"];
                        resultEmarAmbiente.CGCMatriz = String.IsNullOrEmpty(reader["CGCMatriz"].ToString()) ? "" : reader["CGCMatriz"].ToString().Trim();
                        resultEmarAmbiente.Empresa = String.IsNullOrEmpty(reader["Empresa"].ToString()) ? "" : reader["Empresa"].ToString().Trim();
                        resultEmarAmbiente.Ambiente = String.IsNullOrEmpty(reader["Ambiente"].ToString()) ? "" : reader["Ambiente"].ToString().Trim();
                        resultEmarAmbiente.DriverFisicoArquivos = String.IsNullOrEmpty(reader["DriverFisicoArquivos"].ToString()) ? "" : reader["DriverFisicoArquivos"].ToString().Trim();
                        resultEmarAmbiente.DATAHORARIO_CADASTRO = (DateTime)reader["DATAHORARIO_CADASTRO"];
                        resultEmarAmbiente.DATAHORARIO_ATUALIZACAO = reader["DATAHORARIO_ATUALIZACAO"] == DBNull.Value ? null : (DateTime)reader["DATAHORARIO_ATUALIZACAO"]; ;
                        resultEmarAmbiente.INDEXCLUI = String.IsNullOrEmpty(reader["INDEXCLUI"].ToString()) ? "" : reader["INDEXCLUI"].ToString().Trim();
                    }
                    reader.NextResult();
                }
            }

            Conn.Dispose();

            return resultEmarAmbiente;
        }

        public static bool GERA_LOG(string Connection, string cLog)
        {
            ConexaoSQLServer Conn = new ConexaoSQLServer(Connection);

            string NomeProcedure = "SP_EMAR_LOG_INS";

            List<SqlParameter> parametros = new List<SqlParameter>();
            parametros.Add(new SqlParameter("@cLog", cLog));

            try
            {
                Conn.ExecutaComParametros(NomeProcedure, parametros);
                Conn.Dispose();

                return true;
            }
            catch(Exception ex)
            {
                Conn.Dispose();

                return false;
            }
        }

        public static bool GERA_LOG_PROCESSAMENTO(
            string Connection, string cCGC, string cCCUSTO,
            string cREQUISIT, string cCDGPED, string cCL_CDG,
            string cCODPROD, string cCODITEM, string cCGCF, string cGRAPH_EMAIL_ID, string cGRAPH_EMAIL_SUBJECT,
            string cGRAPH_EMAIL_SENDER_NAME, string cGRAPH_EMAIL_SENDER_EMAIL, string cGRAPH_EMAIL_BODY,
            string cGRAPH_EMAIL_HASATTACHMENTS, string cANEXO, string cTOKEN_JWT,
            string cTOKEN_JWT_DECRYPT, string cSTATUS, string cDESCRICAO_LOG)
        {
            ConexaoSQLServer Conn = new ConexaoSQLServer(Connection);

            string NomeProcedure = "SP_EMAR_LOGS_PROCESSAMENTO_INS";

            List<SqlParameter> parametros = new List<SqlParameter>();
            parametros.Add(new SqlParameter("@cCGC", cCGC));
            parametros.Add(new SqlParameter("@cCCUSTO", cCCUSTO));
            parametros.Add(new SqlParameter("@cREQUISIT", cREQUISIT));
            parametros.Add(new SqlParameter("@iCDGPED", cCDGPED));
            parametros.Add(new SqlParameter("@cCODPROD", cCODPROD));
            parametros.Add(new SqlParameter("@iCODITEM", cCODITEM));
            parametros.Add(new SqlParameter("@iCL_CDG", cCL_CDG));
            parametros.Add(new SqlParameter("@cCGCF", cCGCF));
            parametros.Add(new SqlParameter("@cGRAPH_EMAIL_ID", cGRAPH_EMAIL_ID));
            parametros.Add(new SqlParameter("@cGRAPH_EMAIL_SUBJECT", cGRAPH_EMAIL_SUBJECT));
            parametros.Add(new SqlParameter("@cGRAPH_EMAIL_SENDER_NAME", cGRAPH_EMAIL_SENDER_NAME));
            parametros.Add(new SqlParameter("@cGRAPH_EMAIL_SENDER_EMAIL", cGRAPH_EMAIL_SENDER_EMAIL));
            parametros.Add(new SqlParameter("@cGRAPH_EMAIL_BODY", cGRAPH_EMAIL_BODY));
            parametros.Add(new SqlParameter("@cGRAPH_EMAIL_HASATTACHMENTS", cGRAPH_EMAIL_HASATTACHMENTS));
            parametros.Add(new SqlParameter("@cANEXO", cANEXO));
            parametros.Add(new SqlParameter("@cTOKEN_JWT", cTOKEN_JWT));
            parametros.Add(new SqlParameter("@cTOKEN_JWT_DECRYPT", cTOKEN_JWT_DECRYPT));
            parametros.Add(new SqlParameter("@cSTATUS", cSTATUS));
            parametros.Add(new SqlParameter("@cDESCRICAO_LOG", cDESCRICAO_LOG));

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

        public static bool PROCESSA_ANEXO_PEDIDOITENS(string Connection, string cCDGPED, string cNome_Arquivo, string cCodProd, string cCGCF)
        {
            ConexaoSQLServer Conn = new ConexaoSQLServer(Connection);

            string NomeProcedure = "SP_Cheil_PedidosItens_temp_Anexos_ins";

            List<SqlParameter> parametros = new List<SqlParameter>();
            parametros.Add(new SqlParameter("@icdgped", cCDGPED));
            parametros.Add(new SqlParameter("@vnome_arquivo", cNome_Arquivo));
            parametros.Add(new SqlParameter("@cCodProd", cCodProd));
            parametros.Add(new SqlParameter("@cCGCF", cCGCF));

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

        public static bool PROCESSA_ANEXO_CL_PROCESSO_ANEXO(string Connection, string CL_CDG, string TIPO, string ARQUIVO, string Visualiza)
        {
            ConexaoSQLServer Conn = new ConexaoSQLServer(Connection);

            string NomeProcedure = "SP_EMPRESA_CL_PROCESSO_ANEXO_INS";

            List<SqlParameter> parametros = new List<SqlParameter>();
            parametros.Add(new SqlParameter("@CL_CDG", CL_CDG));
            parametros.Add(new SqlParameter("@TIPO", TIPO));
            parametros.Add(new SqlParameter("@ARQUIVO", ARQUIVO));
            parametros.Add(new SqlParameter("@Visualiza", Visualiza));

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

        public static string CONSULTA_JWT_EXISTENTE(string Connection, string cJWT)
        {
            ConexaoSQLServer Conn = new ConexaoSQLServer(Connection);

            string NomeProcedure = "SP_EMAR_VALIDA_JWT";

            List<SqlParameter> parametros = new List<SqlParameter>();
            parametros.Add(new SqlParameter("@cJWT", cJWT));

            string Valido = "S";

            using (var reader = Conn.ExecutaComParametros(NomeProcedure, parametros))
            {
                while (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Valido = String.IsNullOrEmpty(reader["Valido"].ToString()) ? "S" : reader["Valido"].ToString().Trim();
                    }
                    reader.NextResult();
                }
            }

            Conn.Dispose();

            return Valido;
        }

        public static int PROCESSA_PEDIDOS_ARQUIVOS(string Connection, string iCdgPed, string iID_Tipo, string vNome_Arquivo)
        {
            int ID_Arquivo = 0;

            ConexaoSQLServer Conn = new ConexaoSQLServer(Connection);

            string NomeProcedure = "Pedidos_Arquivos_Ins";

            List<SqlParameter> parametros = new List<SqlParameter>();
            parametros.Add(new SqlParameter("@iCdgPed", iCdgPed));
            parametros.Add(new SqlParameter("@iID_Tipo", iID_Tipo));
            parametros.Add(new SqlParameter("@vNome_Arquivo", vNome_Arquivo));

            try
            {
                using (var reader = Conn.ExecutaComParametros(NomeProcedure, parametros))
                {
                    while (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ID_Arquivo = Convert.ToInt32(reader["ID_Arquivo"]);
                        }
                        reader.NextResult();
                    }
                }

                Conn.Dispose();

                return ID_Arquivo;
            }
            catch (Exception ex)
            {
                Conn.Dispose();

                return ID_Arquivo;
            }
        }
    }
}
