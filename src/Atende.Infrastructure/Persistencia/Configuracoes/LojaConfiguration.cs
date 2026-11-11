using Atende.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atende.Infrastructure.Persistencia.Configuracoes
{
    public class LojaConfiguration : IEntityTypeConfiguration<Loja>
    {
        public void Configure(EntityTypeBuilder<Loja> builder)
        {
            builder.ToTable("Loja");

            builder.HasKey(l => l.id);
            builder.Property(l => l.id).ValueGeneratedNever();

            builder.Property(l => l.nome).IsRequired().HasMaxLength(150);
            builder.Property(l => l.slug).IsRequired().HasMaxLength(80);
            builder.Property(l => l.FusoHorario).IsRequired().HasMaxLength(60);
            builder.Property(l => l.MensagemEncaminhamento).IsRequired().HasMaxLength(500);
            builder.Property(l => l.CriadaEm).IsRequired();
            builder.Property(l => l.Ativa).IsRequired();

            builder.HasIndex(l => l.slug).IsUnique().HasDatabaseName("UQ_Loja_Slug");
        }
    }
}
