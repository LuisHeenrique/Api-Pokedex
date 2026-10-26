using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PokedexApi.Migrations
{
    /// <inheritdoc />
    public partial class BancoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pokemons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    TipoPrimario = table.Column<string>(type: "TEXT", nullable: false),
                    TipoSecundario = table.Column<string>(type: "TEXT", nullable: true),
                    PontosDeVida = table.Column<int>(type: "INTEGER", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    ImagemUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pokemons", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Pokemons",
                columns: new[] { "Id", "Descricao", "ImagemUrl", "Nome", "PontosDeVida", "TipoPrimario", "TipoSecundario" },
                values: new object[,]
                {
                    { 1, "Um Pokémon semente que carrega uma planta nas costas.", "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/1.png", "Bulbasaur", 45, "Planta", "Veneno" },
                    { 2, "A chama de sua cauda indica seu estado de saúde.", "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/2.png", "Charmander", 39, "Fogo", null },
                    { 3, "Um Pokémon tartaruga que dispara água de sua boca.", "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/3.png", "Squirtle", 44, "Água", null },
                    { 4, "Um Pokémon elétrico que armazena energia em suas bochechas.", "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/4.png", "Pikachu", 35, "Elétrico", null },
                    { 5, "Um Pokémon fofinho que adormece os oponentes com sua canção.", "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/5.png", "Jigglypuff", 115, "Normal", "Fada" },
                    { 6, "Um Pokémon fantasma que se esconde nas sombras e assusta os inimigos.", "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/6.png", "Gengar", 60, "Fantasma", "Veneno" },
                    { 7, "Um Pokémon versátil que pode evoluir para diferentes formas.", "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/7.png", "Eevee", 55, "Normal", null },
                    { 8, "Um Pokémon preguiçoso que passa a maior parte do tempo dormindo.", "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/8.png", "Snorlax", 160, "Normal", null },
                    { 9, "Um Pokémon lendário criado a partir do DNA de Mew.", "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/9.png", "Mewtwo", 106, "Psíquico", null },
                    { 10, "Um Pokémon feroz que evolui de Magikarp e causa destruição com sua força.", "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/10.png", "Gyarados", 95, "Água", "Voador" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pokemons");
        }
    }
}
