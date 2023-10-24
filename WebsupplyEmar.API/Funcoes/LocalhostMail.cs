using System.Net.Mail;
using System.Net;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MimeKit;
using System.Text.RegularExpressions;

namespace WebsupplyEmar.API.Funcoes
{
    public class LocalhostMail
    {
        private readonly IConfiguration _configuration;

        public LocalhostMail(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public object receberEmailImap(bool habilitarSSL)
        {
            using (var client = new ImapClient())
            {
                if (habilitarSSL)
                {
                    client.Connect(
                        _configuration.GetValue<string>("Email:locahostMail:imap:host"),
                        _configuration.GetValue<int>("Email:locahostMail:imap:portSSL"),
                        habilitarSSL);
                }
                else
                {
                    client.Connect(
                        _configuration.GetValue<string>("Email:locahostMail:imap:host"),
                        _configuration.GetValue<int>("Email:locahostMail:imap:port"),
                        habilitarSSL);
                }

                client.Authenticate(
                    _configuration.GetValue<string>("Email:locahostMail:imap:user"),
                    _configuration.GetValue<string>("Email:locahostMail:imap:pass"));

                var inbox = client.Inbox;
                inbox.Open(MailKit.FolderAccess.ReadOnly);

                var mensagens = new Dictionary<int, object>();

                for (int i = 0; i < inbox.Count; i++)
                {
                    var mensagem = inbox.GetMessage(i);

                    List<object> mensagemAnexos = new List<object>();

                    for (int j = 0; j < mensagem.Attachments.ToList().Count(); j++)
                    {
                        var anexo = mensagem.Attachments.ToList()[j];

                        var fileName = anexo.ContentDisposition?.FileName ?? "unknown";
                        var filePath = Path.Combine("C:\\uploads", fileName);

                        using (var stream = File.Create(filePath))
                        {
                            if (anexo is MimePart mimePart)
                            {
                                mimePart.Content.DecodeTo(stream);
                            }
                        }

                        mensagemAnexos.Add(new
                        {
                            fileName = fileName,
                            filePath = filePath
                        });
                    }

                    object objClaims = new object();

                    if (mensagem.Subject.IndexOf("[Processamento de Anexo]") > -1)
                    {
                        string pattern = @"\[(.*?)\]";
                        Regex regex = new Regex(pattern);
                        Match match = regex.Match(mensagem.TextBody);
                        if (match.Success)
                        {
                            string token = match.Groups[1].Value;
                            objClaims = GeradorClaimsJWT.CarregaToken(token);
                        }
                    }

                    object objMensagem = new
                    {
                        MessageID = String.IsNullOrEmpty(mensagem.MessageId.ToString()) ? "" : mensagem.MessageId.ToString(),
                        To = String.IsNullOrEmpty(mensagem.To.ToString()) ? "" : mensagem.To.ToString(),
                        From = String.IsNullOrEmpty(mensagem.From.ToString()) ? "" : mensagem.From.ToString(),
                        Bcc = String.IsNullOrEmpty(mensagem.Bcc.ToString()) ? "" : mensagem.Bcc.ToString(),
                        Cc = String.IsNullOrEmpty(mensagem.Cc.ToString()) ? "" : mensagem.Cc.ToString(),
                        Body = String.IsNullOrEmpty(mensagem.Body.ToString()) ? "" : mensagem.Body.ToString(),
                        Date = String.IsNullOrEmpty(mensagem.Date.ToString()) ? "" : mensagem.Date.ToString(),
                        HtmlBody = String.IsNullOrEmpty(mensagem.HtmlBody.ToString()) ? "" : mensagem.HtmlBody.ToString(),
                        Importance = String.IsNullOrEmpty(mensagem.Importance.ToString()) ? "" : mensagem.Importance.ToString(),
                        MimeVersion = String.IsNullOrEmpty(mensagem.MimeVersion.ToString()) ? "" : mensagem.MimeVersion.ToString(),
                        Priority = String.IsNullOrEmpty(mensagem.Priority.ToString()) ? "" : mensagem.Priority.ToString(),
                        References = String.IsNullOrEmpty(mensagem.References.ToString()) ? "" : mensagem.References.ToString(),
                        ReplyTo = String.IsNullOrEmpty(mensagem.ReplyTo.ToString()) ? "" : mensagem.ReplyTo.ToString(),
                        //ResentBcc = String.IsNullOrEmpty(mensagem.ResentBcc.ToString()) ? "" : mensagem.ResentBcc.ToString(),
                        //ResentCc = String.IsNullOrEmpty(mensagem.ResentCc.ToString()) ? "" : mensagem.ResentCc.ToString(),
                        //ResentDate = String.IsNullOrEmpty(mensagem.ResentDate.ToString()) ? "" : mensagem.ResentDate.ToString(),
                        //ResentFrom = String.IsNullOrEmpty(mensagem.ResentFrom.ToString()) ? "" : mensagem.ResentFrom.ToString(),
                        //ResentMessageId = String.IsNullOrEmpty(mensagem.ResentMessageId.ToString()) ? "" : mensagem.ResentMessageId.ToString(),
                        //ResentReplyTo = String.IsNullOrEmpty(mensagem.ResentReplyTo.ToString()) ? "" : mensagem.ResentReplyTo.ToString(),
                        //ResentSender = String.IsNullOrEmpty(mensagem.ResentSender.ToString()) ? "" : mensagem.ResentSender.ToString(),
                        //ResentTo = String.IsNullOrEmpty(mensagem.ResentTo.ToString()) ? "" : mensagem.ResentTo.ToString(),
                        //Sender = String.IsNullOrEmpty(mensagem.Sender.ToString()) ? "" : mensagem.Sender.ToString(),
                        Subject = String.IsNullOrEmpty(mensagem.Subject.ToString()) ? "" : mensagem.Subject.ToString(),
                        TextBody = String.IsNullOrEmpty(mensagem.TextBody.ToString()) ? "" : mensagem.TextBody.ToString(),
                        XPriority = String.IsNullOrEmpty(mensagem.XPriority.ToString()) ? "" : mensagem.XPriority.ToString(),
                        Attachments = mensagemAnexos,
                        Claims = objClaims
                    };

                    mensagens.Add(i, objMensagem);
                }

                return mensagens;
            }
        }

        public object receberEmailPop(bool habilitarSSL)
        {
            using (var client = new Pop3Client())
            {
                if (habilitarSSL)
                {
                    client.Connect(
                        _configuration.GetValue<string>("Email:locahostMail:pop3:host"),
                        _configuration.GetValue<int>("Email:locahostMail:pop3:portSSL"),
                        habilitarSSL);
                }
                else
                {
                    client.Connect(
                        _configuration.GetValue<string>("Email:locahostMail:pop3:host"),
                        _configuration.GetValue<int>("Email:locahostMail:pop3:port"),
                        habilitarSSL);
                }

                client.Authenticate(
                    _configuration.GetValue<string>("Email:locahostMail:pop3:user"),
                    _configuration.GetValue<string>("Email:locahostMail:pop3:pass"));

                var mensagens = new Dictionary<int, object>();

                for (int i = 0; i < client.Count; i++)
                {
                    var mensagem = client.GetMessage(i);
                    
                    List<object> mensagemAnexos = new List<object>();

                    for(int j = 0; j < mensagem.Attachments.ToList().Count(); j++)
                    {
                        var anexo = mensagem.Attachments.ToList()[j];

                        var fileName = anexo.ContentDisposition?.FileName ?? "unknown";
                        var filePath = Path.Combine("C:\\uploads", fileName);

                        using (var stream = File.Create(filePath))
                        {
                            if (anexo is MimePart mimePart)
                            {
                                mimePart.Content.DecodeTo(stream);
                            }
                        }

                        mensagemAnexos.Add(new {
                            fileName = fileName,
                            filePath = filePath
                        });
                    }

                    object objMensagem = new
                    {
                        MessageID = String.IsNullOrEmpty(mensagem.MessageId.ToString()) ? "" : mensagem.MessageId.ToString(),
                        To = String.IsNullOrEmpty(mensagem.To.ToString()) ? "" : mensagem.To.ToString(),
                        From = String.IsNullOrEmpty(mensagem.From.ToString()) ? "" : mensagem.From.ToString(),
                        Bcc = String.IsNullOrEmpty(mensagem.Bcc.ToString()) ? "" : mensagem.Bcc.ToString(),
                        Cc = String.IsNullOrEmpty(mensagem.Cc.ToString()) ? "" : mensagem.Cc.ToString(),
                        Body = String.IsNullOrEmpty(mensagem.Body.ToString()) ? "" : mensagem.Body.ToString(),
                        Date = String.IsNullOrEmpty(mensagem.Date.ToString()) ? "" : mensagem.Date.ToString(),
                        HtmlBody = String.IsNullOrEmpty(mensagem.HtmlBody.ToString()) ? "" : mensagem.HtmlBody.ToString(),
                        Importance = String.IsNullOrEmpty(mensagem.Importance.ToString()) ? "" : mensagem.Importance.ToString(),
                        MimeVersion = String.IsNullOrEmpty(mensagem.MimeVersion.ToString()) ? "" : mensagem.MimeVersion.ToString(),
                        Priority = String.IsNullOrEmpty(mensagem.Priority.ToString()) ? "" : mensagem.Priority.ToString(),
                        References = String.IsNullOrEmpty(mensagem.References.ToString()) ? "" : mensagem.References.ToString(),
                        ReplyTo = String.IsNullOrEmpty(mensagem.ReplyTo.ToString()) ? "" : mensagem.ReplyTo.ToString(),
                        //ResentBcc = String.IsNullOrEmpty(mensagem.ResentBcc.ToString()) ? "" : mensagem.ResentBcc.ToString(),
                        //ResentCc = String.IsNullOrEmpty(mensagem.ResentCc.ToString()) ? "" : mensagem.ResentCc.ToString(),
                        //ResentDate = String.IsNullOrEmpty(mensagem.ResentDate.ToString()) ? "" : mensagem.ResentDate.ToString(),
                        //ResentFrom = String.IsNullOrEmpty(mensagem.ResentFrom.ToString()) ? "" : mensagem.ResentFrom.ToString(),
                        //ResentMessageId = String.IsNullOrEmpty(mensagem.ResentMessageId.ToString()) ? "" : mensagem.ResentMessageId.ToString(),
                        //ResentReplyTo = String.IsNullOrEmpty(mensagem.ResentReplyTo.ToString()) ? "" : mensagem.ResentReplyTo.ToString(),
                        //ResentSender = String.IsNullOrEmpty(mensagem.ResentSender.ToString()) ? "" : mensagem.ResentSender.ToString(),
                        //ResentTo = String.IsNullOrEmpty(mensagem.ResentTo.ToString()) ? "" : mensagem.ResentTo.ToString(),
                        //Sender = String.IsNullOrEmpty(mensagem.Sender.ToString()) ? "" : mensagem.Sender.ToString(),
                        Subject = String.IsNullOrEmpty(mensagem.Subject.ToString()) ? "" : mensagem.Subject.ToString(),
                        TextBody = String.IsNullOrEmpty(mensagem.TextBody.ToString()) ? "" : mensagem.TextBody.ToString(),
                        XPriority = String.IsNullOrEmpty(mensagem.XPriority.ToString()) ? "" : mensagem.XPriority.ToString(),
                        Attachments = mensagemAnexos
                    };

                    mensagens.Add(i, objMensagem);
                }

                return mensagens;
            }
        }

        public bool enviarEmailSmtp(string strEmail, string strTitulo, string strMensagem)
        {
            var smtpClient = new SmtpClient(_configuration.GetValue<string>("Email:locahostMail:smtp:host"))
            {
                Port = _configuration.GetValue<int>("Email:locahostMail:smtp:portSSL"),
                Credentials = new NetworkCredential(
                        _configuration.GetValue<string>("Email:locahostMail:smtp:user"),
                        _configuration.GetValue<string>("Email:locahostMail:smtp:pass")
                    ),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration.GetValue<string>("Email:locahostMail:smtp:user").ToString()),
                Subject = strTitulo,
                Body = strMensagem,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(strEmail);

            try
            {
                smtpClient.Send(mailMessage);
                return true;
            }
            catch (ArgumentException e)
            {
                return false;
            }

        }
    }
}
