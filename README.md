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
- **.NET 8 SDK** (instalado e configurado)
- **PostgreSQL 18** rodando localmente na **porta 5433**
  - Instale via instalador oficial ou Chocolatey
  - Crie a database `tccconcursos` com owner `postgres`
  - Usuário: `postgres` (senha padrão ou configurada)

### Configuração do banco de dados
1. Certifique-se de que o PostgreSQL 18 está rodando na porta 5433.
2. No pgAdmin ou psql, conecte como `postgres` e crie a database:
```sql
   CREATE DATABASE tccconcursos OWNER = postgres ENCODING = 'UTF8';
