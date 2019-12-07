using PlatformaDeStiri.Models;
using System;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlatformaDeStiri.Controllers
{
    public class SuggestionController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Suggestion

        public ActionResult Index()
        {

            if(User.IsInRole("User"))
            {
                var sugg = db.Suggestions.Include("User").Include("User").ToList();
                //System.Diagnostics.Debug.WriteLine(sugg.Count);
                List<Suggestion> sentSugg = new List<Suggestion>();
                foreach (Suggestion s in sugg)
                {
                    if (s.UserID == User.Identity.GetUserId())
                    {
                        sentSugg.Add(s);
                    }
                }
                ViewBag.sentSugg = sentSugg;
                //ViewBag.sentSugg = sugg;
                return View();
            }

            else
            {
                if (User.IsInRole("Administrator") || User.IsInRole("Editor"))
                {
                    var sugg = db.Suggestions.Include("User").Include("Editor");
                    List<Suggestion> sentSugg = new List<Suggestion>();
                    List<Suggestion> receivedSugg = new List<Suggestion>();
                    foreach (Suggestion s in sugg)
                    {
                        if (s.UserID == User.Identity.GetUserId())
                        {
                            sentSugg.Add(s);
                        }
                        else
                        {
                            if (s.EditorID == User.Identity.GetUserId())
                            {
                                receivedSugg.Add(s);
                            }
                        }
                    }
                    ViewBag.sentSugg = sentSugg;
                    ViewBag.receivedSugg = receivedSugg;
                    return View();
                }
            }

            return View();
        }


        public ActionResult New (int newsId)
        {
            Suggestion sugg = new Suggestion();
            sugg.UserID = User.Identity.GetUserId();
            sugg.User = db.Users.Find(sugg.UserID);
            sugg.EditorID = db.News.Find(newsId).UserID;
            sugg.Editor = db.Users.Find(sugg.EditorID);
            sugg.suggDate = DateTime.Now;
            sugg.suggState = 0;
            return View(sugg);

        }


        [HttpPost]
        public ActionResult New(Suggestion sugg)
        {
            db.Suggestions.Add(sugg);

           //System.Diagnostics.Debug.WriteLine(sugg.suggTitle);
            db.SaveChanges();
            TempData["message"] = "Sugestia a fost adaugata! ";
            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult Show(int id)
        {
            try
            {
                Suggestion sugg = db.Suggestions.Find(id);
                ViewBag.currentUser = User.Identity.GetUserId();
                if (sugg == null)
                    throw (new Exception());
                return View(sugg);
            }
            catch (Exception)
            {
                ViewBag.error = "Element with ID=" + id +
                    " could not be found.";
                return View("Error");
            }

        }


        
        public ActionResult Accept (int id)
        {

            return RedirectToAction("NewFromSuggestion","News", new { id = id});
           
           }


        [HttpGet]
        public ActionResult Reject (int id)
        {
            var suggestion = db.Suggestions.Find(id);
            suggestion.suggState = 2; //respinsa 

            db.SaveChanges();
            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult Edit(int id)
        {
            try
            {
                Suggestion sugg = db.Suggestions.Find(id);
                if (sugg == null)
                    throw (new Exception());
                return View(sugg);
            }
            catch (Exception)
            {
                ViewBag.error = "Could not show element with ID=" +
                    id + ". ";
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult Edit(Suggestion sugg)
        {
            try
            {
                Suggestion db_sugg = db.Suggestions.Find(sugg.suggID);

                db_sugg.suggTitle = sugg.suggTitle;
                db_sugg.UserID = User.Identity.GetUserId();
                db_sugg.User = db.Users.Find(db_sugg.UserID);
                db_sugg.EditorID = sugg.EditorID;
                db_sugg.Editor = db.Users.Find(db_sugg.EditorID);
                db_sugg.suggState = sugg.suggState;
                db_sugg.suggContent = sugg.suggContent;
                db_sugg.suggDate = DateTime.Now;
                db.SaveChanges();

                // System.Diagnostics.Debug.WriteLine(comm.commNewsID);
                TempData["message"] = "Sugestia a fost actualizata.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.error = "Could not edit elementi with ID=" +
                    sugg.suggID + ". (Post)";
                return View("Error");
            }
        }


        [HttpDelete]
        public ActionResult Delete(int id)
        {

            Suggestion sugg = db.Suggestions.Find(id);
            db.Suggestions.Remove(sugg);
            db.SaveChanges();
            TempData["message"] = "Sugestia a fost stearsa";
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