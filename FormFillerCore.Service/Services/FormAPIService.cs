using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FormFillerCore.Common.Models;
using FormFillerCore.Repository.Interfaces;
using FormFillerCore.Repository.RepModels;
using FormFillerCore.Service.Interfaces;
using iText.Kernel.Geom;
using System.Xml;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using Newtonsoft.Json;
using iText.Kernel.Font;
using iText.Layout.Borders;
using iText.Forms.Fields;
using iText.Forms;
using System.Reflection.PortableExecutable;

namespace FormFillerCore.Service.Services
{
    public class FormAPIService : IFormAPIService
    {
        private readonly IDataMapItemRepository _DataMapRepository;
        private readonly IFormsService _formsService;
        private readonly IDataTypeRepository _dataTypeRepository;

        public FormAPIService(IDataMapItemRepository DataMapRepository, IFormsService formsService, IDataTypeRepository dataTypeRepository)
        {
            _DataMapRepository = DataMapRepository;
            _formsService = formsService;
            _dataTypeRepository = dataTypeRepository;
        }

        public async Task<Dictionary<string, object>> GetDataSchema(string fname, string dtype)
        {
            List<string> ditems = await _DataMapRepository.FormDataObjectsbyName(fname, dtype, false);

            Dictionary<string, object> js = new Dictionary<string, object>();

            foreach (string item in ditems)
            {
                js.Add(item, item);
            }

            List<string> ritems = await _DataMapRepository.FormDataObjectsbyName(fname, dtype, true);
            List<Dictionary<string, object>> rditems = new List<Dictionary<string, object>>();
            Dictionary<string, object> citem = new Dictionary<string, object>();
            foreach (string item in ritems)
            {
                List<string> cobjects = await _DataMapRepository.ChildDataObjectsByParentName(item, fname, dtype);

                rditems = new List<Dictionary<string, object>>();

                citem = new Dictionary<string, object>();

                foreach (string child in cobjects)
                {
                    citem.Add(child, child);
                }
                rditems.Add(citem);

                js.Add(item, rditems);

            }

            return js;
        }

        public string XmlConvert(byte[] xdoc)
        {
            string xml = Encoding.UTF8.GetString(xdoc, 0, xdoc.Length);

            XmlDocument xmdoc = new XmlDocument();

            xmdoc.LoadXml(xml);

            string jsonc = JsonConvert.SerializeXmlNode(xmdoc);

            return jsonc;
        }
        public async Task<string> XmlConvertAsync(byte[] xdoc)
        {
            Task<string> xmlTask = new Task<string>(() => XmlConvert(xdoc));

            xmlTask.Start();

            return await xmlTask;
        }


        public byte[] BuildForm(Dictionary<string, object> values, string title)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Style qstyle = new Style();
                Style astyle = new Style();
                Style hstyle = new Style();
                PdfFont qfont = PdfFontFactory.CreateFont(StandardFonts.COURIER_BOLD);
                PdfFont afont = PdfFontFactory.CreateFont(StandardFonts.COURIER);
   

                qstyle.SetFont(qfont).SetFontSize(14);
                astyle.SetFont(afont).SetFontSize(12);
                hstyle.SetFont(qfont).SetFontSize(22);

                using (PdfWriter writer = new PdfWriter(ms))
                {

                    var pdf = new PdfDocument(writer);

                    var doc = new Document(pdf);

                    Table table = new Table(1);

                    table.SetWidth(UnitValue.CreatePercentValue(100));

                    Paragraph para = new Paragraph(new Text(title));

                    para.AddStyle(hstyle);

                    Cell tcell = new Cell();

                    tcell.Add(para);

                    tcell.SetHorizontalAlignment(HorizontalAlignment.CENTER);

                    tcell.SetBorder(Border.NO_BORDER);

                    tcell.SetPaddingBottom(40);

                    table.AddCell(tcell);

                    foreach (KeyValuePair<string, object> kvp in values)
                    {
                        Paragraph kphr = new Paragraph(new Text(kvp.Key)).AddStyle(qstyle);
                        Paragraph aphr = new Paragraph(new Text(kvp.Value.ToString())).AddStyle(astyle);

                        Cell kcell = new Cell();

                        kcell.Add(kphr);

                        Cell acell = new Cell();

                        acell.Add(aphr);

                        //kcell.UseVariableBorders = true;

                        kcell.SetBorder(Border.NO_BORDER);
                        acell.SetBorder(Border.NO_BORDER);

                        acell.SetPaddingBottom(20);

                        table.AddCell(kcell);
                        table.AddCell(acell);
                    }

                    doc.Add(table);
                    doc.Close();
                }
                return ms.ToArray();
            }
        }
        public async Task<byte[]> BuildFormAsync(Dictionary<string, object> values, string title)
        {
            Task<byte[]> bfTask = new Task<byte[]>(() => BuildForm(values,title));

            bfTask.Start();

            return await bfTask;
        }
        public async Task<byte[]> FillForm(string fname, Dictionary<string, object> values)
        {
            FormModel form = await _formsService.FormByName(fname);

            return await StampPDFFormAsync(form, values);

            //Git test
        }

        public async Task<byte[]> FillForm(string fname, Dictionary<string, object> values, string OptReplace)
        {
            FormModel form = await _formsService.FormByName(fname);

            FormDataType fdt;

            fdt = _dataTypeRepository.DataTypesByForm((int)form.fid).Result.First();

            if (fdt.DataType != "HTMLSTRING")
            {
                return await StampPDFFormAsync(form, values);
            }
            else
            {
                var stampedEmail = await StampEMAILFormAsync(OptReplace, values);

                return Encoding.ASCII.GetBytes(stampedEmail);
            }
        }

        private string StampEMAILForm(string strEmail, Dictionary<string, object> values)
        {
            string strReplacedEmail;

            strReplacedEmail = strEmail;

            foreach (KeyValuePair<string, object> kvp in values)
            {
                strReplacedEmail = strReplacedEmail.Replace(kvp.Key, kvp.Value.ToString());
            }

            return strReplacedEmail;
        }

        private async Task<string> StampEMAILFormAsync(string strEmail, Dictionary<string, object> values)
        {
            Task<string> eTask = new Task<string>(() => StampEMAILForm(strEmail, values));

            eTask.Start();

            return await eTask;
        }

        private byte[] StampPDFForm(FormModel frm, Dictionary<string, object> values)
        {
            bool repeats = false;
            string js = "";
            int num = 0;

            foreach (KeyValuePair<string, object> kvp in values)
            {
                if (kvp.Value is IList)
                {
                    repeats = true;
                }
            }

            if (repeats == true)
            {
                using (MemoryStream repms = new MemoryStream())
                {
                    PdfDocument newDoc = new PdfDocument(new PdfWriter(repms));

                    int pages = TotalRepeatPages(values, frm);
                    for (int i = 0; i <= pages - 1; i++)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            MemoryStream readFrm = new MemoryStream(frm.Form);
                            
                            using (PdfReader read = new PdfReader(readFrm))
                            {
                                PdfDocument pdf = new PdfDocument(read, new PdfWriter(ms));

                                PdfAcroForm stamper = PdfAcroForm.GetAcroForm(pdf, false);

                                foreach (KeyValuePair<string, object> kvp in values)
                                {

                                    if (kvp.Value is IList)
                                    {
                                        if (kvp.Value.GetType() == typeof(List<string>))
                                        {
                                            List<string> tvals = new List<string>();

                                            tvals = (List<string>)kvp.Value;


                                            if (tvals.Count >= i + 1)
                                            {
                                                //if(Int32.TryParse(tvals[i],out num)){
                                                //js = js + string.Format("var f = this.getField('{0}'); f.value = 1 * {1};", kvp.Key,tvals[i]);


                                                //}

                                                stamper.GetField(kvp.Key).SetValue(tvals[i]);
                                            }


                                        }
                                        else
                                        {
                                            List<Dictionary<string, object>> vals = (List<Dictionary<string, object>>)kvp.Value;

                                            int r = 0;
                                            int tItems = (int)_DataMapRepository.GetItemCountbyName(kvp.Key).Result;
                                            while (r <= tItems - 1 && vals.Count > 0)
                                            {
                                                foreach (KeyValuePair<string, object> formitems in vals[0])
                                                {
                                                    if (formitems.Key.ToString() != formitems.Value.ToString())
                                                    {

                                                        //if(Int32.TryParse(formitems.Value.ToString(),out num)){
                                                        //js = js + string.Format("var f = this.getField('{0}'); f.value = 1 * {1};", formitems.Key + Convert.ToString(r + 1),formitems.Value);
                                                        //}

                                                        stamper.GetField(formitems.Key.ToString() + Convert.ToString(r + 1)).SetValue(formitems.Value.ToString());
                                                    }
                                                }

                                                vals.Remove(vals[0]);

                                                r = r + 1;
                                            }
                                        }
                                    }
                                    else
                                    {

                                        //if (Int32.TryParse(kvp.Value.ToString(), out num))
                                        //{
                                        //js = js + string.Format("var f = this.getField('{0}'); f.value = 1 * f.value;", kvp.Key,kvp.Value);
                                        //}
                                        stamper.GetField(kvp.Key).SetValue(kvp.Value.ToString());
                                    }
                                }

                                //repeatstamper.JavaScript = js;
                                stamper.FlattenFields();
                                pdf.Close();
                            }

                            PdfDocument tempPdf = new PdfDocument(new PdfReader(ms));

                            tempPdf.CopyPagesTo(1, 1, newDoc);

                            tempPdf.Close();
                        }
                    }
                    newDoc.Close();
                    return repms.ToArray();
                }
            }
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (MemoryStream cfrm = new MemoryStream(frm.Form)) {   
                        using (var reader = new PdfReader(cfrm))
                        {
                            PdfDocument pdf = new PdfDocument(reader,new PdfWriter(ms));

                            PdfAcroForm stamper = PdfAcroForm.GetAcroForm(pdf,false);
                            foreach (KeyValuePair<string, PdfFormField> fields in stamper.GetAllFormFields())
                            {
                                if (values.ContainsKey(fields.Key.ToString()))
                                {

                                    if (values[fields.Key.ToString()].ToString() != fields.Key.ToString())
                                    {
                                        stamper.GetField(fields.Key.ToString()).SetValue(values[fields.Key.ToString()].ToString());
                                    }
                                }
                            }

                            stamper.FlattenFields();
                            pdf.Close();

                        }
                    }
                    return ms.ToArray();
                }
            }
        }

        private async Task<byte[]> StampPDFFormAsync(FormModel frm, Dictionary<string, object> values)
        {
            Task<byte[]> pdfStamp = new Task<byte[]>(() => StampPDFForm(frm, values));

            pdfStamp.Start();

            return await pdfStamp;
        }

        private int TotalRepeatPages(Dictionary<string, object> values, FormModel frm)
        {

            int pages = 1;
            foreach (KeyValuePair<string, object> kvp in values)
            {
                int tItems;

                if (kvp.Value is IList)
                {
                    try
                    {
                        List<Dictionary<string, object>> vals = (List<Dictionary<string, object>>)kvp.Value;

                        tItems = _DataMapRepository.GetItemCountbyName(kvp.Key).Result;

                        int oitems = (vals.Count > tItems) ? (vals.Count % tItems) : 0;

                        int eitems = ((vals.Count / tItems) > 1) ? (vals.Count / tItems) : 1;

                        int expectedp = (oitems > 0) ? eitems + 1 : eitems;

                        if (pages < expectedp)
                        {
                            pages = expectedp;
                        }
                    }
                    catch (InvalidCastException e)
                    {

                    }
                }
            }

            return pages;
        }


    }
}
