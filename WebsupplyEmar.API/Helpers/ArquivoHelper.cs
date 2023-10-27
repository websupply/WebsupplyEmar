using Microsoft.Graph.Models;

namespace WebsupplyEmar.API.Helpers
{
    public class ArquivoHelper
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

        public static int ContabilizaAnexosValidos(List<Attachment> Anexos)
        {
            int arquivosValidos = 0;

            for(int i = 0;i < Anexos.Count(); i++)
            {
                FileAttachment Anexo = (FileAttachment)Anexos[i];

                if(!(bool)Anexo.IsInline && Anexo.ContentId == null)
                {
                    arquivosValidos++;
                }
            }

            return arquivosValidos;
        }
    }
}
