using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("autenticar", (Auth autenticacao, IConfiguration configuration) =>
{
    if (autenticacao.Usuario == "ADM" && autenticacao.Senha == "adminGAV")
        return Token.Create(configuration, "ADM");
    if (autenticacao.Usuario == "USR" && autenticacao.Senha == "userGAV")
        return Token.Create(configuration, "USR");

    return $"O usuário {autenticacao.Usuario} não foi encontrado.";
}).ProducesValidationProblem()
    .Produces<string>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .WithName("Autenticar")
    .WithTags("Auth"); ;


app.UseHttpsRedirection();
app.Run();

record Auth(string Usuario, string Senha);

public static class Token
{
    public static object Create(IConfiguration config, string role)
    {
        var chave = Encoding.ASCII.GetBytes(config["AuthSettings:Key"]);

        var tokenConfig = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.Role, role)
            }),
            Expires = DateTime.UtcNow.AddHours(4),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(chave), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenConfig);
        var tokenString = tokenHandler.WriteToken(token);

        return new
        {
            token = tokenString
        };
    }
}

