using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Aplica automaticamente as migrations pendentes ao inicializar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    db.Database.Migrate();
}

app.UseCors();

// Se a requisição for para a raiz e não solicitar HTML explicitamente (ex: Bruno/Postman), retorna o status JSON da API
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/" && !context.Request.Headers.Accept.ToString().Contains("text/html"))
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsJsonAsync(new { mensagem = "A Pokédex API está no ar!" });
        return;
    }
    await next();
});

// Serve os arquivos estáticos do frontend da pasta wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Endpoint de status da API
app.MapGet("/api", () => Results.Ok(new { mensagem = "A Pokédex API está no ar!" }));

// CRUD da coleção Pokemon com persistência no banco SQLite via EF Core
app.MapGet("/api/pokemon", async (ApiDbContext db) =>
{
    var pokemons = await db.Pokemons.AsNoTracking().ToListAsync();
    var resposta = pokemons.Select(pokemon => ToDto(pokemon));
    return Results.Ok(resposta);
});

app.MapGet("/api/pokemon/{id:int}", async (int id, ApiDbContext db) =>
{
    var pokemon = await db.Pokemons.FindAsync(id);
    return pokemon is null
        ? Results.NotFound(new { mensagem = "Pokémon não encontrado." })
        : Results.Ok(ToDto(pokemon));
});

app.MapPost("/api/pokemon", async (PokemonInputDto entrada, ApiDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(entrada.Nome))
        return Results.BadRequest(new { mensagem = "O nome do Pokémon é obrigatório." });

    if (string.IsNullOrWhiteSpace(entrada.TipoPrimario))
        return Results.BadRequest(new { mensagem = "O tipo primário é obrigatório." });

    if (entrada.PontosDeVida <= 0)
        return Results.BadRequest(new { mensagem = "Os pontos de vida devem ser maiores que zero." });

    if (string.IsNullOrWhiteSpace(entrada.Descricao))
        return Results.BadRequest(new { mensagem = "A descrição é obrigatória." });

    var pokemon = new Pokemon
    {
        Nome = entrada.Nome.Trim(),
        TipoPrimario = entrada.TipoPrimario.Trim(),
        TipoSecundario = string.IsNullOrWhiteSpace(entrada.TipoSecundario) ? null : entrada.TipoSecundario.Trim(),
        PontosDeVida = entrada.PontosDeVida,
        Descricao = entrada.Descricao.Trim(),
        ImagemUrl = string.IsNullOrWhiteSpace(entrada.ImagemUrl) ? null : entrada.ImagemUrl.Trim()
    };

    db.Pokemons.Add(pokemon);
    await db.SaveChangesAsync();

    return Results.Created($"/api/pokemon/{pokemon.Id}", ToDto(pokemon));
});

app.MapPut("/api/pokemon/{id:int}", async (int id, PokemonInputDto entrada, ApiDbContext db) =>
{
    var pokemon = await db.Pokemons.FindAsync(id);
    if (pokemon is null)
        return Results.NotFound(new { mensagem = "Pokémon não encontrado." });

    if (string.IsNullOrWhiteSpace(entrada.Nome))
        return Results.BadRequest(new { mensagem = "O nome do Pokémon é obrigatório." });

    if (string.IsNullOrWhiteSpace(entrada.TipoPrimario))
        return Results.BadRequest(new { mensagem = "O tipo primário é obrigatório." });

    if (entrada.PontosDeVida <= 0)
        return Results.BadRequest(new { mensagem = "Os pontos de vida devem ser maiores que zero." });

    if (string.IsNullOrWhiteSpace(entrada.Descricao))
        return Results.BadRequest(new { mensagem = "A descrição é obrigatória." });

    pokemon.Nome = entrada.Nome.Trim();
    pokemon.TipoPrimario = entrada.TipoPrimario.Trim();
    pokemon.TipoSecundario = string.IsNullOrWhiteSpace(entrada.TipoSecundario) ? null : entrada.TipoSecundario.Trim();
    pokemon.PontosDeVida = entrada.PontosDeVida;
    pokemon.Descricao = entrada.Descricao.Trim();

    if (!string.IsNullOrWhiteSpace(entrada.ImagemUrl))
    {
        pokemon.ImagemUrl = entrada.ImagemUrl.Trim();
    }

    await db.SaveChangesAsync();

    return Results.Ok(ToDto(pokemon));
});

app.MapDelete("/api/pokemon/{id:int}", async (int id, ApiDbContext db) =>
{
    var pokemon = await db.Pokemons.FindAsync(id);
    if (pokemon is null)
        return Results.NotFound(new { mensagem = "Pokémon não encontrado." });

    db.Pokemons.Remove(pokemon);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();

// Funções e Modelos
static PokemonDto ToDto(Pokemon pokemon) => new(
    pokemon.Id,
    pokemon.Nome,
    pokemon.TipoPrimario,
    pokemon.TipoSecundario,
    pokemon.PontosDeVida,
    pokemon.Descricao,
    string.IsNullOrWhiteSpace(pokemon.ImagemUrl)
        ? $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{pokemon.Id}.png"
        : pokemon.ImagemUrl);

public class Pokemon
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string TipoPrimario { get; set; } = string.Empty;
    public string? TipoSecundario { get; set; }
    public int PontosDeVida { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string? ImagemUrl { get; set; }
}

public record PokemonDto(
    int Id,
    string Nome,
    string TipoPrimario,
    string? TipoSecundario,
    int PontosDeVida,
    string Descricao,
    string? ImagemUrl);

public record PokemonInputDto(
    string Nome,
    string TipoPrimario,
    string? TipoSecundario,
    int PontosDeVida,
    string Descricao,
    string? ImagemUrl = null);

// DbContext do EF Core
public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }

    public DbSet<Pokemon> Pokemons => Set<Pokemon>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Pokemon>().HasData(
            new Pokemon { Id = 1,  Nome = "Bulbasaur",  TipoPrimario = "Planta",   TipoSecundario = "Veneno", PontosDeVida = 45,  Descricao = "Um Pokémon semente que carrega uma planta nas costas.",                        ImagemUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/1.png" },
            new Pokemon { Id = 2,  Nome = "Charmander", TipoPrimario = "Fogo",     TipoSecundario = null,     PontosDeVida = 39,  Descricao = "A chama de sua cauda indica seu estado de saúde.",                            ImagemUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/4.png" },
            new Pokemon { Id = 3,  Nome = "Squirtle",   TipoPrimario = "Água",     TipoSecundario = null,     PontosDeVida = 44,  Descricao = "Um Pokémon tartaruga que dispara água de sua boca.",                         ImagemUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/7.png" },
            new Pokemon { Id = 4,  Nome = "Pikachu",    TipoPrimario = "Elétrico", TipoSecundario = null,     PontosDeVida = 35,  Descricao = "Um Pokémon elétrico que armazena energia em suas bochechas.",                ImagemUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/25.png" },
            new Pokemon { Id = 5,  Nome = "Jigglypuff", TipoPrimario = "Normal",   TipoSecundario = "Fada",   PontosDeVida = 115, Descricao = "Um Pokémon fofinho que adormece os oponentes com sua canção.",               ImagemUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/39.png" },
            new Pokemon { Id = 6,  Nome = "Gengar",     TipoPrimario = "Fantasma", TipoSecundario = "Veneno", PontosDeVida = 60,  Descricao = "Um Pokémon fantasma que se esconde nas sombras e assusta os inimigos.",      ImagemUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/94.png" },
            new Pokemon { Id = 7,  Nome = "Eevee",      TipoPrimario = "Normal",   TipoSecundario = null,     PontosDeVida = 55,  Descricao = "Um Pokémon versátil que pode evoluir para diferentes formas.",               ImagemUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/133.png" },
            new Pokemon { Id = 8,  Nome = "Snorlax",    TipoPrimario = "Normal",   TipoSecundario = null,     PontosDeVida = 160, Descricao = "Um Pokémon preguiçoso que passa a maior parte do tempo dormindo.",           ImagemUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/143.png" },
            new Pokemon { Id = 9,  Nome = "Mewtwo",     TipoPrimario = "Psíquico", TipoSecundario = null,     PontosDeVida = 106, Descricao = "Um Pokémon lendário criado a partir do DNA de Mew.",                        ImagemUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/150.png" },
            new Pokemon { Id = 10, Nome = "Gyarados",   TipoPrimario = "Água",     TipoSecundario = "Voador", PontosDeVida = 95,  Descricao = "Um Pokémon feroz que evolui de Magikarp e causa destruição com sua força.", ImagemUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/130.png" }
        );
    }
}
