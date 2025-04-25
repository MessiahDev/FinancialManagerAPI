# FinancialManagerAPI
API RESTful desenvolvida em .NET Core para gerenciamento de finanças pessoais, permitindo o controle de receitas, despesas e categorias, com autenticação segura via JWT.

Índice
Funcionalidades

Tecnologias Utilizadas

Pré-requisitos

Como Executar

Endpoints

Deploy

Autor

# Funcionalidades:

Cadastro e autenticação de usuários com JWT

CRUD de receitas e despesas

Gerenciamento de categorias financeiras

Proteção de rotas com autenticação

Documentação interativa via Swagger

# Tecnologias Utilizadas:

.NET Core 8.0

Entity Framework Core

JWT Bearer Authentication

Swagger para documentação da API

Banco de dados: (especificar, ex: SQL Server, PostgreSQL)

# Pré-requisitos:

Antes de iniciar, certifique-se de ter instalado:

.NET SDK (versão utilizada no projeto)

MySQL ou outro banco de dados compatível

Visual Studio ou outro IDE de sua preferência

# Como Executar:

Clone o repositório:

git clone https://github.com/MessiahDev/FinancialManagerAPI.git
Navegue até o diretório do projeto:

cd FinancialManagerAPI
Restaure as dependências:

dotnet restore
Execute as migrações para criar o banco de dados:

dotnet ef database update
Inicie a aplicação:

dotnet run
Acesse a documentação Swagger em:

http://localhost:5000/swagger

# Endpoints:

Autenticação
POST /api/auth/register - Registro de novo usuário

POST /api/auth/login - Autenticação e geração de token JWT

Usuários
GET /api/users - Listar usuários

GET /api/users/{id} - Obter detalhes de um usuário

PUT /api/users/{id} - Atualizar informações do usuário

DELETE /api/users/{id} - Remover usuário

Receitas
GET /api/incomes - Listar receitas

POST /api/incomes - Criar nova receita

GET /api/incomes/{id} - Obter detalhes de uma receita

PUT /api/incomes/{id} - Atualizar receita

DELETE /api/incomes/{id} - Remover receita

Despesas
GET /api/expenses - Listar despesas

POST /api/expenses - Criar nova despesa

GET /api/expenses/{id} - Obter detalhes de uma despesa

PUT /api/expenses/{id} - Atualizar despesa

DELETE /api/expenses/{id} - Remover despesa

Categorias
GET /api/categories - Listar categorias

POST /api/categories - Criar nova categoria

GET /api/categories/{id} - Obter detalhes de uma categoria

PUT /api/categories/{id} - Atualizar categoria

DELETE /api/categories/{id} - Remover categoria

Nota: Todos os endpoints (exceto registro e login) requerem autenticação via token JWT no header Authorization: Bearer {token}.

Deploy
Para facilitar o acesso e testes da API, você pode realizar o deploy gratuito em plataformas como:

Render

Railway

Heroku (com suporte ao .NET)

# Autor:

Alex Messias Alle
MessiahDev - GitHub

