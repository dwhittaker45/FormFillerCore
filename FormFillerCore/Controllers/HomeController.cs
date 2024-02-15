using FormFillerCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FormFillerCore.Common.Models;
using FormFillerCore.Service.Interfaces;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FormFillerCore.Common.Enumerators;

namespace FormFillerCore.Controllers
{
    [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
    public class HomeController : Controller
    {
        private readonly IFormsService _formService;
        private readonly IDataMapService _datamapService;
        public HomeController(IFormsService formService, IDataMapService datamapService)
        {
            _formService = formService;
            _datamapService = datamapService;
        }
        //
        // GET: /Forms/
        public async Task<IActionResult> List()
        {
            var model = await _formService.AllForms();
            return View("List", model);
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Delete(int fid)
        {
            _formService.FormDelete(fid);
            return RedirectToAction("List");
        }
        public async Task<ActionResult> ViewForm(int fid)
        {
            byte[] form = await _formService.GetFile(fid);
            MemoryStream fstream = new MemoryStream();
            fstream.Write(form, 0, form.Length);
            fstream.Position = 0;
            string ftype = await _formService.FileTypeByID(fid);

            if (ftype == "PDF")
            {
                return new FileStreamResult(fstream, "application/pdf");
            }
            else
            {
                return new FileStreamResult(fstream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _formService.FullFormInfo(id);
            ViewBag.DataID = id;
            ViewBag.DataMap = model.DataMap;

            var fdtype = await _formService.FullFormInfo(id);

            List<string> ffields = new List<string>();

            if (fdtype.DataType.DataType.ToString() != "HTMLSTRING"){ 
                ffields = _datamapService.GetFormFields(id);
            }
            ViewBag.DataType = Convert.ToInt32(fdtype.DataType.FormDataTypeID);

            ViewBag.FormFields = ffields;

            return View("Edit", model);
        }
        [HttpPost]
        public async Task<IActionResult> FormEdit(int id)
        {
            var model = await _formService.FullFormInfo(id);
            ViewBag.DataID = id;
            ViewBag.DataMap = model.DataMap;

            return View("Edit", model);

        }

        public async Task<PartialViewResult> CreateDataMapItem(int id)
        {
            var pmodel = new FormFillerCore.Common.Models.DataMapItemModel();

            var fmodel = await _formService.FullFormInfo(id);

            ViewBag.DataType = Convert.ToInt32(fmodel.DataType.FormDataTypeID);
            ViewBag.DataMap = fmodel.DataMap;

            return PartialView("CreateDataMapItem", pmodel);
        }
        public async Task<PartialViewResult> LoadEdit(int formid)
        {
            int fid;
            if (formid == 0)
            {
                fid = Convert.ToInt32(Request.RouteValues["id"].ToString());
            }
            else
            {
                fid = formid;
            }
            List<string> ffields = _datamapService.GetFormFields(fid);
            ffields.Add("Dynamic");
            ffields.Add("Repeatable");
            ffields.Add("Calculated");
            DataMapItemModel dmi = new DataMapItemModel();
            ViewBag.FormFields = ffields;
            ViewBag.FormID = fid;

            var fdtype = await _formService.FullFormInfo(fid);

            ViewBag.DataType = Convert.ToInt32(fdtype.DataType.FormDataTypeID);
            ViewBag.DataMap = await _datamapService.GetFormDataMap(Convert.ToInt32(fdtype.DataType.FormDataTypeID));
            return PartialView("CreateDataMapItem", dmi);
        }
        [HttpGet]
        public ActionResult NewForm()
        {
            var model = new FullFormModel();

            model.DataType = new DataTypeModel();
            model.DataType.DataType = DataFormat.JSON;
            model.FormModel = new FormModel();
            model.FormModel.FileType = FileType.PDF;
            model.DataMap = new List<DataMapItemModel>();

            return View(model);
        }
        [HttpPost]
        public ActionResult NewForm(FullFormModel nform)
        {
            if (ModelState.IsValid)
            {
                nform.FormModel.Active = true;
                _formService.FormAdd(nform);
                return RedirectToAction("List");
            }
            else
            {
                return View(nform);
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddMapItem(string ditem)
        {
            if (ModelState.IsValid)
            {
                //int fid = Convert.ToInt32(Request.RequestContext.RouteData.Values["id"].ToString());
               

                Dictionary<string, object> dmap = JsonSerializer.Deserialize<Dictionary<string, object>>(ditem);

                DataMapItemModel newitem = new DataMapItemModel();

                newitem.FormDataTypeID = Convert.ToInt32(dmap["FormDataTypeID"].ToString());
                newitem.DataObject = dmap["DataObject"].ToString();

                if (dmap["FormObject"].ToString() == "Dynamic")
                {
                    newitem.FormObject = dmap["DataObject"].ToString();
                }
                else
                {
                    newitem.FormObject = dmap["FormObject"].ToString();
                }
                newitem.Repeatable = Convert.ToBoolean(dmap["Repeatable"].ToString());
                newitem.Calculated = Convert.ToBoolean(dmap["Calculated"].ToString());
                newitem.Expression = dmap["Expression"].ToString();

                if (newitem.Repeatable == true)
                {
                    newitem.ChildFormObjects = true;
                }
                else
                {
                    newitem.ChildFormObjects = false;
                }
                newitem.ItemCount = Convert.ToInt32(dmap["ItemCount"].ToString());
                newitem.CheckValue = dmap["CheckValue"].ToString();

                await _datamapService.AddMapItem(newitem);

                int fid = await _formService.GetFormIDbyDataType(Convert.ToInt32(newitem.FormDataTypeID));

                Dictionary<string, string> returndata = new Dictionary<string, string>();

                returndata.Add("data", fid.ToString());

                return (Json(returndata));
            }
            else
            {
                return View(ditem);
            }
        }

        public async Task<IActionResult> DeleteMapItem(int MapID)
        {
            DataMapItemModel ditem = await _datamapService.GetMapItem(MapID);

            int fid = await _datamapService.GetFormIDFromMapItem(MapID);

            _datamapService.DeleteMapItem(MapID);

            Dictionary<string, string> returndata = new Dictionary<string, string>();

            returndata.Add("data", fid.ToString());

            return (Json(returndata));
        }
        public async Task<IActionResult> AutoMapItems(int did)
        {
            int fid = await _formService.GetFormIDbyDataType(did);

            _datamapService.AutoMapItems(did, fid);

            Dictionary<string, string> returndata = new Dictionary<string, string>();

            returndata.Add("data", fid.ToString());

            return (Json(returndata));
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, FullFormModel nform)
        {
            var model = await _formService.FullFormInfo(id);

            ViewBag.DataID = model.FormModel.fid;
            ViewBag.DataMap = model.DataMap;

            if (Request.Form.Files.Count != 0)
            {
                model.FormModel.TempFile = nform.FormModel.TempFile;
                await _formService.FormUpdate(model.FormModel);
            }

            return View("Edit", model);
        }

        [HttpGet]
        public async Task<IActionResult> EditChildren(int id)
        {
            ViewBag.ParentMap = await _datamapService.GetChildObjectsByParent(id);
            DataMapItemModel parent = await _datamapService.GetMapItem(id);

            ViewBag.Parent = parent.DataObject;

            var model = new ChildMapItemModel();

            model.ParentObject = parent.DataMapID;

            return View("EditChildren", model);


        }
        [HttpPost]
        public async Task<IActionResult> EditChildren(int id, ChildMapItemModel citem)
        {
            if (ModelState.IsValid)
            {

                citem.ParentObject = id;

                citem.Calculated = false;

                await _datamapService.AddChildObject(citem);
                ViewBag.ParentMap = _datamapService.GetChildObjectsByParent(Convert.ToInt32(citem.ParentObject));
                ViewBag.Parent = Convert.ToInt32(citem.ParentObject);



                var model = new ChildMapItemModel();

                model.ParentObject = Convert.ToInt32(citem.ParentObject);

                return View("EditChildren", model);

            }
            else
            {
                return View(citem);
            }
        }
    }
}