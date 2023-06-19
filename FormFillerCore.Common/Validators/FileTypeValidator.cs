using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.
using Microsoft.AspNetCore.Http;

namespace FormFillerCore.Common.Validators
{
    public class FileTypeValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var file = value as IFormFile;
            //using (var pdf = )

            if (file == null)
            {
                return false;

            }

            if (file.ContentType == "application/pdf")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
