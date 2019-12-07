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
        static int NEWS_PER_PAGE = 3;
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Stire
        
        public ActionResult Index()
        {
            // Query the database for all the news
            var news = db.News.Include("Category").Include("User").Include("Comments").Include("Suggestions");

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            // Complete the ViewBag.maxPgNr and ViewBag.pageNr
            ViewBag.maxPgNr = news.Count() / NEWS_PER_PAGE + 1;
            ViewBag.pageNr = TempData.ContainsKey("pageNr") ? TempData["pageNr"] : 1;
            
            // Select from the list
            List<News> newstoShow = new List<News>();
            int firstIndex = NEWS_PER_PAGE * (ViewBag.pageNr - 1);
            int lastIndex = min(firstIndex + NEWS_PER_PAGE, news.Count());
            int ind = 0;

            foreach(News dbNews in news)
            {
                if(ind >= firstIndex && ind < lastIndex)
                {
                    newstoShow.Add(dbNews);
                }

                if (ind >= lastIndex)
                    break;

                ind++;
            }

            // Load trimmed list into viewbag
            ViewBag.news = newstoShow;
            return View();
        }

        [HttpGet]
        public ActionResult PageSwitch (int currPage)
        {
            TempData["pageNr"] = currPage;
            return RedirectToAction("Index");
        }

        [NonAction]
        private int min(int v1, int v2)
        {
            return v1 < v2 ? v1 : v2;
        }

        [Authorize(Roles = "Editor, Administrator")]
        [HttpGet]
        public ActionResult New()
        {
            News news = new News();

            news.UserID = User.Identity.GetUserId();
            news.Categories = GetAllCategories();
            news.Date = DateTime.Now;

            return View(news);
        }

        [Authorize(Roles = "Editor, Administrator")]
        [HttpPost]
        public ActionResult AddComment (string newsId, string commStr)
        {
            int id = System.Int32.Parse(newsId);
            Comment newComment = new Comment();
            newComment.commUserID = User.Identity.GetUserId();
            newComment.commNewsID = id;
            newComment.commContent = commStr;
            newComment.commDate = DateTime.Now;
            newComment.news = db.News.Find(id);
            newComment.user = db.Users.Find(newComment.commUserID);

            db.Comments.Add(newComment);
            
            db.SaveChanges();
            TempData["message"] = "Comentariul a fost adaugat.";

            return RedirectToAction("Index");
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


        [Authorize(Roles = "User, Editor, Administrator")]
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

        [Authorize(Roles = "Editor, Administrator")]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            try
            {
                News news = db.News.Find(id);
                if (news == null)
                    throw (new Exception());
                news.Categories = GetAllCategories();

                if (news.UserID == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                {
                    return View(news);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari!";
                    return RedirectToAction("Index");
                }

            }
            catch (Exception)
            {
                ViewBag.error = "Could not edit element with ID=" +
                    id + ". ";
                return View("Error");
            }
        }

        [Authorize(Roles = "Editor, Administrator")]
        [HttpPost]
        public ActionResult Edit(int id, News news)
        {
            news.Categories = GetAllCategories();
            try
            {
                if (ModelState.IsValid)
                {
                    News db_news = db.News.Find(id);
                    if (db_news.UserID == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                    {
                        if (TryUpdateModel(news))
                        {
                            db_news.Title = news.Title;
                            db_news.Content = news.Content;
                            db_news.Date = DateTime.Now;
                            db_news.UserID = User.Identity.GetUserId();
                            db_news.CategoryID = news.CategoryID;
                            db.SaveChanges();

                            TempData["message"] = "Stirea cu titlul '" +
                                news.Title + "' a fost actualizata.";
                        }
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari!";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return View(news);
                }
            }
            catch (Exception)
            {
                ViewBag.error = "Could not edit elementi with ID=" +
                    news.ID + ". (Post)";
                return View("Error");
            }
        }

        [Authorize(Roles = "Editor, Administrator")]
        [HttpDelete]
        public ActionResult Delete(int id)
        {
           
            News news = db.News.Find(id);
            if (news.UserID == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                db.News.Remove(news);
                db.SaveChanges();
                TempData["message"] = "Articolul cu numele " +
                    news.Title + " a fost sters din baza de date";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul de a sterge aceasta stire";
                return RedirectToAction("Index");
            }
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