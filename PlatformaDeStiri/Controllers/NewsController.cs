using Microsoft.AspNet.Identity;
using PlatformaDeStiri.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
            var news = db.News.Include("Category").Include("User").Include("Comments").Include("Suggestions").Include("Image")
                    .AsEnumerable().OrderBy(n => n.Date).Reverse().ToList();
            

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

        public ActionResult MyNews()
        {
            var news = db.News.Include("Category").Include("User").Include("Comments").Include("Suggestions");
            List<News> myNews = new List<News>();

            foreach (News n in news)
            {
                if (n.User.Id == User.Identity.GetUserId())
                {
                    myNews.Add(n);
                }
            }

            ViewBag.myNews = myNews;
            return View("MyNews");
        }

        [Authorize(Roles = "Editor, Administrator")]
        [HttpGet]
        public ActionResult New()
        {
            News news = new News();

            news.UserID = User.Identity.GetUserId();
            news.Categories = GetAllCategories();
            news.Date = DateTime.Now;

            List<Category> categs = db.Categories.ToList();
            ViewBag.categories = categs;

            return View(news);
        }

        
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

            return RedirectToAction("Show","News", new { id = newsId});
        }

        [Authorize(Roles = "Editor, Administrator")]
        [HttpPost]
        public ActionResult New(News news, string cumstomCategory, HttpPostedFileBase image)
        {
            news.Categories = GetAllCategories();
            System.Diagnostics.Debug.WriteLine(" mesaj din new cu post" + image.ToString());

            try
            {


                DbImage img = new DbImage();

                img.ImageFile = image;
                img.Title = news.Title;
               
                
                string fileName = Path.GetFileNameWithoutExtension(image.FileName);
                string extension = Path.GetExtension(image.FileName);
                fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                img.ImagePath = "~/Image/" + fileName;

                fileName = Path.Combine(Server.MapPath("~/Image/"), fileName);
                img.ImageFile.SaveAs(fileName);


                
                db.DbImages.Add(img);
                //db.SaveChanges();
                System.Diagnostics.Debug.WriteLine("Id imagine: " +img.ImageID);


                System.Diagnostics.Debug.WriteLine("Titlu stire: " + news.Title);
                if (news.CategoryID == -999)
                {
                    Category category = new Category();
                    category.Name = cumstomCategory;
                    db.Categories.Add(category);
                    news.CategoryID = category.ID;
                    
                }
                news.ImageID = img.ImageID;

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

        public ActionResult NewFromSuggestion(int id)
        {
            var suggestion = db.Suggestions.Find(id);
            suggestion.suggState = 1; // acceptata

            db.SaveChanges();
            System.Diagnostics.Debug.WriteLine("[Suggestion Title: ]" + suggestion.suggTitle);
            News newss = new News();

            newss.Title = suggestion.suggTitle;
            newss.Content = suggestion.suggContent;
            newss.Date = DateTime.Now;
            newss.UserID = suggestion.EditorID;
            newss.suggestedUser = suggestion.UserID;
            newss.Categories = GetAllCategories();
            List<Category> categs = db.Categories.ToList();
            ViewBag.categories = categs;

            return View("New", newss);
        }


        [Authorize(Roles = "Editor, Administrator")]
        [HttpPost]
        public ActionResult NewFromSuggestion(News news)
        {
            news.Categories = GetAllCategories();

            try
            {
                System.Diagnostics.Debug.WriteLine("ID stire: " + news.ID);
                db.News.Add(news);
                db.SaveChanges();
                System.Diagnostics.Debug.WriteLine("ID stire after: " + news.ID);
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

                List<Category> categs = db.Categories.ToList();
                ViewBag.categories = categs;
               // System.Diagnostics.Debug.WriteLine("categorie selectata IN EDIT GET: " + news.CategoryID);

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
        [HttpPut]
        public ActionResult Edit(int id, News news,  string cumstomCategory, HttpPostedFileBase ImageFile)
        {
            news.Categories = GetAllCategories();
            try
            {
               // if (ModelState.IsValid)
               // {
                    News db_news = db.News.Find(id);
                    if (db_news.UserID == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                    {
                    System.Diagnostics.Debug.WriteLine(" first if");

                    if (TryUpdateModel(db_news))
                       {
                       

                        db_news.Title = news.Title;
                        
                        db_news.Content = news.Content;
                        db_news.Date = DateTime.Now;
                        db_news.UserID = User.Identity.GetUserId();

                        System.Diagnostics.Debug.WriteLine(" db_news.Title " + db_news.Title);

                        System.Diagnostics.Debug.WriteLine(" db_news.Content " + db_news.Content);
                        System.Diagnostics.Debug.WriteLine(" db_news.Date " + db_news.Date);

                        System.Diagnostics.Debug.WriteLine(" db_news.UserID " + db_news.UserID);
                        if (news.CategoryID == -999)
                            {
                                Category category = new Category();
                                category.Name = cumstomCategory;
                                db.Categories.Add(category);
                                news.CategoryID = category.ID;

                            }

                    
                            db_news.CategoryID = news.CategoryID;
                        System.Diagnostics.Debug.WriteLine(" db_news.CategoryID " + db_news.CategoryID);

                        if (ImageFile != null && ImageFile.ContentLength > 0)
                            {
                                DbImage img = new DbImage();


                                img.Title = news.Title;
                                img.ImageFile = ImageFile;

                                string fileName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                                string extension = Path.GetExtension(ImageFile.FileName);
                                fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                                img.ImagePath = "~/Image/" + fileName;

                                fileName = Path.Combine(Server.MapPath("~/Image/"), fileName);
                                img.ImageFile.SaveAs(fileName);


                                db.DbImages.Add(img);
                                db_news.ImageID = img.ImageID;
                            }


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
                //}
              /*  else
                {
                    System.Diagnostics.Debug.WriteLine(" Invalid Model State");
                    return RedirectToAction("Index");
                }*/
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

        [HttpGet]
        public ActionResult SearchNews (string keywords)
        {
            ViewBag.keywords = keywords;
            var searchResults = db.News.SqlQuery("" +
                "SELECT * FROM News " +
                "WHERE lower(Title) LIKE '%" + keywords.ToLower() + "%'").ToList();
            ViewBag.searchResults = searchResults;
            return View();
        }


    }

}