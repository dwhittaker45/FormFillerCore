using System;
using System.Collections.Generic;

namespace FormFillerCore.Repository.RepModels;

public partial class FormDataMap
{
    public int DataMapId { get; set; }

    public int FormDataTypeId { get; set; }

    public string FormObject { get; set; } = null!;

    public string DataObject { get; set; } = null!;

    public bool ChildFormObjects { get; set; }

    public string? CheckValue { get; set; }

    public bool? Repeatable { get; set; }

    public int? ItemCount { get; set; }

    public bool Calculated { get; set; }

    public string? Expression { get; set; }

    public string? DataFormat { get; set; }

    public virtual ICollection<DataMapChildObject> DataMapChildObjects { get; set; } = new List<DataMapChildObject>();

    public virtual FormDataType FormDataType { get; set; } = null!;
}
