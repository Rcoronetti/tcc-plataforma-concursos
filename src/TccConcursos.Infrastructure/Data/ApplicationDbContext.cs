using Microsoft.EntityFrameworkCore;
using TccConcursos.Domain.Entities;

namespace TccConcursos.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Concurso> Concursos => Set<Concurso>();
        public DbSet<Disciplina> Disciplinas => Set<Disciplina>();
        public DbSet<Topico> Topicos => Set<Topico>();
        public DbSet<SessaoEstudo> SessoesEstudo => Set<SessaoEstudo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Concurso
            modelBuilder.Entity<Concurso>(entity =>
            {
                entity.ToTable("concursos");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Nome)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.DataProva);

                entity.HasMany(x => x.Disciplinas)
                    .WithOne(x => x.Concurso!)
                    .HasForeignKey(x => x.ConcursoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Disciplina
            modelBuilder.Entity<Disciplina>(entity =>
            {
                entity.ToTable("disciplinas");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Nome)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasMany(x => x.Topicos)
                    .WithOne(x => x.Disciplina!)
                    .HasForeignKey(x => x.DisciplinaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Topico
            modelBuilder.Entity<Topico>(entity =>
            {
                entity.ToTable("topicos");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Nome)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasMany(x => x.Sessoes)
                    .WithOne(x => x.Topico!)
                    .HasForeignKey(x => x.TopicoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // SessaoEstudo
            modelBuilder.Entity<SessaoEstudo>(entity =>
            {
                entity.ToTable("sessoes_estudo");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Inicio)
                    .IsRequired();

                entity.Property(x => x.Fim)
                    .IsRequired();

                entity.Property(x => x.Tipo)
                    .IsRequired()
                    .HasConversion<int>();

                entity.Property(x => x.QuestoesTotal);
                entity.Property(x => x.QuestoesAcertos);

                // Índice útil para métricas por tópico e período
                entity.HasIndex(x => new { x.TopicoId, x.Inicio });
            });
        }
    }
}