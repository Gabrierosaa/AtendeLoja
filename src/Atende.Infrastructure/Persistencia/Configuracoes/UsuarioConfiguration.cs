using Atende.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atende.Infrastructure.Persistencia.Configuracoes
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuario");

            builder.HasKey(u => u.id);
            builder.Property(u => u.id).ValueGeneratedNever();

            builder.Property(u => u.LojaId).IsRequired();
            builder.Property(u => u.email).IsRequired().HasMaxLength(200);
            builder.Property(u => u.SenhaHash).IsRequired();
            builder.Property(u => u.nome).IsRequired().HasMaxLength(150);
            builder.Property(u => u.CriadoEm).IsRequired();
            builder.Property(u => u.Ativo).IsRequired();

            builder.HasOne<Loja>()
                   .WithMany()
                   .HasForeignKey(u => u.LojaId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(u => u.email).IsUnique().HasDatabaseName("UQ_Usuario_Email");
        }
    }
}
