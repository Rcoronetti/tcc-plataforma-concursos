namespace TccConcursos.Domain.Entities
{
    public class Topico
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DisciplinaId { get; set; }
        public Disciplina? Disciplina { get; set; }

        public string Nome { get; set; } = string.Empty;

        public ICollection<SessaoEstudo> Sessoes { get; set; } = new List<SessaoEstudo>();
    }
}
