
# Sommus.DengueApi

API para consulta e persistência de dados de alertas de dengue para a cidade de Belo Horizonte, consumindo a API pública AlertaDengue e disponibilizando endpoints para consulta via Web API.

---

## Tecnologias

- .NET 7 / C#
- ASP.NET Core Web API
- Entity Framework Core
- MySQL
- Swagger (OpenAPI)
- Front-end: React

---

## Descrição do Projeto

Este projeto foi desenvolvido para o teste técnico da Sommus Sistemas. Ele realiza:

1. Consulta dos dados de dengue dos últimos 6 meses para a cidade de Belo Horizonte (código IBGE: 3106200) utilizando a API AlertaDengue.  
2. Persistência dos dados em banco MySQL via Entity Framework Core.  
3. Exposição de endpoints para consulta dos dados por semana epidemiológica, últimas semanas e semanas com alerta.

---

## Endpoints principais

| Endpoint                          | Método | Descrição                                               |
| -------------------------------- | ------ | ------------------------------------------------------ |
| `/api/importacao`                 | POST   | Importa e persiste os dados dos últimos 6 meses        |
| `/api/dengue/por-semana`          | GET    | Consulta dados de dengue por semana e ano (ew, ey)     |
| `/api/dengue/ultimas-semanas`     | GET    | Consulta dados das últimas N semanas (default = 3)     |
| `/api/dengue/semanas-com-alerta`  | GET    | Consulta semanas com alerta, filtrando por nível mínimo|

---

## Configuração do Banco de Dados

1. Instale o MySQL (versão 8.x recomendada).  
2. Crie um banco de dados para o projeto, por exemplo `sommus_dengue`:

```sql
CREATE DATABASE sommus_dengue;
```

3. Configure a connection string no arquivo `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=sommus_dengue;User=root;Password=SuaSenha;"
  }
}
```

Substitua `root` e `SuaSenha` pelos dados do seu banco.

4. Execute as migrações do Entity Framework para criar as tabelas:

```bash
dotnet ef database update
```

---

## Como executar o projeto

1. Clone o repositório:

```bash
git clone https://github.com/ygorriam/Teste-Sommus
cd sommus.dengueapi
```

2. Configure a connection string conforme o passo anterior.  
3. Restaure os pacotes e compile:

```bash
dotnet restore
dotnet build
```

4. Execute a API:

```bash
dotnet run
```

5. Acesse o Swagger para explorar os endpoints em:

```
https://localhost:{porta}/swagger
```

---

## Como importar os dados

Para importar os dados de dengue dos últimos 6 meses, faça uma requisição POST para o endpoint:

```http
POST /api/importacao
```

Isso irá consumir a API pública AlertaDengue, armazenar os dados no banco e permitir a consulta via API.

---

## Como usar os endpoints de consulta

- Exemplo: Consultar dados da semana epidemiológica 40 do ano 2023

```http
GET /api/dengue/por-semana?ew=40&ey=2023
```

- Exemplo: Consultar as últimas 3 semanas (padrão)

```http
GET /api/dengue/ultimas-semanas
```

- Exemplo: Consultar últimas 5 semanas com nível mínimo de alerta 2

```http
GET /api/dengue/semanas-com-alerta?quantidade=5&nivelMinimo=2
```

---

## Testes

Este projeto inclui testes unitários para as camadas de serviço e controle. Para executar os testes:

```bash
dotnet test
```

---

## Considerações finais

- A aplicação segue boas práticas de arquitetura, separando responsabilidades em camadas Controller, Service e Repository.  
- O consumo da API externa foi encapsulado em serviço dedicado para facilitar manutenção e testes.  
- O projeto pode ser expandido para incluir front-end, notificações e melhorias na análise dos dados.

---

**Sommus Sistemas - Teste Técnico Desenvolvedor - 2025**
