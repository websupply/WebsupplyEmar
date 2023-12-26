using System.Globalization;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace WebsupplyEmar.API.Helpers
{
    public class HelperValidacao
    {
        private static Regex regex;

        // Função para Validar E-Mail
        public static bool ValidaEmail(string email)
        {
            regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");

            if (regex.Match(email).Success)
            {
                return true;
            }

            return false;
        }

        // Função para Validar Data
        public static bool ValidaData(string data, string formato)
        {
            regex = new Regex(@"\d{4}-\d{2}-\d{2}");

            if (DateTime.TryParseExact(data, formato, new CultureInfo("pt-br"), DateTimeStyles.None, out _) && regex.Match(data).Success)
            {
                return true;
            }

            return false;
        }

        // Função para Validar Arquivos Permitidos
        public static bool ValidaArquivo(string arquivo)
        {
            regex = new Regex(@"\.");

            if (regex.Match(arquivo).Success)
            {
                string[] arquivoSplit = arquivo.Split(".");

                bool extensaoPermitida = arquivoSplit[arquivoSplit.Count() - 1].ToLower() switch
                {
                    "exe" => false,
                    "ex_" => false,
                    "ps1" => false,
                    "bat" => false,
                    "ba_" => false,
                    "cs" => false,
                    "sql" => false,
                    "asp" => false,
                    "aspx" => false,
                    "php" => false,
                    "config" => false,
                    "json" => false,
                    "dll" => false,
                    "dl_" => false,
                    _ => true
                };

                return extensaoPermitida;
            }

            return false;
        }
    }
}
