using FormFillerCore.Common.Enumerators;
using FormFillerCore.Common.Validators;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormFillerCore.Common.Models
{
    public class FormModel
    {
        public int? fid { get; set; }

        public byte[] Form { get; set; }

        [Required]
        public string FormName { get; set; }

        [Required]
        public FileType? FileType { get; set; }

        public bool Active { get; set; }

        [FileTypeValidation(ErrorMessage = "You must select a PDF File")]
        public IFormFile TempFile { get; set; }

    }
}
