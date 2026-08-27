using System;
using System.Collections.Generic;

namespace GamePredictor.Domain.Entities;

public partial class Developers
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal AvgMetacriticLast3 { get; set; }

    public int GamesCount { get; set; }

    public ICollection<Game> Games { get; set; } = new List<Game>();
}
