using Atende.Domain.Exceptions;

namespace Atende.Domain.Entidades
{
    public class Loja
    {
        public Guid id{ get; set; }
        public String nome { get; set; }
        public String slug { get; set; }
        public String FusoHorario { get; set; }
        public DateTime CriadaEm { get; set; }
        public String MensagemEncaminhamento { get; set; }
        public bool Ativa { get; set; }

        private Loja() { }

        public Loja(string nome, string slug, string fusoHorario, string mensagemEncaminhamento)
        {
            id = Guid.NewGuid();
            this.nome = nome;
            this.slug = slug;
            FusoHorario = fusoHorario;
            CriadaEm = DateTime.Now;
            MensagemEncaminhamento = mensagemEncaminhamento;
            Ativa = true;

            ValidarLoja();
        }

        public void ValidarLoja()
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("O nome da loja é obrigatório.");

            if (string.IsNullOrWhiteSpace(slug))
                throw new DomainException("O slug da loja é obrigatório.");

            if (string.IsNullOrWhiteSpace(FusoHorario))
                throw new DomainException("O fuso horário da loja é obrigatório.");
        }

    }
}
