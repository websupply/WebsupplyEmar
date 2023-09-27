using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Net.Http.Headers;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions;

namespace WebsupplyEmar.API.Funcoes
{
    public class GraphAzure
    {
        public async Task<string> GERAR_ACCESSTOKEN(string ClientID, string ClientSecret, string TenantID)
        {
            // Declara as Variaveis
            string clientId = ClientID;
            string clientSecret = ClientSecret;
            string tenantId = TenantID;
            string tokenEndPoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            // Cria a Requisição do httpclient
            using (HttpClient client = new HttpClient())
            {
                // Define a request
                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default")
                });

                // Envia a requisição
                HttpResponseMessage response = await client.PostAsync(tokenEndPoint, requestContent);

                // Recebe e serializa a Resposta
                string responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                // Recebe o accessToken
                string accessToken = tokenResponse.GetProperty("access_token").GetString();

                return accessToken;
            }
        }

        public async Task<List<object>> CARREGAR_EMAILS(string userMailId, string accessToken)
        {

            // Declara as Variaveis
            string graphEndPoint = $"https://graph.microsoft.com/v1.0/users/{userMailId}/mailFolders/inbox/messages";

            // Cria a Requisição do httpclient
            using (HttpClient client = new HttpClient())
            {
                // Seta a request
                var request = new HttpRequestMessage(HttpMethod.Get, graphEndPoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Envia a requisição
                HttpResponseMessage response = await client.SendAsync(request);

                // Recebe e serializa a Resposta
                string responseContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                // Cria a lista de emails
                List<object> emails = new List<object>();

                // Enumera os emails do json
                var arrayEmails = jsonResponse.GetProperty("values").EnumerateArray();

                // Adiciona os emails a lista
                if(arrayEmails.Count() > 0)
                {
                    foreach(var Email in arrayEmails)
                    {
                        emails.Add(Email);
                    }
                }

                return emails;
            }
        }

        public GraphServiceClient CRIA_AMBIENTE_GRAPH(dynamic ambientOptions)
        {
            if (ambientOptions.AmbientType == "UserAndPass")
            {
                // Cria Ambiente baseado no usuário e senha
                var scopes = new[] { "https://graph.microsoft.com/.default" };

                var options = new UsernamePasswordCredentialOptions
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                };

                var userNamePasswordCredential = new UsernamePasswordCredential(
                    ambientOptions.AuthUser, ambientOptions.AuthPass, ambientOptions.TenantID, ambientOptions.ClientID, options);

                return new GraphServiceClient(userNamePasswordCredential, scopes);
            }
            else
            {
                // Cria Ambiente baseado no ClientID and ClientSecret
                var scopes = new[] { "https://graph.microsoft.com/.default" };

                var options = new ClientSecretCredentialOptions
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                };

                var clientSecretCredential = new ClientSecretCredential(
                    ambientOptions.TenantID, ambientOptions.ClientID, ambientOptions.ClientSecret, options);

                return new GraphServiceClient(clientSecretCredential, scopes);
            }
        }
    }
}
