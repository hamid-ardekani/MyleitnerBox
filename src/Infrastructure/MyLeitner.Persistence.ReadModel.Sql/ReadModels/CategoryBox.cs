using System;
using System.Collections.Generic;

namespace MyLeitner.Persistence.ReadModel.Sql.ReadModels;

public partial class CategoryBox
{
    public long Id { get; set; }

    public short Order { get; set; }

    public long CategoryId { get; set; }

    public long BoxId { get; set; }

    public virtual Box Box { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;
}
