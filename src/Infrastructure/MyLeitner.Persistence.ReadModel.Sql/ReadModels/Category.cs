using System;
using System.Collections.Generic;

namespace MyLeitner.Persistence.ReadModel.Sql.ReadModels;

public partial class Category
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public short Order { get; set; }

    public long UserId { get; set; }

    public virtual ICollection<CategoryBox> CategoryBoxes { get; set; } = new List<CategoryBox>();
}
