# tcc-plataforma-concursos

## Plataforma Web para Planejamento e Monitoramento de Estudos para Concursos

Aplicação web desenvolvida como Trabalho de Conclusão de Curso (**Pós-graduação em Desenvolvimento Full Stack – PUCRS**). O sistema é voltado a **concurseiros** e permite **organizar concursos, disciplinas e tópicos de estudo**, **registrar sessões** e **acompanhar métricas de desempenho**.

---

## Tecnologias
- **C# / .NET 8**
- **Blazor Server** (UI com **MudBlazor**)
- **ASP.NET Core Minimal API**
- **PostgreSQL**
- **Entity Framework Core 8 + Npgsql** (ORM e migrations)

---

## Status
**Em desenvolvimento — base funcional do MVP implementada**

---

## Escopo atual (implementado)
- Autenticação de usuário com:
  - Cadastro (`/usuarios/cadastro`)
  - Login (`/usuarios/login`)
  - Edição de perfil (`/usuarios/{id}/perfil`)
  - Alteração de senha (`/usuarios/{id}/senha`)
- CRUD completo de concursos (`/concursos`)
- CRUD completo de disciplinas por concurso (`/concursos/{concursoId}/disciplinas`)
- CRUD completo de tópicos por disciplina (`/disciplinas/{disciplinaId}/topicos`)
- CRUD completo de sessões de estudo por tópico (`/topicos/{topicoId}/sessoes`)
  - Tipos de sessão: teoria, revisão e questões
  - Regras de validação de duração e consistência de questões/acertos
- Dashboard com agregações:
  - Resumo geral (`/dashboard/resumo`)
  - Métricas por disciplina (`/dashboard/por-disciplina`)
  - Métricas por concurso (`/dashboard/por-concurso`)
  - Métricas por tópico (`/dashboard/por-topico`)
  - Variações por concurso específico

---

## Regras de negócio (resumo)
- Cada usuário pode possuir **vários concursos**
- Identificadores principais são **UUID (Guid)**
- Não é permitido:
  - Criar disciplina duplicada no mesmo concurso
  - Criar tópico duplicado na mesma disciplina
- Sessões com tipo **Questões** exigem quantidade total e acertos válidos

---

## Como executar

Esta solução contém os seguintes projetos:
+ **`src/TccConcursos.Api`**: API back-end (endpoints e regras de negócio).
+ **`TccConcursos.Blazor.Server`**: aplicação web principal (front-end).
+ **`src/TccConcursos.Domain`**: entidades e enums de domínio.
+ **`src/TccConcursos.Infrastructure`**: `DbContext` e migrations.
+ **`src/TccConcursos.Web`**: projeto web base (template), não utilizado no fluxo principal atual.

### Pré-requisitos
- **.NET 8 SDK**
- **PostgreSQL** rodando localmente
- Database **`tccconcursos`** criada

---

### 1. Criar a database (se ainda não existir)
Conecte como `postgres` (via pgAdmin ou psql) e execute:
```sql
CREATE DATABASE tccconcursos OWNER = postgres ENCODING = 'UTF8';
```

### 2. Configurar as aplicações
#### 2.1. Configurar a API (Connection String)
Para não versionar senhas no Git, a connection string deve ficar em **User Secrets**.

No Visual Studio: clique com o botão direito no projeto **TccConcursos.Api** → *Manage User Secrets*.
Adicione o seguinte conteúdo ao arquivo `secrets.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=127.0.0.1;Port=5432;Database=tccconcursos;Username=postgres;Password=SUA_SENHA_AQUI"
  }
}
```

> Observação: se o seu PostgreSQL usar outra porta (ex.: `5433`), ajuste a connection string.

#### 2.2. Configurar o Front-end (URL da API)
O projeto Blazor precisa saber onde a API está rodando. Abra o arquivo `appsettings.json` no projeto **TccConcursos.Blazor.Server** e ajuste a URL:

```json
{
  "ApiBaseUrl": "https://localhost:7043/"
}
```

### 3. Aplicar Migrations no Banco
No Visual Studio, abra o **Package Manager Console**
(*Tools > NuGet Package Manager > Package Manager Console*) e execute:

```powershell
Update-Database -Project TccConcursos.Infrastructure -StartupProject TccConcursos.Api
```

As migrations ficam em `src/TccConcursos.Infrastructure/Data/Migrations`.

### 4. Executar a Solução (API + Front-end)
No Visual Studio, configure a solução para iniciar os dois projetos:
+ Clique com o botão direito na **Solution** no Solution Explorer.
+ Selecione `Set Startup Projects....`
+ Escolha a opção `Multiple startup projects`.
+ Para **TccConcursos.Api** e **TccConcursos.Blazor.Server**, mude a "Action" para `Start`.
+ Clique em `OK`.

Pressione **F5** para executar.
- API (Swagger): `https://localhost:7043/swagger`
- Front-end Blazor: `https://localhost:7098`

## Notas de Desenvolvimento

### Banco e migrations (estado atual)
- O projeto já possui migrations para:
  - Estrutura inicial
  - Tabelas de domínio (concursos, disciplinas, tópicos, sessões)
  - Índices únicos para evitar duplicidade de disciplina/tópico
  - Tabela de usuários

### Front-end (estado atual)
- Navegação protegida com autenticação na interface.
- Telas implementadas para:
  - Login e cadastro
  - Configuração de usuário
  - Dashboard
  - Concursos, disciplinas, tópicos e sessões
- Integração com API via `HttpClient` no serviço `ConcursosApi`.

### Criar uma nova migration
Após alterar entidades no `DbContext`, use:

```powershell
Add-Migration NOME_DA_MIGRATION -Project TccConcursos.Infrastructure -StartupProject TccConcursos.Api -OutputDir Data/Migrations
```

### Para atualizar o banco para a última versão
```powershell
Update-Database -Project TccConcursos.Infrastructure -StartupProject TccConcursos.Api
```
