using System;
using System.Collections.Generic;

namespace FormFillerCore.Repository.RepModels;

public partial class DataMapChildObject
{
    public int ChildObjectId { get; set; }

    public int ParentObject { get; set; }

    public string FormObject { get; set; } = null!;

    public string DataObject { get; set; } = null!;

    public string? CheckValue { get; set; }

    public bool Calculated { get; set; }

    public string? Expression { get; set; }

    public string? DataFormat { get; set; }

    public virtual FormDataMap ParentObjectNavigation { get; set; } = null!;
}
