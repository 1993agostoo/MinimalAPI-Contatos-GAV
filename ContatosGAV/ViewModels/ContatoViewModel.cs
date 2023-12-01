using ContatosGAV.Models;
using Flunt.Notifications;
using Flunt.Validations;

namespace ContatosGAV.ViewModels
{
    public class ContatoViewModel : Notifiable<Notification>
    {
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Numero { get; set; }

        public Contato Mapto()
        {
            var validar = new Contract<Notification>()
                .Requires()
                .IsNotNullOrWhiteSpace(Nome, "Obrigatório informar um nome.")
                .IsNotNullOrWhiteSpace(Email, "Obrigatório informar um e-mail.")
                .IsNotNullOrWhiteSpace(Numero, "Obrigatório informar um número.")
                .IsGreaterThan(Nome, 2, "O nome deve ter mais de 2 caracteres.");

            AddNotifications(validar);

            return new Contato(Guid.NewGuid(), Nome, Email, Numero);
        }
    }
}
