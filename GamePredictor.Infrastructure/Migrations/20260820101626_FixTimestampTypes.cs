using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GamePredictor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTimestampTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Developers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Avg_Metacritic_Last_3 = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    games_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Developer_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Game",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    rawg_id = table.Column<int>(type: "integer", nullable: true),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    genre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    releasedate = table.Column<DateOnly>(type: "date", nullable: true),
                    platforms = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    budget_estimate = table.Column<double>(type: "double precision", precision: 6, scale: 2, nullable: true),
                    metacritic_score = table.Column<int>(type: "integer", nullable: true),
                    is_released = table.Column<bool>(type: "boolean", nullable: false),
                    steam_app_Id = table.Column<int>(type: "integer", nullable: true),
                    trailer_youtube_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    developer_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Game_pkey", x => x.id);
                    table.ForeignKey(
                        name: "Game_developer_id_fkey",
                        column: x => x.developer_id,
                        principalTable: "Developers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "NewsSentiment",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sentiment_score = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: true),
                    relevance = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: true),
                    keywords = table.Column<string>(type: "text", nullable: true),
                    game_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("NewsSentiment_pkey", x => x.id);
                    table.ForeignKey(
                        name: "NewsSentiment_game_id_fkey",
                        column: x => x.game_id,
                        principalTable: "Game",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Predictions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    predicted_metacritic = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    sales_class = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    confidence = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    risk_level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    arguments = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verified = table.Column<bool>(type: "boolean", nullable: true),
                    game_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Prediction_pkey", x => x.id);
                    table.ForeignKey(
                        name: "Prediction_game_id_fkey",
                        column: x => x.game_id,
                        principalTable: "Game",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "PreReleaseMetrics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    wishlist_count = table.Column<int>(type: "integer", nullable: false),
                    twitch_viewer_avg = table.Column<int>(type: "integer", nullable: true),
                    youtube_trailer_views = table.Column<long>(type: "bigint", nullable: true),
                    reddit_mentions = table.Column<int>(type: "integer", nullable: false),
                    game_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PreRelelaseMetric_pkey", x => x.id);
                    table.ForeignKey(
                        name: "PreRelelaseMetric_game_id_fkey",
                        column: x => x.game_id,
                        principalTable: "Game",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Game_developer_id",
                table: "Game",
                column: "developer_id");

            migrationBuilder.CreateIndex(
                name: "IX_NewsSentiment_game_id",
                table: "NewsSentiment",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_game_id",
                table: "Predictions",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_PreReleaseMetrics_game_id",
                table: "PreReleaseMetrics",
                column: "game_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsSentiment");

            migrationBuilder.DropTable(
                name: "Predictions");

            migrationBuilder.DropTable(
                name: "PreReleaseMetrics");

            migrationBuilder.DropTable(
                name: "Game");

            migrationBuilder.DropTable(
                name: "Developers");
        }
    }
}
