using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebsupplyEmar.Dominio.Dto;

namespace WebsupplyEmar.Dados.ADO
{
    public class CockpitADO
    {
        public static CockpitResponseDto CONSULTA_CARDS(string Connection, CockpitRequestDto objRequest, CockpitResponseDto objResponse)
        {
            // Faz a consutla dos dados da Solicitação de Pagamento
            ConexaoSQLServer Conn = new ConexaoSQLServer(Connection);

            string NomeProcedure = "SP_EMAR_COCKPIT_CARDS_SEL";

            List<SqlParameter> parametros = new List<SqlParameter>();
            parametros.Add(new SqlParameter("@dDataInicio", objRequest.periodoInicio));
            parametros.Add(new SqlParameter("@dDataFinal", objRequest.periodoFim));

            using (var reader = Conn.ExecutaComParametros(NomeProcedure, parametros))
            {
                while (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        CockpitResponseDto.Card card = new CockpitResponseDto.Card
                        {
                            Tipo = reader["Status"].ToString().Trim().ToUpper(),
                            Valor = reader["Total"].ToString().Trim()
                        };

                        objResponse.card.Add(card);
                    }
                    reader.NextResult();
                }
            }

            Conn.Dispose();

            return objResponse;
        }

        public static CockpitResponseDto CONSULTA_WEBSOCKET_LOGS(string Connection, CockpitRequestDto objRequest, CockpitResponseDto objResponse)
        {
            // Faz a consutla dos dados da Solicitação de Pagamento
            ConexaoSQLServer Conn = new ConexaoSQLServer(Connection);

            string NomeProcedure = "SP_WEBSOCKETS_COCKPIT_LOGS_SEL";

            List<SqlParameter> parametros = new List<SqlParameter>();
            parametros.Add(new SqlParameter("@dDataInicio", objRequest.periodoInicio));
            parametros.Add(new SqlParameter("@dDataFinal", objRequest.periodoFim));
            parametros.Add(new SqlParameter("@sConteudoLog", objRequest.Search.Value));
            parametros.Add(new SqlParameter("@iPagina", objRequest.Start));
            parametros.Add(new SqlParameter("@iTamanhoPagina", objRequest.Length));

            using (var reader = Conn.ExecutaComParametros(NomeProcedure, parametros))
            {
                while (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        // Atualiza o Total de Pagina
                        objResponse.RecordsTotal = int.Parse(reader["total_registros"].ToString().Trim());
                        objResponse.RecordsFiltered = objResponse.RecordsTotal;

                        CockpitResponseDto.LogsWebsocket log = new CockpitResponseDto.LogsWebsocket
                        {
                            Log = reader["LOG"].ToString().Trim().ToUpper(),
                            DataHorario = reader["DATAHORARIO_CADASTRO"].ToString().Trim()
                        };

                        objResponse.logsWebsocket.Add(log);
                    }
                    reader.NextResult();
                }
            }

            Conn.Dispose();

            return objResponse;
        }

        public static CockpitResponseDto CONSULTA_EMAR_LOGS(string Connection, CockpitRequestDto objRequest, CockpitResponseDto objResponse)
        {
            // Faz a consutla dos dados da Solicitação de Pagamento
            ConexaoSQLServer Conn = new ConexaoSQLServer(Connection);

            string NomeProcedure = "SP_EMAR_COCKPIT_LOG_SEL";

            List<SqlParameter> parametros = new List<SqlParameter>();
            parametros.Add(new SqlParameter("@dDataInicio", objRequest.periodoInicio));
            parametros.Add(new SqlParameter("@dDataFinal", objRequest.periodoFim));
            parametros.Add(new SqlParameter("@sConteudoLog", objRequest.Search.Value));
            parametros.Add(new SqlParameter("@iPagina", objRequest.Start));
            parametros.Add(new SqlParameter("@iTamanhoPagina", objRequest.Length));

            using (var reader = Conn.ExecutaComParametros(NomeProcedure, parametros))
            {
                while (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        // Atualiza o Total de Pagina
                        objResponse.RecordsTotal = int.Parse(reader["total_registros"].ToString().Trim());
                        objResponse.RecordsFiltered = objResponse.RecordsTotal;

                        CockpitResponseDto.LogsEmar log = new CockpitResponseDto.LogsEmar
                        {
                            Log = reader["LOG"].ToString().Trim().ToUpper(),
                            DataHorario = reader["DATAHORARIO_LOG"].ToString().Trim()
                        };

                        objResponse.logsEmar.Add(log);
                    }
                    reader.NextResult();
                }
            }

            Conn.Dispose();

            return objResponse;
        }

        public static CockpitResponseDto CONSULTA_EMAR_LOGS_PROCESSAMENTO(string Connection, CockpitRequestDto objRequest, CockpitResponseDto objResponse)
        {
            // Faz a consutla dos dados da Solicitação de Pagamento
            ConexaoSQLServer Conn = new ConexaoSQLServer(Connection);

            string NomeProcedure = "SP_EMAR_COCKPIT_LOGS_PROCESSAMENTO_SEL";

            List<SqlParameter> parametros = new List<SqlParameter>();
            parametros.Add(new SqlParameter("@dDataInicio", objRequest.periodoInicio));
            parametros.Add(new SqlParameter("@dDataFinal", objRequest.periodoFim));
            parametros.Add(new SqlParameter("@sConteudoLog", objRequest.Search.Value));
            parametros.Add(new SqlParameter("@iPagina", objRequest.Start));
            parametros.Add(new SqlParameter("@iTamanhoPagina", objRequest.Length));

            using (var reader = Conn.ExecutaComParametros(NomeProcedure, parametros))
            {
                while (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        // Atualiza o Total de Pagina
                        objResponse.RecordsTotal = int.Parse(reader["total_registros"].ToString().Trim());
                        objResponse.RecordsFiltered = objResponse.RecordsTotal;

                        CockpitResponseDto.LogsEmarProcessamento log = new CockpitResponseDto.LogsEmarProcessamento
                        {
                            Email = reader["EMAIL"].ToString().Trim(),
                            Anexo = reader["ANEXO"].ToString().Trim(),
                            Status = reader["STATUS"].ToString().Trim().ToUpper(),
                            Log = reader["DESCRICAO_LOG"].ToString().Trim().ToUpper(),
                            DataHorario = reader["DATAHORARIO_PROCESSAMENTO"].ToString().Trim()
                        };

                        objResponse.logsEmarProcessamento.Add(log);
                    }
                    reader.NextResult();
                }
            }

            Conn.Dispose();

            return objResponse;
        }
    }
}
