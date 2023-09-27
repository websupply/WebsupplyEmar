namespace WebsupplyEmar.API.Helpers
{
    public class FileHelper
    {
        public static string ObterNomeUnico(string diretorio, string nomeOriginal)
        {
            string nomeBase = Path.GetFileNameWithoutExtension(nomeOriginal);
            string extensao = Path.GetExtension(nomeOriginal);
            string nomeUnico = nomeOriginal;
            int contador = 1;

            while (File.Exists(Path.Combine(diretorio, nomeUnico)))
            {
                nomeUnico = $"{nomeBase} (cópia {contador}){extensao}";
                contador++;
            }

            return nomeUnico;
        }
    }
}
