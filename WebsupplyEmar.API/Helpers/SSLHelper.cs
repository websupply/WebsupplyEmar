using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace WebsupplyEmar.API.Helpers
{
    public class SSLHelper
    {
        // Consulta a Hash do certificado SSL
        public static X509Certificate2 ConsultaHashCertificado(string hostname)
        {
            try
            {
                using (var client = new TcpClient(hostname, 443))
                using (var sslStream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidaCertificadoServidor)))
                {
                    sslStream.AuthenticateAsClient(hostname);
                    X509Certificate cert = sslStream.RemoteCertificate;

                    if (cert != null)
                    {
                        return new X509Certificate2(cert);
                    }
                    else
                    {
                        Console.WriteLine($"Não foi possível obter o certificado de {hostname}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter o certificado de {hostname}: {ex.Message}");
                return null;
            }
        }

        // Função para Comparar os Hashs SSL
        public static bool ComparaHashSSL(byte[] hash1, byte[] hash2)
        {
            if (hash1.Length != hash2.Length)
                return false;

            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i])
                    return false;
            }

            return true;
        }

        // Função para Validar o Certificado do Servidor
        public static bool ValidaCertificadoServidor(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Aceitar qualquer certificado (você pode ajustar conforme suas necessidades)
        }
    }
}
