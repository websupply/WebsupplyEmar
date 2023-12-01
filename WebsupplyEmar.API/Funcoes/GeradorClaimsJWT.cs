using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using WebsupplyEmar.Dominio.Dto;
using WebsupplyEmar.Dominio.Model;

namespace WebsupplyEmar.API.Funcoes
{
    public class GeradorClaimsJWT
    {
        // Declara as variaveis
        private dynamic _jwt;

        // Inicia o Construtor
        public GeradorClaimsJWT(string ValidAudience, string ValidIssuer, string SecretKey, int TokenValidityInMinutes, int RefreshTokenValidityInMinutes) {
            _jwt = new {
                ValidAudience = ValidAudience,
                ValidIssuer = ValidIssuer,
                SecretKey = SecretKey,
                TokenValidityInMinutes = TokenValidityInMinutes,
                RefreshTokenValidityInMinutes = RefreshTokenValidityInMinutes
            };
        }

        // Função para Pegar os valores do JWT

        private dynamic ConsultaJWT()
        {
            return _jwt;
        }

        public string CriaToken(ClaimsModel objClaims)
        {
            dynamic _jwt = ConsultaJWT();

            List<Claim> tokenClaims = new List<Claim>();
            tokenClaims.Add(new Claim(ClaimTypes.Name, objClaims.CGCMatriz));
            if (objClaims.CGCMatriz != null)
            {
                tokenClaims.Add(new Claim("CGCMatriz", objClaims.CGCMatriz));
            }
            if (objClaims.CGC != null)
            {
                tokenClaims.Add(new Claim("CGC", objClaims.CGC));
            }
            if (objClaims.CCUSTO != null)
            {
                tokenClaims.Add(new Claim("CCUSTO", objClaims.CCUSTO));
            }
            if (objClaims.REQUISIT != null)
            {
                tokenClaims.Add(new Claim("REQUISIT", objClaims.REQUISIT));
            }
            if (objClaims.TABELA != null)
            {
                tokenClaims.Add(new Claim("TABELA", objClaims.TABELA));
            }
            if (objClaims.CGCF != null)
            {
                tokenClaims.Add(new Claim("CGCF", objClaims.CGCF));
            }
            if (objClaims.CDGPED != null)
            {
                tokenClaims.Add(new Claim("CDGPED", objClaims.CDGPED));
            }
            if (objClaims.CODPROD != null)
            {
                tokenClaims.Add(new Claim("CODPROD", objClaims.CODPROD));
            }
            if (objClaims.CODITEM != null)
            {
                tokenClaims.Add(new Claim("CODITEM", objClaims.CODITEM));
            }
            if (objClaims.CL_CDG != null)
            {
                tokenClaims.Add(new Claim("CL_CDG", objClaims.CL_CDG));
            }
            if (objClaims.DISPONIVEL_FORNEC != null)
            {
                tokenClaims.Add(new Claim("DISPONIVEL_FORNEC", objClaims.DISPONIVEL_FORNEC));
            }
            if (objClaims.TIPO != null)
            {
                tokenClaims.Add(new Claim("TIPO", objClaims.TIPO));
            }
            tokenClaims.Add(new Claim("DT_CRIACAO", DateTime.Now.ToString()));
            tokenClaims.Add(new Claim("DT_EXPIRACAO", DateTime.Now.AddMinutes(_jwt.RefreshTokenValidityInMinutes).ToString()));
            tokenClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            var token = CriaJWT(_jwt, tokenClaims);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private JwtSecurityToken CriaJWT(dynamic _jwt, List<Claim> objClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                                 .GetBytes(_jwt.SecretKey));

            var token = new JwtSecurityToken(
                issuer: _jwt.ValidIssuer,
                audience: _jwt.ValidAudience,
                expires: DateTime.Now.AddMinutes(_jwt.TokenValidityInMinutes),
                claims: objClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        public static ClaimsModel CarregaToken(string tokenString)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(tokenString);

            ClaimsModel objClaim = new ClaimsModel();

            foreach (Claim claim in token.Claims)
            {
                if (claim.Type == "CGCMatriz")
                    objClaim.CGCMatriz = claim.Value;
                if (claim.Type == "CGC")
                    objClaim.CGC = claim.Value;
                if (claim.Type == "CCUSTO")
                    objClaim.CCUSTO = claim.Value;
                if (claim.Type == "REQUISIT")
                    objClaim.REQUISIT = claim.Value;
                if (claim.Type == "TABELA")
                    objClaim.TABELA = claim.Value;
                if (claim.Type == "CGCF")
                    objClaim.CGCF = claim.Value;
                if (claim.Type == "CDGPED")
                    objClaim.CDGPED = claim.Value;
                if (claim.Type == "CODPROD")
                    objClaim.CODPROD = claim.Value;
                if (claim.Type == "CODITEM")
                    objClaim.CODITEM = claim.Value;
                if (claim.Type == "CL_CDG")
                    objClaim.CL_CDG = claim.Value;
                if (claim.Type == "DISPONIVEL_FORNEC")
                    objClaim.DISPONIVEL_FORNEC = claim.Value;
                if (claim.Type == "TIPO")
                    objClaim.TIPO = claim.Value;
                if (claim.Type == "DT_CRIACAO")
                    objClaim.DT_CRIACAO = DateTime.Parse(claim.Value, CultureInfo.InvariantCulture);
                if (claim.Type == "DT_EXPIRACAO")
                    objClaim.DT_EXPIRACAO = DateTime.Parse(claim.Value, CultureInfo.InvariantCulture);
            }

            return objClaim;
        }

        public static bool ValidaToken(string token, string SecretKey, string ValidIssuer, string ValidAudience)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(SecretKey));
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidIssuer = ValidIssuer,
                ValidAudience = ValidAudience
            };

            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
                return true;
            }
            catch (SecurityTokenException)
            {
                return false;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Erro ao validar o token: {ex.Message}");
                return false;
            }
        }

        public static string ConverteClaimsParaString(ClaimsModel objClaims)
        {
            var result = new StringBuilder();
            var properties = objClaims.GetType().GetProperties();

            foreach (var property in properties)
            {
                var key = property.Name;
                var value = property.GetValue(objClaims)?.ToString();
                result.Append($"[{key} : {value}]");
            }

            return result.ToString();
        }

        public static bool ValidaClaims(ClaimsModel objClaims)
        {
            if(objClaims.CGCMatriz == null) return false;
            if(objClaims.CGC == null) return false;
            if(objClaims.CCUSTO == null) return false;
            if(objClaims.REQUISIT == null) return false;

            if(objClaims.TABELA == "PedidosItens_Temp")
            {
                if(objClaims.CGCF == null) return false;
                if(objClaims.CDGPED == null) return false;
                if(objClaims.CODPROD == null) return false;
                if(objClaims.CODITEM == null) return false;
            }
            else if(objClaims.TABELA == "CL_PROCESSO_ANEXO")
            {
                if(objClaims.CL_CDG == null) return false;
                if(objClaims.DISPONIVEL_FORNEC == null) return false;
                if(objClaims.TIPO == null) return false;
            }
            else
            {
                return false;
            }

            if(objClaims.DT_CRIACAO == null) return false;
            if(objClaims.DT_EXPIRACAO == null) return false;

            return true;
        }
    }
}
