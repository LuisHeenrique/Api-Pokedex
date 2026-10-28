# Pokédex API — AP2

API REST desenvolvida em **ASP.NET Core Minimal API com .NET 10**, persistência em banco **SQLite via Entity Framework Core** e frontend integrado servido pela própria API.

---

## Tecnologias

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core Minimal API (.NET 10) |
| ORM / Banco | Entity Framework Core 10 + SQLite |
| Frontend | HTML5 + CSS3 + JavaScript (Vanilla) |
| Testes de API | Bruno Collection |

---

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Bruno](https://www.usebruno.com/) (para testar a Collection)

Não são necessários Docker, Node.js, npm ou banco de dados externo.

---

## Como executar

### 1. Clonar o repositório

```bash
git clone <URL_DO_REPOSITORIO>
cd PokedexApi/PokedexApi
```

### 2. Preparar o banco de dados

As migrations já estão versionadas no repositório. Ao iniciar a aplicação, elas são aplicadas **automaticamente** (via `db.Database.Migrate()` no `Program.cs`). Não é necessário nenhum comando manual para criar o banco.

Se preferir aplicar as migrations manualmente antes de executar:

```bash
dotnet ef database update
```

> O arquivo `pokedex.db` (SQLite) é criado na pasta do projeto na primeira execução e está no `.gitignore` — não é versionado.

### 3. Iniciar a API

```bash
dotnet run --urls http://localhost:5050
```

A API ficará disponível em **`http://localhost:5050`**.

---

## Como abrir a interface (Frontend)

Com a API rodando, abra no navegador:

```
http://localhost:5050
```

O frontend é servido diretamente pela API a partir da pasta `wwwroot` (arquivos estáticos via `UseDefaultFiles` + `UseStaticFiles`). **Não é necessário nenhum servidor separado.**

---

## Rotas da API

| Método | Rota | Corpo | Resposta de sucesso | Resposta de erro |
|---|---|---|---|---|
| `GET` | `/api/pokemon` | — | `200 OK` + lista JSON | — |
| `GET` | `/api/pokemon/{id}` | — | `200 OK` + Pokémon | `404 Not Found` |
| `POST` | `/api/pokemon` | JSON | `201 Created` + Pokémon criado | `400 Bad Request` |
| `PUT` | `/api/pokemon/{id}` | JSON | `200 OK` + Pokémon atualizado | `404 Not Found` / `400 Bad Request` |
| `DELETE` | `/api/pokemon/{id}` | — | `204 No Content` | `404 Not Found` |

### Exemplo de corpo JSON (POST / PUT)

```json
{
  "nome": "Lucario",
  "tipoPrimario": "Lutador",
  "tipoSecundario": "Aço",
  "pontosDeVida": 70,
  "descricao": "Um Pokémon que detecta e manipula energia Aura.",
  "imagemUrl": https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/448.png?utm_source=chatgpt.com
}
```

> O campo `imagemUrl` é opcional. Se omitido ou nulo, o sistema usa automaticamente a arte oficial da PokéAPI pelo ID do registro.

---

## Collection de testes (Bruno)

A collection está na pasta `bruno/` do projeto. Para utilizá-la:

1. Abra o Bruno
2. Clique em **Open Collection** e selecione a pasta `bruno/`
3. Selecione o ambiente **Local** (já configurado com `baseUrl = http://localhost:5050`)
4. Execute as requisições na ordem numérica:
   - `00` — Status da API (`GET /api`)
   - `01` — Listar Pokémons (`GET /api/pokemon`)
   - `02` — Buscar por ID (`GET /api/pokemon/{id}`)
   - `03` — Criar Pokémon (`POST /api/pokemon`) → espera `201`
   - `04` — Atualizar Pokémon (`PUT /api/pokemon/{id}`) → espera `200`
   - `05` — Excluir Pokémon (`DELETE /api/pokemon/{id}`) → espera `204`
   - `06` — Buscar ID excluído (`GET /api/pokemon/{id}`) → espera `404`
   - `07` — Buscar ID inexistente (`GET /api/pokemon/9999`) → espera `404`

---

## Persistência dos dados

- Os dados são gravados permanentemente no banco SQLite `pokedex.db`.
- Ao **reiniciar** a aplicação, todos os registros não excluídos permanecem disponíveis.
- 10 Pokémons iniciais são inseridos via **seed** na migration `BancoInicial` (caso o banco seja recriado do zero).

---

## Estrutura do projeto

```
PokedexApi/
├── Migrations/                  # Migrations do EF Core versionadas
│   ├── 20261026225835_BancoInicial.cs
│   └── ApiDbContextModelSnapshot.cs
├── wwwroot/                     # Frontend (servido como arquivos estáticos)
│   ├── index.html
│   ├── app.js
│   └── style.css
├── bruno/                       # Collection de testes da API
│   ├── 00-status-api/
│   ├── 01-listar-pokemon/
│   ├── 02-buscar-pokemon-por-id/
│   ├── 03-criar-pokemon/
│   ├── 04-atualizar-pokemon/
│   ├── 05-excluir-pokemon/
│   ├── 06-buscar-pokemon-excluido/
│   ├── 07-buscar-pokemon-inexistente/
│   └── environments/
├── Program.cs                   # Toda a lógica: Model, DbContext, endpoints
├── PokedexApi.csproj
├── appsettings.json             # Connection string do SQLite
└── .gitignore                   # Exclui bin/, obj/, *.db
```

---

## Vídeo de demonstração — AP2

> 🎬 **[https://youtu.be/RWMZAmVj7KA]**

O vídeo demonstra:
- Apresentação do tema e organização do projeto (backend, frontend, collection)
- Aplicação iniciando com migrations automáticas
- Testes CRUD no Bruno/Postman com códigos de resposta (`201`, `200`, `204`, `404`)
- Uso do frontend para cadastrar, editar, listar e excluir Pokémons reais
- Confirmação da persistência após reiniciar a API
