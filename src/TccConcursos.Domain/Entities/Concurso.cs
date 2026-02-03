namespace TccConcursos.Domain.Entities
{
    public class Concurso
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Nome { get; set; } = string.Empty;

        public DateOnly? DataProva { get; set; }

        public ICollection<Disciplina> Disciplinas { get; set; } = new List<Disciplina>();
    }
}
