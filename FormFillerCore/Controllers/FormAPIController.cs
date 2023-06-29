using FormFillerCore.Common.Models;
using FormFillerCore.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Web;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web.Resource;

namespace FormFillerCore.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [RequiredScope("default")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FormAPIController : ControllerBase
    {
        private readonly IFormsService _formService;
        private readonly IDataMapService _datamapService;
        private readonly IFormAPIService _formApiService;
        public FormAPIController(IFormsService formService, IDataMapService datamapService, IFormAPIService formApiService)
        {
            _formService = formService;
            _datamapService = datamapService;
            _formApiService = formApiService;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> FillForm([FromQuery] string Form, [FromQuery] string DataType, [FromBody] Dictionary<string, object> JObject)
        {
            string str = Convert.ToString(JObject["JObject"]);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            Dictionary<string, object> values = JsonSerializer.Deserialize<Dictionary<string, object>>(str);
            FormModel frm = await _formService.FormByName(Form);

            string strHtml = "";

            if (DataType == "HTMLSTRING")
            {
                strHtml = values["HTML"].ToString();

                values.Remove("HTML");
            }


            Dictionary<string, object> dmap = await _datamapService.FillMap(Form, DataType, values);


            if (DataType != "HTMLSTRING")
            {
                byte[] stampedfile = await _formApiService.FillForm(Form, dmap);

                MemoryStream fstream = new MemoryStream();
                fstream.Write(stampedfile, 0, stampedfile.Length);
                fstream.Position = 0;

                response.Content = new StreamContent(fstream);
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = Form + ".pdf";
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            }
            else
            {
                var frmEmail = await _formApiService.FillForm(Form, values, strHtml);

                string emailstamp = Encoding.ASCII.GetString(frmEmail);

                var data = new Dictionary<string, string>();
                data.Add("EMAILHTML", emailstamp);

                var resp = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

                response.Content = resp;
            }

            return response;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> EmailForm([FromQuery] string Form, [FromQuery] string DataType, [FromBody] EmailModel mod)
        {
            SmtpClient mailclient = new SmtpClient("outlook.office365.com",587); //new SmtpClient("outlook.office365.com",587);

            NetworkCredential credential = new NetworkCredential("cbflow@cbsd.org", "C8fl34");

            mailclient.Credentials = credential;

            mailclient.EnableSsl = true;

            mailclient.DeliveryMethod = SmtpDeliveryMethod.Network;

            string str = JsonSerializer.Serialize(mod.JObject);

            Dictionary<string, object> values = JsonSerializer.Deserialize<Dictionary<string, object>>(str);

            //string em = Convert.ToString(mod.EmailInfo);
            //Dictionary<string, object> emvalues = jser.Deserialize<Dictionary<string, object>>(em);

            FormModel frm = await _formService.FormByName(Form);

            Dictionary<string, object> dmap = await _datamapService.FillMap(Form, DataType, values);


            byte[] stampedfile = await _formApiService.FillForm(Form, dmap);

            MemoryStream fstream = new MemoryStream();
            fstream.Write(stampedfile, 0, stampedfile.Length);
            fstream.Position = 0;

            Attachment att = new Attachment(fstream, mod.EmailInfo["AttachmentName"].ToString());

            MailMessage mailm = new MailMessage();

            mailm.From = new MailAddress("cbflow@cbsd.org", string.Empty, System.Text.Encoding.UTF8);

            mailm.To.Add(mod.EmailInfo["Address"].ToString());

            mailm.Attachments.Add(att);

            mailm.Subject = mod.EmailInfo["Subject"].ToString();

            mailm.Body = mod.EmailInfo["Body"].ToString();

            //mailclient.UseDefaultCredentials = false;
            //mailclient.Credentials = new NetworkCredential("cbsdflow@cbsd.org", "C8fl34");
            //mailclient.EnableSsl = true;

            try
            {
                mailclient.Send(mailm);

                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

                return response;
            }
            catch (Exception ex)
            {

                HttpResponseMessage rs = new HttpResponseMessage(HttpStatusCode.InternalServerError);

                rs.ReasonPhrase = ex.Message;

                return rs;
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> CreateForm([FromBody] CreateModel mod)
        {
            string dstr = JsonSerializer.Serialize(mod.JObject);
            string fstr = JsonSerializer.Serialize(mod.FormInfo);
            Dictionary<string, object> values = JsonSerializer.Deserialize<Dictionary<string, object>>(dstr);
            Dictionary<string, object> forminfo = JsonSerializer.Deserialize<Dictionary<string, object>>(fstr);

            object ftitle;
            object fname;

            forminfo.TryGetValue("Title", out ftitle);
            forminfo.TryGetValue("Name", out fname);

            byte[] builtfile = await _formApiService.BuildFormAsync(values, ftitle.ToString());

            MemoryStream fstream = new MemoryStream();
            fstream.Write(builtfile, 0, builtfile.Length);
            fstream.Position = 0;

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(fstream);
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = fname.ToString() + ".pdf";
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

            return response;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> CreateEmailForm([FromBody] CreateEmailModel mod)
        {
            SmtpClient mailclient = new SmtpClient();//new SmtpClient("smtp.office365.com", 587);
            string dstr = JsonSerializer.Serialize(mod.JObject);
            string fstr = JsonSerializer.Serialize(mod.FormInfo);
            Dictionary<string, object> values = JsonSerializer.Deserialize<Dictionary<string, object>>(dstr);
            Dictionary<string, object> forminfo = JsonSerializer.Deserialize<Dictionary<string, object>>(fstr);

            object ftitle;
            object fname;

            forminfo.TryGetValue("Title", out ftitle);
            forminfo.TryGetValue("Name", out fname);

            byte[] builtfile = await _formApiService.BuildFormAsync(values, ftitle.ToString());

            MemoryStream fstream = new MemoryStream();
            fstream.Write(builtfile, 0, builtfile.Length);
            fstream.Position = 0;

            Attachment att = new Attachment(fstream, mod.EmailInfo["AttachmentName"].ToString());

            MailMessage mailm = new MailMessage();

            mailm.From = new MailAddress("cbflow@cbsd.org", string.Empty, System.Text.Encoding.UTF8);

            mailm.To.Add(mod.EmailInfo["Address"].ToString());

            mailm.Attachments.Add(att);

            mailm.Subject = mod.EmailInfo["Subject"].ToString();

            mailm.Body = mod.EmailInfo["Body"].ToString();

            //mailclient.UseDefaultCredentials = false;
            //mailclient.Credentials = new NetworkCredential("cbflow@cbsd.org", "C8fl34");
            //mailclient.EnableSsl = true;

            try
            {
                mailclient.Send(mailm);

                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

                return response;
            }
            catch (Exception ex)
            {

                HttpResponseMessage rs = new HttpResponseMessage(HttpStatusCode.InternalServerError);

                rs.ReasonPhrase = ex.Message;

                return rs;
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> ConvertXml([FromBody] string file)
        {
            byte[] xmld = Convert.FromBase64String(file);

            var xmlCon = await _formApiService.XmlConvertAsync(xmld);

            StringContent jresponse = new StringContent(xmlCon);

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = (jresponse);
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            return response;
        }
        [HttpGet]
        public async Task<Dictionary<string, object>> GetDataSchema([FromQuery] string Form, [FromQuery] string DataType)
        {
            return await _formApiService.GetDataSchema(Form, DataType);
        }
    }
}
