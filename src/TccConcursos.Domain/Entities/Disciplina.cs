namespace TccConcursos.Domain.Entities
{
    public class Disciplina
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ConcursoId { get; set; }
        public Concurso? Concurso { get; set; }

        public string Nome { get; set; } = string.Empty;

        public ICollection<Topico> Topicos { get; set; } = new List<Topico>();
    }
}
