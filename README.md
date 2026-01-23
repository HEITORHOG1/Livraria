# Sistema de Livraria

Sistema de cadastro de livros desenvolvido em **.NET 9** com **Blazor WebAssembly**, seguindo **Clean Architecture**, **CQRS** e boas práticas de desenvolvimento.

## 🚀 Tecnologias

| Tecnologia | Versão | Finalidade |
|------------|--------|------------|
| .NET | 9.0 | Framework principal |
| Blazor WebAssembly | 9.0 | Frontend SPA |
| ASP.NET Core Web API | 9.0 | Backend REST |
| Entity Framework Core | 9.0 | ORM / Persistência |
| SQL Server | 2022 | Banco de dados |
| MediatR | 12.x | Implementação CQRS |
| FluentValidation | 11.x | Validação de dados |
| QuestPDF | 2024.x | Geração de relatórios PDF |
| xUnit + FsCheck | - | Testes unitários e property-based |
| Docker | - | Containerização |

## 📋 Pré-requisitos

### Para execução com Docker (Recomendado)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

### Para execução sem Docker
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server 2019+](https://www.microsoft.com/sql-server) (LocalDB, Express ou Developer)
- [Git](https://git-scm.com/)

---

## 🐳 Opção 1: Execução com Docker (Recomendado)

A forma mais simples de executar o projeto. Todos os serviços são configurados automaticamente.

### Passo 1: Clonar o repositório

```bash
git clone https://github.com/seu-usuario/livraria.git
cd livraria
```

### Passo 2: Subir os serviços

```bash
docker-compose up -d --build
```

### Passo 3: Aguardar inicialização

O SQL Server leva cerca de 30-60 segundos para iniciar. A API aguarda automaticamente via healthcheck.

```bash
# Verificar status dos containers
docker-compose ps

# Acompanhar logs (opcional)
docker-compose logs -f
```

### Passo 4: Acessar a aplicação

| Serviço | URL |
|---------|-----|
| **Frontend (Blazor)** | http://localhost:5001 |
| **API (Swagger)** | http://localhost:5000/swagger |
| **API via Proxy** | http://localhost:5001/api |

**Credenciais SQL Server:**
- **Servidor:** localhost:1433
- **Usuário:** sa
- **Senha:** Livraria@123

### Comandos Docker úteis

```bash
# Parar todos os serviços
docker-compose down

# Parar e remover volumes (apaga dados do banco)
docker-compose down -v

# Reiniciar um serviço específico
docker-compose restart api

# Ver logs de um serviço
docker-compose logs -f api

# Reconstruir imagens
docker-compose build --no-cache
```

---

## 💻 Opção 2: Execução sem Docker

Para quem prefere executar localmente sem containers.

### Passo 1: Configurar SQL Server

Você precisa de uma instância do SQL Server rodando. Opções:
- **SQL Server LocalDB** (vem com Visual Studio)
- **SQL Server Express** (gratuito)
- **SQL Server Developer** (gratuito para desenvolvimento)

### Passo 2: Configurar Connection String

Edite o arquivo `backend/src/Livraria.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LivrariaDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

**Exemplos de Connection Strings:**

```bash
# LocalDB (Visual Studio)
Server=(localdb)\mssqllocaldb;Database=LivrariaDB;Trusted_Connection=True;TrustServerCertificate=True

# SQL Server Express
Server=.\SQLEXPRESS;Database=LivrariaDB;Trusted_Connection=True;TrustServerCertificate=True

# SQL Server com autenticação SQL
Server=localhost;Database=LivrariaDB;User Id=sa;Password=SuaSenha;TrustServerCertificate=True
```

### Passo 3: Aplicar Migrations

```bash
cd backend
dotnet ef database update -p src/Livraria.Infrastructure -s src/Livraria.API
```

### Passo 4: Executar a API

```bash
cd backend/src/Livraria.API
dotnet run
```

A API estará disponível em: **https://localhost:5000** ou **http://localhost:5000**

### Passo 5: Configurar o Frontend

Edite `frontend/src/Livraria.Blazor/wwwroot/appsettings.json`:

```json
{
  "ApiBaseAddress": "https://localhost:5000"
}
```

### Passo 6: Executar o Frontend (em outro terminal)

```bash
cd frontend/src/Livraria.Blazor
dotnet run
```

O Frontend estará disponível em: **https://localhost:5001** ou **http://localhost:5001**

### Resumo dos Acessos (Sem Docker)

| Serviço | URL |
|---------|-----|
| **Frontend (Blazor)** | https://localhost:5001 |
| **API (Swagger)** | https://localhost:5000/swagger |
| **SQL Server** | Conforme sua configuração |

---

## 📁 Estrutura do Projeto

```
Livraria/
├── backend/                        # Backend (API)
│   ├── src/
│   │   ├── Livraria.Domain/        # Entidades, Interfaces, Exceções
│   │   ├── Livraria.Application/   # CQRS (Commands/Queries), DTOs, Validators
│   │   ├── Livraria.Infrastructure/# EF Core, Repositórios, Migrations
│   │   └── Livraria.API/           # Controllers, Middleware, Swagger
│   └── tests/
│       ├── Livraria.Domain.Tests/      # Testes de entidades
│       ├── Livraria.Application.Tests/ # Testes de handlers
│       └── Livraria.Integration.Tests/ # Testes de integração
│
├── frontend/                       # Frontend (Blazor)
│   └── src/
│       └── Livraria.Blazor/        # UI, Services HTTP, Components
│
├── docker-compose.yml              # Orquestração de containers
├── .env                            # Variáveis de ambiente
├── ARQUITETURA.md                  # Documentação técnica detalhada
└── README.md                       # Este arquivo
```

## ✅ Funcionalidades Implementadas

### CRUD Completo
- [x] **Livros**: Criar, Listar, Editar, Excluir, Detalhes
- [x] **Autores**: Criar, Listar, Editar, Excluir
- [x] **Assuntos**: Criar, Listar, Editar, Excluir

### Relacionamentos
- [x] Livro ↔ Autor (N:N)
- [x] Livro ↔ Assunto (N:N)
- [x] Livro ↔ Preço por Forma de Compra

### Relatórios
- [x] Relatório de livros agrupado por autor (com efeito sanfona)
- [x] Paginação no relatório
- [x] Exportação em PDF (QuestPDF)
- [x] VIEW no banco de dados

### Interface
- [x] Dashboard com estatísticas e últimos livros
- [x] Formatação de moeda (R$)
- [x] Interface responsiva (Bootstrap 5)
- [x] Feedback visual (loading, erros, confirmações)

## 🧪 Executar Testes

```bash
# Todos os testes
dotnet test backend/Livraria.Backend.sln

# Testes de domínio
dotnet test backend/tests/Livraria.Domain.Tests

# Testes de aplicação
dotnet test backend/tests/Livraria.Application.Tests

# Testes de integração
dotnet test backend/tests/Livraria.Integration.Tests

# Com cobertura (requer coverlet)
dotnet test --collect:"XPlat Code Coverage"
```

## 🔧 Comandos Entity Framework

```bash
# Criar nova migration
dotnet ef migrations add NomeDaMigration \
  -p backend/src/Livraria.Infrastructure \
  -s backend/src/Livraria.API

# Aplicar migrations
dotnet ef database update \
  -p backend/src/Livraria.Infrastructure \
  -s backend/src/Livraria.API

# Reverter última migration
dotnet ef migrations remove \
  -p backend/src/Livraria.Infrastructure \
  -s backend/src/Livraria.API
```

## 🌐 Endpoints da API

### Livros
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/livros` | Lista todos os livros |
| GET | `/api/livros/{codL}` | Busca livro por código |
| POST | `/api/livros` | Cria novo livro |
| PUT | `/api/livros/{codL}` | Atualiza livro existente |
| DELETE | `/api/livros/{codL}` | Remove livro |

### Autores
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/autores` | Lista todos os autores |
| GET | `/api/autores/{codAu}` | Busca autor por código |
| POST | `/api/autores` | Cria novo autor |
| PUT | `/api/autores/{codAu}` | Atualiza autor existente |
| DELETE | `/api/autores/{codAu}` | Remove autor |

### Assuntos
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/assuntos` | Lista todos os assuntos |
| GET | `/api/assuntos/{codAs}` | Busca assunto por código |
| POST | `/api/assuntos` | Cria novo assunto |
| PUT | `/api/assuntos/{codAs}` | Atualiza assunto existente |
| DELETE | `/api/assuntos/{codAs}` | Remove assunto |

### Formas de Compra
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/formas-compra` | Lista formas de compra |

### Relatórios
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/relatorios/livros-por-autor` | Dados do relatório (JSON) |
| GET | `/api/relatorios/livros-por-autor/pdf` | Download do relatório (PDF) |

## 🏗️ Arquitetura

O projeto segue **Clean Architecture** com os seguintes padrões:

- **CQRS** (Command Query Responsibility Segregation) com MediatR
- **Repository Pattern** para abstração de dados
- **Unit of Work** para transações
- **Rich Domain Model** (DDD) com validações na entidade
- **Result Pattern** para tratamento de erros

## 📄 Licença

