var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var pokemons = new List<Pokemon>
{
    new(1, "Bulbasaur", "Planta", "Veneno", 45, "Um Pokémon semente que carrega uma planta nas costas."),
    new(2, "Charmander", "Fogo", null, 39, "A chama de sua cauda indica seu estado de saúde."),
    new(3, "Squirtle", "Água", null, 44, "Um Pokémon tartaruga que dispara água de sua boca."),
    new(4, "Pikachu", "Elétrico", null, 35, "Um Pokémon elétrico que armazena energia em suas bochechas."),
    new(5, "Jigglypuff", "Normal", "Fada", 115, "Um Pokémon fofinho que adormece os oponentes com sua canção."),
    new(6, "Gengar", "Fantasma", "Venenoso", 60, "Um Pokémon fantasma que se esconde nas sombras e assusta os inimigos."),
    new(7, "Eevee", "Normal", null, 55, "Um Pokémon versátil que pode evoluir para diferentes formas."),
    new(8, "Snorlax", "Normal", null, 160, "Um Pokémon preguiçoso que passa a maior parte do tempo dormindo."),
    new(9, "Mewtwo", "Psíquico", null, 106, "Um Pokémon lendário criado a partir do DNA de Mew."),
    new(10, "Gyarados", "Água", "Voador", 95, "Um Pokémon feroz que evolui de Magikarp e causa destruição com sua força.")
};

app.MapGet("/", () => Results.Ok(new { mensagem = "A Pokédex API está no ar!" }));

app.MapGet("/api/pokemon", () =>
{
    var resposta = pokemons.Select(pokemon => ToDto(pokemon));
    return Results.Ok(resposta);
});

app.MapGet("/api/pokemon/{id:int}", (int id) =>
{
    var pokemon = pokemons.FirstOrDefault(pokemon => pokemon.Id == id);
    return pokemon is null
        ? Results.NotFound(new { mensagem = "Pokémon não encontrado." })
        : Results.Ok(ToDto(pokemon));
});

app.MapPost("/api/pokemon", (PokemonInputDto entrada) =>
{
    var novoId = pokemons.Count == 0 ? 1 : pokemons.Max(pokemon => pokemon.Id) + 1;
    var pokemon = new Pokemon(novoId, entrada.Nome, entrada.TipoPrimario, entrada.TipoSecundario,
        entrada.PontosDeVida, entrada.Descricao);

    pokemons.Add(pokemon);
    return Results.Created($"/api/pokemon/{pokemon.Id}", ToDto(pokemon));
});

app.MapPut("/api/pokemon/{id:int}", (int id, PokemonInputDto entrada) =>
{
    var indice = pokemons.FindIndex(pokemon => pokemon.Id == id);
    if (indice == -1)
        return Results.NotFound(new { mensagem = "Pokémon não encontrado." });

    var pokemonAtualizado = new Pokemon(id, entrada.Nome, entrada.TipoPrimario, entrada.TipoSecundario,
        entrada.PontosDeVida, entrada.Descricao);
    pokemons[indice] = pokemonAtualizado;

    return Results.Ok(ToDto(pokemonAtualizado));
});

app.MapDelete("/api/pokemon/{id:int}", (int id) =>
{
    var pokemon = pokemons.FirstOrDefault(pokemon => pokemon.Id == id);
    if (pokemon is null)
        return Results.NotFound(new { mensagem = "Pokémon não encontrado." });

    pokemons.Remove(pokemon);
    return Results.NoContent();
});

app.Run();

static PokemonDto ToDto(Pokemon pokemon) => new(
    pokemon.Id,
    pokemon.Nome,
    pokemon.TipoPrimario,
    pokemon.TipoSecundario,
    pokemon.PontosDeVida,
    pokemon.Descricao);

record Pokemon(
    int Id,
    string Nome,
    string TipoPrimario,
    string? TipoSecundario,
    int PontosDeVida,
    string Descricao);

record PokemonDto(
    int Id,
    string Nome,
    string TipoPrimario,
    string? TipoSecundario,
    int PontosDeVida,
    string Descricao);

record PokemonInputDto(
    string Nome,
    string TipoPrimario,
    string? TipoSecundario,
    int PontosDeVida,
    string Descricao);
