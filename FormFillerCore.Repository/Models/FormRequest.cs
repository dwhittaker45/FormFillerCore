using System;
using System.Collections.Generic;

namespace FormFillerCore.Repository.RepModels;

public partial class FormRequest
{
    public int ReqId { get; set; }

    public int FormId { get; set; }

    public int? FormDataType { get; set; }

    public string? RequestObject { get; set; }

    public string RequestStatus { get; set; } = null!;

    public DateTime RequestDateTime { get; set; }

    public virtual Form Form { get; set; } = null!;

    public virtual FormDataType? FormDataTypeNavigation { get; set; }
}
