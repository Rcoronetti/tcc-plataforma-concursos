namespace TccConcursos.Blazor.Server.Services;

public class NavigationContext
{
    public event Action? OnChange;

    private Guid? _concursoId;
    public Guid? ConcursoId { get => _concursoId; set { _concursoId = value; Notify(); } }

    private string? _concursoNome;
    public string? ConcursoNome { get => _concursoNome; set { _concursoNome = value; Notify(); } }

    private Guid? _disciplinaId;
    public Guid? DisciplinaId { get => _disciplinaId; set { _disciplinaId = value; Notify(); } }

    private string? _disciplinaNome;
    public string? DisciplinaNome { get => _disciplinaNome; set { _disciplinaNome = value; Notify(); } }

    private Guid? _topicoId;
    public Guid? TopicoId { get => _topicoId; set { _topicoId = value; Notify(); } }

    private string? _topicoNome;
    public string? TopicoNome { get => _topicoNome; set { _topicoNome = value; Notify(); } }

    public void ClearBelowConcurso()
    {
        DisciplinaId = null;
        DisciplinaNome = null;
        TopicoId = null;
        TopicoNome = null;
        Notify();
    }

    public void ClearBelowDisciplina()
    {
        TopicoId = null;
        TopicoNome = null;
        Notify();
    }

    private void Notify() => OnChange?.Invoke();
}