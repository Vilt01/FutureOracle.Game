using System;
using System.Collections.Generic;
using GamePredictor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamePredictor.Infrastructure.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Developer> Developers { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<NewsSentiment> NewsSentiments { get; set; }

    public virtual DbSet<PreRelelaseMetric> PreRelelaseMetrics { get; set; }

    public virtual DbSet<Prediction> Predictions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=postgres;Username=postgres;Password=123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Developer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Developer_pkey");

            entity.ToTable("Developer");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AvgMetacriticLast3)
                .HasPrecision(4, 1)
                .HasColumnName("Avg_Metacritic_Last_3");
            entity.Property(e => e.GameCount).HasColumnName("game_count");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Game_pkey");

            entity.ToTable("Game");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.BudgetEstimate)
                .HasPrecision(6, 2)
                .HasColumnName("budget_estimate");
            entity.Property(e => e.DeveloperId).HasColumnName("developer_id");
            entity.Property(e => e.Genre)
                .HasMaxLength(100)
                .HasColumnName("genre");
            entity.Property(e => e.IsReleased).HasColumnName("is_released");
            entity.Property(e => e.MetacriticScore).HasColumnName("metacritic_score");
            entity.Property(e => e.Platforms)
                .HasMaxLength(150)
                .HasColumnName("platforms");
            entity.Property(e => e.Releasedate).HasColumnName("releasedate");
            entity.Property(e => e.SteamAppId).HasColumnName("steam_app_Id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.TrailerYoutubeId)
                .HasMaxLength(20)
                .HasColumnName("trailer_youtube_id");

            entity.HasOne(d => d.Developer).WithMany(p => p.Games)
                .HasForeignKey(d => d.DeveloperId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Game_developer_id_fkey");
        });

        modelBuilder.Entity<NewsSentiment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("NewsSentiment_pkey");

            entity.ToTable("NewsSentiment");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.Keywords).HasColumnName("keywords");
            entity.Property(e => e.PublishedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("published_at");
            entity.Property(e => e.Relevance)
                .HasPrecision(3, 2)
                .HasColumnName("relevance");
            entity.Property(e => e.SentimentScore)
                .HasPrecision(3, 2)
                .HasColumnName("sentiment_score");
            entity.Property(e => e.Source)
                .HasMaxLength(100)
                .HasColumnName("source");

            entity.HasOne(d => d.Game).WithMany(p => p.NewsSentiments)
                .HasForeignKey(d => d.GameId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("NewsSentiment_game_id_fkey");
        });

        modelBuilder.Entity<PreRelelaseMetric>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PreRelelaseMetric_pkey");

            entity.ToTable("PreRelelaseMetric");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.RedditMentions).HasColumnName("reddit_mentions");
            entity.Property(e => e.Timestamp)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("timestamp");
            entity.Property(e => e.TwitchViewerAvg).HasColumnName("twitch_viewer_avg");
            entity.Property(e => e.WishlistCount).HasColumnName("wishlist_count");
            entity.Property(e => e.YoutubeTrailerViews).HasColumnName("youtube_trailer_views");

            entity.HasOne(d => d.Game).WithMany(p => p.PreRelelaseMetrics)
                .HasForeignKey(d => d.GameId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PreRelelaseMetric_game_id_fkey");
        });

        modelBuilder.Entity<Prediction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Prediction_pkey");

            entity.ToTable("Prediction");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Arguments).HasColumnName("arguments");
            entity.Property(e => e.Confidence)
                .HasPrecision(3, 2)
                .HasColumnName("confidence");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.PredictedMetacritic)
                .HasPrecision(4, 1)
                .HasColumnName("predicted_metacritic");
            entity.Property(e => e.RiskLevel)
                .HasMaxLength(20)
                .HasColumnName("risk_level");
            entity.Property(e => e.SalesClas)
                .HasMaxLength(50)
                .HasColumnName("sales_clas");
            entity.Property(e => e.Verified).HasColumnName("verified");

            entity.HasOne(d => d.Game).WithMany(p => p.Predictions)
                .HasForeignKey(d => d.GameId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Prediction_game_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
