using System;
using System.Collections.Generic;

namespace FormFillerCore.Repository.RepModels;

public partial class FormDataType
{
    public int FormDataTypeId { get; set; }

    public int FormId { get; set; }

    public string DataType { get; set; } = null!;

    public virtual Form Form { get; set; } = null!;

    public virtual ICollection<FormDataMap> FormDataMaps { get; set; } = new List<FormDataMap>();

    public virtual ICollection<FormRequest> FormRequests { get; set; } = new List<FormRequest>();
}
