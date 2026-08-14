using System;
using System.Collections.Generic;

namespace GamePredictor.Domain.Entities;

public partial class Developer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal AvgMetacriticLast3 { get; set; }

    public int GameCount { get; set; }

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
