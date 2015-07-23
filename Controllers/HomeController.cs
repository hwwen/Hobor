using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Hobor.Controllers
{
    public class HomeController : Hobor.Base.BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public JsonResult GetHobor(double? XAxis, double? YAxis)
        {
            Hobor.Models.HoborModel Hobor = new Models.HoborModel();
            if (XAxis.HasValue && YAxis.HasValue)
            {
                Hobor = Models.HoborModel.HoborAsker(MyHelper, XAxis.Value, YAxis.Value);
            }
            return Json(Hobor, JsonRequestBehavior.AllowGet);
        }
    }
}
