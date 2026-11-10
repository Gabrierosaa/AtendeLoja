using Atende.Domain.Exceptions;


namespace Atende.Domain.Entidades
{
    public class Usuario
    {
        public Guid id { get; set; }
        public Guid LojaId { get; set; }
        public String email { get; set; }
        public string SenhaHash { get; set; }
        public string nome { get; set; }
        public DateTime CriadoEm { get; set; }
        public bool Ativo { get; set; }

        private Usuario() { }

        public Usuario(Guid lojaId, string email, string senhaHash, string nome)
        {
            id = Guid.NewGuid();
            LojaId = lojaId;
            this.email = email;
            SenhaHash = senhaHash;
            this.nome = nome;
            CriadoEm = DateTime.Now;
            Ativo = true;

            ValidarUsuario();
        }

        public void ValidarUsuario()
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("O email do usuário é obrigatório.");

            if (string.IsNullOrWhiteSpace(SenhaHash))
                throw new DomainException("A senha do usuário é obrigatória.");

            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("O nome do usuário é obrigatório.");
        }

    }
}
