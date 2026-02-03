using TccConcursos.Domain.Enums;

namespace TccConcursos.Domain.Entities
{
    public class SessaoEstudo
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TopicoId { get; set; }
        public Topico? Topico { get; set; }

        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }

        public TipoSessaoEstudo Tipo { get; set; }

        public int? QuestoesTotal { get; set; }
        public int? QuestoesAcertos { get; set; }
    }
}
