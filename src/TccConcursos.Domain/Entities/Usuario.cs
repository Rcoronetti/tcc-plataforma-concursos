namespace TccConcursos.Domain.Entities
{
    public class Usuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string FotoUrl { get; set; } = string.Empty;
        public DateTime CriadoEmUtc { get; set; } = DateTime.UtcNow;
        public DateTime AtualizadoEmUtc { get; set; } = DateTime.UtcNow;
    }
}
