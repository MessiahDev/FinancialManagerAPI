# FinancialManagerAPI
API RESTful desenvolvida em .NET Core para gerenciamento de finanças pessoais, permitindo o controle de receitas, despesas e categorias, com autenticação segura via JWT.

Índice
Funcionalidades

Tecnologias Utilizadas

Pré-requisitos

Como Executar

Endpoints

Envio de E-mail

Deploy

Autor

# Funcionalidades:

Cadastro e autenticação de usuários com JWT com confirmação de e-mail

CRUD de receitas e despesas

Gerenciamento de categorias financeiras

Proteção de rotas com autenticação

Documentação interativa via Swagger

Tecnologias Utilizadas:
.NET Core 8.0

Entity Framework Core

JWT Bearer Authentication

Swagger para documentação da API

Banco de dados: PostgreSQL

# Pré-requisitos:

Antes de iniciar, certifique-se de ter instalado:

.NET SDK (versão utilizada no projeto)

PostgreSQL ou outro banco de dados compatível

Visual Studio ou outro IDE de sua preferência

# Como Executar:

- Clone o repositório:

git clone https://github.com/MessiahDev/FinancialManagerAPI.git

- Navegue até o diretório do projeto:

cd FinancialManagerAPI

- Restaure as dependências:

dotnet restore

- Execute as migrações para criar o banco de dados:

dotnet ef database update

- Inicie a aplicação:

dotnet run

- Acesse a documentação Swagger em:

- Quando rodando com perfil https (recomendado):

HTTPS: https://localhost:7023

HTTP: http://localhost:5257

- Quando rodando com IIS Express:

HTTPS: https://localhost:44349

HTTP: http://localhost:65526
# Endpoints:

# Autenticação

POST /api/Auth/register – Registro de novo usuário

POST /api/Auth/login – Autenticação e geração de token JWT

GET /api/Auth/profile – Retorna os dados do usuário autenticado

GET /api/Auth/confirm-email – Confirmação de e-mail com token enviado

POST /api/Auth/resend-confirmation-email – Reenvio de e-mail de confirmação

POST /api/Auth/forgot-password – Solicitação de redefinição de senha (envia e-mail com token)

POST /api/Auth/reset-password – Redefine a senha utilizando o token enviado

# Usuários

GET /api/users - Listar usuários

GET /api/users/{id} - Obter detalhes de um usuário

PUT /api/users/{id} - Atualizar informações do usuário

DELETE /api/users/{id} - Remover usuário

# Receitas

GET /api/revenues - Listar receitas

POST /api/revenues - Criar nova receita

GET /api/revenues/{id} - Obter detalhes de uma receita

GET /api/revenues/user{id} - Obter detalhes de receitas por usuário

PUT /api/revenues/{id} - Atualizar receita

DELETE /api/revenues/{id} - Remover receita

# Despesas

GET /api/expenses - Listar despesas

POST /api/expenses - Criar nova despesa

GET /api/expenses/{id} - Obter detalhes de uma despesa

GET /api/expenses/user{id} - Obter detalhes de despesas por usuário

PUT /api/expenses/{id} - Atualizar despesa

DELETE /api/expenses/{id} - Remover despesa

# Categorias

GET /api/categories - Listar categorias

POST /api/categories - Criar nova categoria

GET /api/categories/{id} - Obter detalhes de uma categoria

GET /api/categories/{id} - Obter detalhes de categorias por usuário

PUT /api/categories/{id} - Atualizar categoria

DELETE /api/categories/{id} - Remover categoria

Nota: Todos os endpoints (exceto registro e login) requerem autenticação via token JWT no header Authorization: Bearer {token}.

- Envio de E-mail
- 
A API possui suporte para envio de e-mails, como parte do processo de confirmação de registro de usuários ou outras interações com a aplicação. Para configurar o envio de e-mails, siga as instruções abaixo:

# Configuração SMTP:

Configurando o Servidor SMTP:

A API utiliza o servidor SMTP para enviar e-mails. Para configurá-lo corretamente, edite o arquivo appsettings.json (ou appsettings.Production.json para produção) com os dados do servidor SMTP.

Exemplo de configuração para ambiente de desenvolvimento (appsettings.json):

"Smtp": {

  "Host": "smtp.gmail.com",
  
  "Port": 587,

  "Username": "seuemail@gmail.com",
  
  "Password": "suasenha",
  
  "From": "seuemail@gmail.com"
  
}

- Exemplo de configuração para ambiente de produção (appsettings.Production.json) se usa variáveis de ambiente.

Nota: Utilize variáveis de ambiente ou mecanismos seguros para armazenar as credenciais em ambientes de produção (como SMTP__Username, SMTP__Password, etc.).

- Variáveis de Ambiente: Para configuração segura, é possível utilizar variáveis de ambiente para armazenar as credenciais, especialmente em ambientes de produção. Exemplo:

export SMTP__HOST="smtp.gmail.com"

export SMTP__PORT=587

export SMTP__USERNAME="seuemail@gmail.com"

export SMTP__PASSWORD="suasenha"

export SMTP__FROM="seuemail@gmail.com"

Envio de E-mails: A API pode enviar e-mails utilizando a classe EmailService. A classe é configurada para enviar e-mails para usuários, como parte do processo de verificação de conta ou envio de notificações.

- Como Funciona

Quando um usuário se registra ou realiza ações que requerem confirmação, o serviço EmailService utiliza as configurações SMTP para enviar um e-mail para o endereço de e-mail do usuário.

O e-mail será enviado a partir do endereço configurado no parâmetro From.

# Dependências:

- O serviço de envio de e-mail utiliza as seguintes bibliotecas:

Pacotes NuGet Utilizados:
 
AutoMapper.Extensions.Microsoft.DependencyInjection – Facilita o mapeamento de objetos com o AutoMapper.

BCrypt.Net-Next – Utilizado para hashing de senhas com segurança.

FluentValidation – Biblioteca para validação fluente de objetos.

MailKit – Envio de e-mails via protocolo SMTP.

Microsoft.AspNetCore.Authentication.JwtBearer – Autenticação baseada em JWT.

Microsoft.EntityFrameworkCore – ORM principal usado para manipulação de banco de dados.

Microsoft.EntityFrameworkCore.Design – Ferramentas de design do EF Core (migrações, scaffolding etc).

Microsoft.EntityFrameworkCore.Tools – Ferramentas para uso via CLI com o EF Core.

Npgsql.EntityFrameworkCore.PostgreSQL – Provedor do Entity Framework Core para PostgreSQL. (Troque caso for usar outro banco de dados relacional)

Swashbuckle.AspNetCore – Geração automática de documentação Swagger para a API.

System.IdentityModel.Tokens.Jwt – Manipulação e validação de tokens JWT.

# Deploy:

- Para facilitar o acesso e testes da API, você pode realizar o deploy gratuito em plataformas como:

Render

Railway

Heroku

Autor:
Alex Messias Alle
MessiahDev - GitHub
