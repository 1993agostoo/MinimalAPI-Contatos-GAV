namespace ContatosGAV.Models
{
    public class Contato
    {


        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Numero { get; set; }

        public Contato(Guid id, string? nome, string? email, string? numero)
        {
            Id = id;
            Nome = nome;
            Email = email;
            Numero = numero;
        }
    }
}
