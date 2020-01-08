using PlatformaDeStiri.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace PlatformaDeStiri.Controllers
{
    public class DbImageController : Controller
    {
        // GET: Image
        public ActionResult Index()
        {

            return View();
        }

        [HttpGet]
        public ActionResult Add()
        {

            return View();

        }

        [HttpPost]
        public ActionResult Add(DbImage imageModel)
        {
            string fileName = Path.GetFileNameWithoutExtension(imageModel.ImageFile.FileName);
            string extension = Path.GetExtension(imageModel.ImageFile.FileName);
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
            imageModel.ImagePath = "~/Image/" + fileName;

            fileName = Path.Combine(Server.MapPath("~/Image/"), fileName);
            imageModel.ImageFile.SaveAs(fileName);

            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                db.DbImages.Add(imageModel);
                db.SaveChanges();
            }
            ModelState.Clear();

                return View();
        }


        [HttpGet]
        public ActionResult View(int id)
        {
            DbImage imageModel = new DbImage();

            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                imageModel = db.DbImages.Where(x => x.ImageID == id).FirstOrDefault();
            }

            return View(imageModel);
        }
    }
}