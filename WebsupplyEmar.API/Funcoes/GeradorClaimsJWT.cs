using Microsoft.IdentityModel.Tokens;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;

namespace WebsupplyEmar.API.Funcoes
{
    public class GeradorClaimsJWT
    {
        // Declara as variaveis
        private dynamic _jwt;

        // Inicia o Construtor
        public GeradorClaimsJWT(string appUrl) {
            _jwt = new {
                ValidAudience = appUrl,
                ValidIssuer = appUrl,
                SecretKey = "GeradorClaimsJWT@Default#2023&brokerslink!GROUP",
                TokenValidityInMinutes = 20,
                RefreshTokenValidityInMinutes = 20
            };
        }

        // Função para Pegar os valores do JWT

        private dynamic ConsultaJWT()
        {
            return _jwt;
        }

        public string CriaClaims(string CDGPED, string CODPROD, string CODITEM, string CGCF)
        {
            dynamic _jwt = ConsultaJWT();

            var tokenClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, CGCF),
                new Claim("CGCF", CGCF),
                new Claim("CDGPED", CDGPED),
                new Claim("CODPROD", CODPROD),
                new Claim("CODITEM", CODITEM),
                new Claim("DT_CRIACAO", DateTime.Now.ToString()),
                new Claim("DT_EXPIRACAO", DateTime.Now.AddMinutes(_jwt.RefreshTokenValidityInMinutes).ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = CriaToken(_jwt, tokenClaims);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private JwtSecurityToken CriaToken(dynamic _jwt, List<Claim> objClaims)
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

        public static object CarregaClaims(string tokenString)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(tokenString);

            dynamic objClaim = new ExpandoObject();

            foreach (Claim claim in token.Claims)
            {
                if (claim.Type == "CGCF")
                    objClaim.CGCF = claim.Value;
                if (claim.Type == "CDGPED")
                    objClaim.CDGPED = claim.Value;
                if (claim.Type == "CODPROD")
                    objClaim.CODPROD = claim.Value;
                if (claim.Type == "CODITEM")
                    objClaim.CODITEM = claim.Value;
                if (claim.Type == "DT_CRIACAO")
                    objClaim.DT_CRIACAO = claim.Value;
                if (claim.Type == "DT_EXPIRACAO")
                    objClaim.DT_EXPIRACAO = claim.Value;
            }

            return objClaim;
        }
    }
}
