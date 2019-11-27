using Microsoft.AspNet.Identity;
using PlatformaDeStiri.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlatformaDeStiri.Controllers
{
    public class NewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Stire
        public ActionResult Index()
        {
            var news = db.News.Include("Category").Include("Comment").Include("Suggestion");

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.news = news;
            return View();
        }

        [HttpGet]
        public ActionResult New()
        {
            News news = new News();

            news.UserID = User.Identity.GetUserId();
            news.Categories = GetAllCategories();
            news.Date = DateTime.Now;

            return View(news);
        }

        [HttpPost]
        public ActionResult New(News news)
        {
            news.Categories = GetAllCategories();

            try
            {
                db.News.Add(news);
                db.SaveChanges();
                TempData["message"] = "Stirea cu titlul '" +
                    news.Title + "' a fost creata.";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                ViewBag.error = e.Message.ToString();
                return View("Error");
            }

        }

        [HttpGet]
        public ActionResult Show(int id)
        {
            
            try
            {
                News news = db.News.Find(id);
                if (news == null)
                    throw (new Exception());
                return View(news);
            }
            catch(Exception)
            {
                ViewBag.error = "Element with ID=" + id +
                    " could not be found.";
                return View("Error");
            }

        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            try
            {
                News news = db.News.Find(id);
                if (news == null)
                    throw (new Exception());
                news.Categories = GetAllCategories();

                return View(news);
            }
            catch (Exception)
            {
                ViewBag.error = "Could not show element with ID=" +
                    id + ". ";
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult Edit(News news)
        {
            try
            {
                News db_news = db.News.Find(news.ID);
                db_news.Title = news.Title;
                db_news.Content = news.Content;
                db_news.Date = DateTime.Now;
                db_news.UserID = User.Identity.GetUserId();
                db_news.CategoryID = news.CategoryID;
                db.SaveChanges();

                TempData["message"] = "Stirea cu titlul '" +
                    news.Title + "' a fost actualizata.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.error = "Could not edit elementi with ID=" +
                    news.ID + ". (Post)";
                return View("Error");
            }
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {

            News news = db.News.Find(id);
            db.News.Remove(news);
            db.SaveChanges();
            TempData["message"] = "Articolul cu numele " +
                news.Title + " a fost sters din baza de date";
            return RedirectToAction("Index");
        }

        [NonAction]
        private IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();

            var categories = from cat in db.Categories
                             select cat;

            foreach (var category in categories)
            {
                selectList.Add(
                        new SelectListItem
                        {
                            Value = category.ID.ToString(),
                            Text = category.Name.ToString()
                        }
                    );
            }

            return selectList;
        }
    }
}