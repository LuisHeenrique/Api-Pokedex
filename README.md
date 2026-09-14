# Pokédex API

Minimal API desenvolvida em ASP.NET Core com .NET 10 para gerenciar Pokémon. Os dados ficam somente em memória, portanto voltam ao estado inicial ao reiniciar a aplicação.

## Tecnologias e requisitos atendidos

- ASP.NET Core Minimal API no .NET 10;
- recurso `Pokemon` definido como `record`;
- armazenamento em `List<Pokemon>` com Bulbasaur e Charmander iniciais;
- `PokemonDto` completo, incluindo `Id`;
- `PokemonInputDto` para entrada, sem `Id`;
- respostas JSON e códigos HTTP adequados.

## Como executar

Com o SDK .NET 10 instalado, dentro da pasta do projeto execute:

```bash
dotnet run --urls http://localhost:5050
```

A API ficará disponível em `http://localhost:5050`. A coleção de testes está na pasta `bruno`; abra-a no Bruno e execute as requisições na ordem numérica.

## Rotas

| Método | Rota | Resposta |
| --- | --- | --- |
| GET | `/` | `200 OK` com mensagem de disponibilidade |
| GET | `/api/pokemon` | `200 OK` com todos os Pokémon |
| GET | `/api/pokemon/{id}` | `200 OK` ou `404 Not Found` |
| POST | `/api/pokemon` | `201 Created` e o Pokémon criado |
| PUT | `/api/pokemon/{id}` | `200 OK` ou `404 Not Found` |
| DELETE | `/api/pokemon/{id}` | `204 No Content` ou `404 Not Found` |

## Exemplo de JSON para criação/atualização

```json
{
  "nome": "Pikachu",
  "tipoPrimario": "Elétrico",
  "tipoSecundario": null,
  "pontosDeVida": 35,
  "descricao": "Armazena eletricidade nas bolsas das bochechas."
}
```

## Entrega complementar

Ainda é necessário publicar este projeto em um repositório GitHub público e gravar/enviar o vídeo de demonstração solicitado na atividade.
