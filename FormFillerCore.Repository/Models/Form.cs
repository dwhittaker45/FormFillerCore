using System;
using System.Collections.Generic;

namespace FormFillerCore.Repository.RepModels;

public partial class Form
{
    public int Fid { get; set; }

    public string FormName { get; set; } = null!;

    public string FileType { get; set; } = null!;

    public byte[] Form1 { get; set; } = null!;

    public bool Active { get; set; }

    public virtual ICollection<FormDataType> FormDataTypes { get; set; } = new List<FormDataType>();

    public virtual ICollection<FormRequest> FormRequests { get; set; } = new List<FormRequest>();
}
