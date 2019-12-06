using PlatformaDeStiri.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlatformaDeStiri.Controllers
{

    public class CategoryController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Categorie
        public ActionResult Index()
        {
            var categories = db.Categories.Include("News");

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.categories = categories;
            return View();
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public ActionResult New()
        {
            Category category = new Category();

            return View(category);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult New(Category category)
        {
            try
            {
                db.Categories.Add(category);
                db.SaveChanges();
                TempData["message"] = "Categoria '" + category.Name +
                    "' a fost creata.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return View("Error");
            }

        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int id)
        {
            Category category = db.Categories.Find(id);
            return View(category);
            
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut]
        public ActionResult Edit(int id, Category requestCategory)
        {
            try
            {
            Category category = db.Categories.Find(id);

                   
            if (TryUpdateModel(category))
            {
                category.Name = requestCategory.Name;
                db.SaveChanges();
            }
            TempData["message"] = "Categoria cu numele " + category.Name + " a fost modificata! :)";
            return RedirectToAction("Index");
                  
               
            }
            catch (Exception e)
            {
                return View("Error");
            }
        }


        [Authorize(Roles = "Administrator")]
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Category category = db.Categories.Find(id);

            TempData["message"] = "Categoria " + category.Name + " a fost stearsa!";
            db.Categories.Remove(category);

            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}