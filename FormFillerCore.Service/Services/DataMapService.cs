using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using iText.Forms;
using FormFillerCore.Common.Models;
using FormFillerCore.Repository.Interfaces;
using FormFillerCore.Repository.RepModels;
using FormFillerCore.Service.Interfaces;
using iText.Kernel.Pdf;
using System.Collections;
using System.Text.RegularExpressions;
using iText.Forms.Fields;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FormFillerCore.Service.Services
{
    public class DataMapService : IDataMapService
    {
        private readonly IDataMapItemRepository _DataMapRepository;
        private readonly IFormsService _formsService;
        private readonly IMapper _mapper;

        public DataMapService(IDataMapItemRepository DataMapRepository, IFormsService formsService, IMapper mapper)
        {
            _DataMapRepository = DataMapRepository;
            _formsService = formsService;
            _mapper = mapper;
        }

        public async Task<List<DataMapItemModel>> GetFormDataMap(int did)
        {
            var data = await _DataMapRepository.DataMapItemsbyIDAsync(did);
            var mitems = _mapper.Map<List<DataMapItemModel>>(data);
            return mitems;
        }

        public List<string> GetFormFields(int fid)
        {
            List<string> results = new List<string>();

            using (MemoryStream ms = new MemoryStream(_formsService.GetFile(fid).Result)) {
                using (var pdfFile = new PdfReader(ms))
                {
                    PdfDocument pdfDoc = new PdfDocument(pdfFile);

                    PdfAcroForm pdfForm = PdfAcroForm.GetAcroForm(pdfDoc, false);

                    //AcroFields fields = pdfFile.AcroFields;
                    foreach (KeyValuePair<string, PdfFormField> kvp in pdfForm.GetAllFormFields())
                    {
                        results.Add(kvp.Key.ToString());
                    }
                }
            }

            return results;
        }

        public async Task<List<string>> GetFormFieldsAsync(int fid)
        {
            Task<List<string>> gffTask = new Task<List<string>>(() => GetFormFields(fid));

            gffTask.Start();

            return await gffTask;
        }

        public async Task AddMapItem(DataMapItemModel dm)
        {
            await _DataMapRepository.AddDataMapItems(_mapper.Map<FormDataMap>(dm));
        }

        public async Task UpdateMapItem(DataMapItemModel dm)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteMapItem(int id)
        {
            await _DataMapRepository.DeleteDataMapItem(id);
        }
        public async Task<List<DataMapItemModel>> GetFormDataMapByName(string fname, string dtype)
        {
            var dmItems = await _DataMapRepository.DataMapItemsByNameAsync(fname, dtype);
            return _mapper.Map<List<DataMapItemModel>>(dmItems);
        }

        public async Task<DataMapItemModel> GetMapItem(int id)
        {
            var dmitem = await _DataMapRepository.DataMapItemByID(id);

            var item = _mapper.Map<DataMapItemModel>(dmitem);

            return item;
        }

        public async Task<int> GetFormIDFromMapItem(int id)
        {
            return await _DataMapRepository.FormIDfromDataIDAsync(id);
        }


        public async Task AutoMapItems(int did, int fid)
        {
            List<string> fields = await GetFormFieldsAsync(fid);
            DataMapItemModel dm = new DataMapItemModel();

            foreach (string s in fields)
            {
                dm.FormDataTypeID = did;
                dm.DataObject = s;
                dm.FormObject = s;

                await AddMapItem(dm);
            }
        }
        public async Task<Dictionary<string, object>> FillMap(string fname, string dtype, Dictionary<string, object> values)
        {
            string val;

            IEnumerable<DataMapItemModel> Objects = _mapper.Map<IEnumerable<DataMapItemModel>>(_DataMapRepository.DataMapItemsByNameAsync(fname, dtype).Result.ToList()); ;

            List<DataMapItemModel> ParentObjects = Objects.Where(x => x.ChildFormObjects == true).ToList();
            List<DataMapItemModel> CheckObjects = Objects.Where(x => x.CheckValue != null && x.CheckValue != "").ToList();
            List<DataMapItemModel> CalcObjects = Objects.Where(x => x.Calculated == true).ToList();
            List<DataMapItemModel> RegularObjects = Objects.Where(x => x.ChildFormObjects == false && (x.CheckValue == null | x.CheckValue == "") && x.Calculated == false).ToList();

            IEnumerable<ChildMapItemModel> ChildObjects = new List<ChildMapItemModel>();
            List<ChildMapItemModel> ChildCheckObjects = new List<ChildMapItemModel>();
            List<ChildMapItemModel> ChildRegularObjects = new List<ChildMapItemModel>();

            Dictionary<string, object> datamap = new Dictionary<string, object>();

            foreach (DataMapItemModel ditem in ParentObjects)
            {

                ChildObjects = _mapper.Map<IEnumerable<ChildMapItemModel>>(_DataMapRepository.ChildObjectsByParent(Convert.ToInt32(ditem.DataMapID)).Result.ToList());

                ChildCheckObjects = ChildObjects.Where(x => x.CheckValue != null).ToList();

                ChildRegularObjects = ChildObjects.Where(x => x.CheckValue == null && x.Calculated == false).ToList();

                if (ditem.Repeatable == true)
                {
                    List<Dictionary<string, object>> repeatmap = new List<Dictionary<string, object>>();


                    string arList = JsonSerializer.Serialize(values[ditem.DataObject.ToString()]);

                    ArrayList vals = JsonSerializer.Deserialize<ArrayList>(arList);

                    //object[] vals = (object[])arr;

                    List<Dictionary<string, object>> kvp = new List<Dictionary<string, object>>();

                    for (int a = 0; a <= vals.Count - 1; a++)
                    {
                    
                        string arVal = JsonSerializer.Serialize(vals[a]);

                        kvp.Add(JsonSerializer.Deserialize <Dictionary<string, object>>(arVal));
                    }


                    for (int i = 0; i <= kvp.Count() - 1; i++)
                    {
                        Dictionary<string, object> childmap = new Dictionary<string, object>();

                        foreach (ChildMapItemModel citem in ChildCheckObjects)
                        {
                            if (kvp[i].ContainsKey(citem.DataObject))
                            {
                                val = kvp[i][citem.DataObject].ToString();

                                if (val.Contains(citem.CheckValue))
                                {
                                    childmap.Add(citem.FormObject, "Yes");
                                }
                                else
                                {
                                    childmap.Add(citem.FormObject, "Off");
                                }
                            }
                        }

                        foreach (ChildMapItemModel citem in ChildRegularObjects)
                        {
                            if (kvp[i].ContainsKey(citem.DataObject))
                            {
                                val = kvp[i][citem.DataObject].ToString();

                                childmap.Add(citem.FormObject, val);
                            }
                        }
                        repeatmap.Add(childmap);
                    }

                    datamap.Add(ditem.DataObject, repeatmap);
                }
                else
                {
                    Dictionary<string, object> childmap = new Dictionary<string, object>();
                    foreach (ChildMapItemModel citem in ChildCheckObjects)
                    {
                        if (values.ContainsKey(citem.DataObject))
                        {
                            val = values[citem.DataObject].ToString();

                            if (val.Contains(citem.CheckValue))
                            {
                                childmap.Add(citem.FormObject, "Yes");
                            }
                            else
                            {
                                childmap.Add(citem.FormObject, "Off");
                            }
                        }
                    }

                    foreach (ChildMapItemModel citem in ChildRegularObjects)
                    {
                        if (values.ContainsKey(citem.DataObject))
                        {
                            val = values[citem.DataObject].ToString();

                            childmap.Add(citem.FormObject, val);
                        }
                    }

                    datamap.Add(ditem.DataObject, childmap);
                }
            }

            foreach (DataMapItemModel ditem in CheckObjects)
            {
                if (values.ContainsKey(ditem.DataObject))
                {
                    val = values[ditem.DataObject].ToString();

                    if (val.Contains(ditem.CheckValue))
                    {
                        datamap.Add(ditem.FormObject, "Yes");
                    }
                    else
                    {
                        datamap.Add(ditem.FormObject, "Off");
                    }
                }
            }

            foreach (DataMapItemModel ditem in RegularObjects)
            {
                if (values.ContainsKey(ditem.DataObject))
                {
                    val = values[ditem.DataObject].ToString();

                    datamap.Add(ditem.FormObject, val);
                }
            }

            Queue<KeyValuePair<string, object>> exprs = new Queue<KeyValuePair<string, object>>();

            Dictionary<string, string> dformats = new Dictionary<string, string>();

            int did = 0;

            foreach (DataMapItemModel ditem in CalcObjects)
            {
                did = (int)ditem.FormDataTypeID;

                KeyValuePair<string, object> cdic = new KeyValuePair<string, object>(ditem.FormObject, ditem.Expression);

                dformats.Add(ditem.FormObject, ditem.DataFormat);
                exprs.Enqueue(cdic);
            }

            if (exprs.Count > 0)
            {
                FormModel frm = await _formsService.FormByName(fname);

                datamap = await FillCalculatedFieldsAsync(exprs, datamap, frm, did, dformats);
            }

            return datamap;
        }


        public List<ChildMapItemModel> GetChildObjectsByParent(int pid)
        {
            return _mapper.Map<List<ChildMapItemModel>>(_DataMapRepository.ChildObjectsByParent(pid));
        }


        public void AddChildObject(ChildMapItemModel citem)
        {
            _DataMapRepository.AddChildItem(_mapper.Map<DataMapChildObject>(citem));
        }


        public int ItemCountByid(int did)
        {
            throw new NotImplementedException();
        }

        public int ItemCountByName(string dname)
        {
            throw new NotImplementedException();
        }

        public void FillCalculatedFields(Queue<KeyValuePair<string, object>> expressions, ref Dictionary<string, object> dmap, FormModel frm, int dtype, Dictionary<string, string> dformats)
        {
            KeyValuePair<string, object> exp;

            List<KeyValuePair<string, object>> cexpressions = new List<KeyValuePair<string, object>>();
            string bexpression;
            bool wait = false;
            bool write = false;


            while (expressions.Count > 0)
            {
                Dictionary<string, object> replacedvalues = new Dictionary<string, object>();

                List<string> res = new List<string>();

                exp = expressions.Dequeue();

                bexpression = exp.Value.ToString();

                MatchCollection ffields = Regex.Matches(bexpression, "\\{.*?\\}");

                if (ffields.Count > 0)
                {
                    foreach (Match fmatch in ffields)
                    {
                        KeyValuePair<string, object> fobject = ReplaceFormField(fmatch.Value, frm.Form, dtype, dmap);

                        replacedvalues.Add(fobject.Key.ToString(), fobject.Value);
                    }
                }

                Match cexpr = Regex.Match(bexpression, "\\[@");

                if (cexpr.Success)
                {

                    string cust = bexpression.Replace(cexpr.Value, "");

                    Match cname = Regex.Match(cust, "^(\\b\\w+)");

                    if (cname.Success)
                    {

                        List<string> returns = new List<string>();

                        switch (cname.Groups[0].Value)
                        {
                            case "Run":
                                foreach (KeyValuePair<string, object> citem in replacedvalues)
                                {
                                    double total = 0;

                                    if (citem.Value.GetType() == typeof(ChildMapItemModel))
                                    {
                                        ChildMapItemModel runitem = (ChildMapItemModel)citem.Value;

                                        DataMapItemModel parentitem = _mapper.Map<DataMapItemModel>(_DataMapRepository.DataMapItemByID(Convert.ToInt32(runitem.ParentObject)).Result);

                                        int tItems = Convert.ToInt32(_DataMapRepository.GetItemCountbyID((int)runitem.ParentObject).Result);

                                        List<Dictionary<string, object>> runobj = new List<Dictionary<string, object>>();

                                        runobj = (List<Dictionary<string, object>>)dmap[parentitem.DataObject];

                                        if (runobj == null)
                                        {
                                            returns.Add("0");
                                        }
                                        else
                                        {
                                            for (int r = 0; r <= runobj.Count - 1;)
                                            {
                                                total = 0;

                                                for (int i = 0; i <= tItems - 1 && r <= runobj.Count - 1; i++)
                                                {
                                                    double val = Convert.ToDouble(runobj[r][runitem.FormObject]);

                                                    total = total + val;

                                                    r = r + 1;
                                                }




                                                returns.Add(FormatValue(Convert.ToString(total), dformats[exp.Key.ToString()]));
                                            }
                                        }
                                        dmap[exp.Key.ToString()] = returns;
                                    }
                                }
                                break;
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, object> rfields in replacedvalues)
                    {
                        if (Convert.ToString(rfields.Value) == "None")
                        {
                            wait = true;
                        }
                        else if (Convert.ToString(rfields.Value) == "NotMapped")
                        {
                            res.Add(bexpression.Replace(rfields.Key.ToString(), "0"));
                        }
                        else
                        {

                            //wait = false;

                            List<string> rval = new List<string>();

                            if (rfields.Value.GetType() == typeof(List<string>))
                            {
                                rval = (List<string>)rfields.Value;
                                {
                                    for (int i = 0; i <= rval.Count - 1; i++)
                                    {
                                        if (rval.Count > res.Count)
                                        {
                                            res.Add(bexpression.Replace(rfields.Key.ToString(), rval[i]));
                                        }
                                        else
                                        {
                                            res[i] = res[i].Replace(rfields.Key.ToString(), rval[i]);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (res.Count == 0)
                                {
                                    res.Add(bexpression.Replace(rfields.Key.ToString(), rfields.Value.ToString()));
                                }
                                else
                                {
                                    for (int i = 0; i <= res.Count - 1; i++)
                                    {
                                        res[i] = res[i].Replace(rfields.Key.ToString(), rfields.Value.ToString());
                                    }
                                }
                            }
                        }
                    }
                    if (wait)
                    {
                        if (res[0].Contains("{"))
                        {
                            expressions.Enqueue(new KeyValuePair<string, object>(exp.Key.ToString(), bexpression));
                        }
                        else
                        {
                            write = true;
                        }
                    }
                    else
                    {
                        write = true;
                    }

                    if (write)
                    {
                        List<string> values = new List<string>();
                        for (int i = 0; i <= res.Count - 1; i++)
                        {
                            double value = Convert.ToDouble(new DataTable().Compute(res[i], null));


                            values.Add(FormatValue(Convert.ToString(value), dformats[exp.Key.ToString()]));
                        }

                        dmap[exp.Key.ToString()] = values;
                    }
                }
            }
        }

        public async Task<Dictionary<string, object>> FillCalculatedFieldsAsync(Queue<KeyValuePair<string, object>> expressions, Dictionary<string, object> dmap, FormModel frm, int dtype, Dictionary<string, string> dformats)
        {
            Task calcFields = new Task(() => FillCalculatedFields(expressions,ref dmap,frm,dtype,dformats));

            calcFields.Start();

            await calcFields;

            return dmap;
        }

        public KeyValuePair<string, object> ReplaceFormField(string field, byte[] pdfform, int dtype, Dictionary<string, object> dmap)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            Match fmatch = Regex.Match(field, "\\b\\w+:\\w+\\b");

            foreach (Group fgroup in fmatch.Groups)
            {
                string[] farray = fgroup.Value.ToString().Split(':');

                result.Add(farray[0], farray[1]);
            }

            Match nmatch = fmatch.NextMatch();

            if (nmatch.Success)
            {
                foreach (Group fgroup in nmatch.Groups)
                {
                    string[] farray = fgroup.Value.ToString().Split(':');

                    result.Add(farray[0], farray[1]);
                }
            }

            if (result.Keys.Count == 1)
            {
                if (result.Keys.Contains("FormObject"))
                {
                   using (MemoryStream ms = new MemoryStream(pdfform)) { 

                        PdfDocument pdfDoc = new PdfDocument(new PdfReader(ms));

                        PdfAcroForm pForm = PdfAcroForm.GetAcroForm(pdfDoc, true);

                        KeyValuePair<string, object> formval = new KeyValuePair<string, object>(field.ToString(), pForm.GetField(result["FormObject"]).GetDisplayValue().ToString());

                        pdfDoc.Close();

                        return formval;

                    }
                }
            }
            else if (result.Keys.Count == 2)
            {
                if (result.Keys.Contains("ChildObject"))
                {
                    ChildMapItemModel citem = new ChildMapItemModel();

                    citem = _mapper.Map<ChildMapItemModel>(_DataMapRepository.ChildObjectbynames(result["ChildObject"], result["DataObject"], dtype).Result);

                    KeyValuePair<string, object> formval = new KeyValuePair<string, object>(field.ToString(), citem);

                    return formval;
                }
                else if (result.Keys.Contains("FormObject"))
                {
                    //DataMapItemModel ditem = new DataMapItemModel();

                    //ditem = Mapper.Map<DataMapItemModel>(_DataMapRepository.GetDataMapItem(result["DataObject"], result["FormObject"], dtype));

                    if (dmap.ContainsKey(result["FormObject"]))
                    {

                        string mapval = dmap[result["FormObject"].ToString()].ToString();

                        if (dmap[result["FormObject"].ToString()].GetType() == typeof(List<string>))
                        {
                            List<string> cobj = new List<string>();

                            cobj = (List<string>)dmap[result["FormObject"].ToString()];

                            KeyValuePair<string, object> formval = new KeyValuePair<string, object>(field.ToString(), cobj);

                            return formval;
                        }
                        else
                        {
                            if (result["FormObject"].ToString() != mapval)
                            {
                                KeyValuePair<string, object> formval = new KeyValuePair<string, object>(field.ToString(), mapval);

                                return formval;
                            }
                            else
                            {
                                KeyValuePair<string, object> formval = new KeyValuePair<string, object>(field.ToString(), "None");

                                return formval;
                            }
                        }
                    }
                    else
                    {
                        KeyValuePair<string, object> formval = new KeyValuePair<string, object>(field.ToString(), "None");

                        return formval;
                    }
                }
            }

            KeyValuePair<string, object> noval = new KeyValuePair<string, object>(field.ToString(), "None");

            return noval;
        }

        public async Task<KeyValuePair<string, object>> ReplaceFormFieldAsync(string field, byte[] pdfform, int dtype, Dictionary<string, object> dmap)
        {
            Task<KeyValuePair<string, object>> kvp = new Task<KeyValuePair<string, object>>(() => ReplaceFormField(field, pdfform, dtype, dmap));

            kvp.Start();

            return await kvp;
        }

        public string FormatValue(string val, string format)
        {
            Match fmMatch = Regex.Match(format, "\\b\\w+:\\w+\\b");

            if (fmMatch.Success)
            {
                string[] decp = format.Split(':');

                int dec = Convert.ToInt32(decp[1]);

                string decf = "{0:0.";

                for (int i = 0; i <= dec - 1; i++)
                {
                    decf = decf + "0";
                }

                decf = decf + "}";

                return string.Format(decf, Convert.ToDouble(val));
            }
            else
            {
                if (format == "Money")
                {
                    return string.Format("{0:#.00}", Convert.ToDecimal(val));
                }
                else
                {
                    return val;
                }
            }
        }
    }
}
