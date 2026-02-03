# tcc-plataforma-concursos

## Plataforma Web para Planejamento e Monitoramento de Estudos para Concursos

Aplicação web desenvolvida como Trabalho de Conclusão de Curso (**Pós-graduação em Desenvolvimento Full Stack – PUCRS**). O sistema é voltado a **concurseiros** e permite **organizar concursos, disciplinas e tópicos de estudo**, **registrar sessões** e **acompanhar métricas de desempenho**.

---

## Tecnologias
- **C# / .NET 8**
- **Blazor**
- **ASP.NET Core Web API**
- **PostgreSQL 18**
- **Entity Framework Core 8 + Npgsql** (ORM e migrations)

---

## Status
**Em desenvolvimento — Sprint 0 (Fundação)**

---

## Escopo inicial (MVP)
- Autenticação (usuário)
- Cadastro de concursos, disciplinas e tópicos
- Registro de sessões de estudo (teoria/revisão/questões)
- Registro de questões (quantidade e acertos)
- Dashboard básico (tempo estudado e desempenho por disciplina)

---

## Regras de negócio (resumo)
- Cada usuário pode possuir **vários concursos**
- Identificadores principais serão **UUID (Guid)**

---

## Como executar

### Pré-requisitos
- **.NET 8 SDK**
- **PostgreSQL 18** rodando localmente na **porta 5433**
- A database **`tccconcursos`** criada (com owner `postgres`)

> Observação: Neste ambiente local foi utilizado o PostgreSQL 18 na porta **5433**. Se o seu ambiente usar outra porta, ajuste a `Connection String`.

---

### 1. Criar a database (se ainda não existir)
Conecte como `postgres` (via pgAdmin ou psql) e execute:
```sql
CREATE DATABASE tccconcursos OWNER = postgres ENCODING = 'UTF8';
```

### 2. Configurar a Connection String (User Secrets)
Para não versionar senhas no Git, a connection string deve ficar em **User Secrets**.  

No Visual Studio: clique com o botão direito no projeto **TccConcursos.Api** → *Manage User Secrets*.  
Adicione o seguinte conteúdo ao arquivo `secrets.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=127.0.0.1;Port=5433;Database=tccconcursos;Username=postgres;Password=SUA_SENHA_AQUI"
  }
}
```

### 3. Aplicar Migrations no Banco
No Visual Studio, abra o **Package Manager Console**  
(*Tools > NuGet Package Manager > Package Manager Console*) e execute o comando para criar as tabelas no banco:

```powershell
Update-Database -Project TccConcursos.Infrastructure -StartupProject TccConcursos.Api
```
As migrations ficam em `TccConcursos.Infrastructure/Data/Migrations`,
e a API é usada como StartupProject para carregar a configuração e a injeção de dependência.

### 4. Executar a API
Defina **TccConcursos.Api** como projeto de inicialização.  
Execute com **F5** (ou `dotnet run`).  

O Swagger estará disponível em `/swagger` na URL indicada pelo Visual Studio  
(ex: `https://localhost:5001/swagger`).

## Notas de Desenvolvimento

### Criar uma nova migration
Após alterar entidades no `DbContext`, use o comando:

```powershell
Add-Migration NOME_DA_MIGRATION -Project TccConcursos.Infrastructure -StartupProject TccConcursos.Api -OutputDir Data/Migrations
```

### Para atualizar o banco para a última versão
```powershell
Update-Database -Project TccConcursos.Infrastructure -StartupProject TccConcursos.Api
```


