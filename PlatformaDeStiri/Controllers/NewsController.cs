using Antlr.Runtime.Misc;
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
        protected ApplicationDbContext db = new ApplicationDbContext();

        // Auxiliar class for the index
        public class IndexIdentity
        {
            public static int NO_CATEG_FILTER = -1;
            public static string NO_SORTING = "noo";
            public SelectListItem chosen_categ { set; get; }
            public SelectListItem chosen_sort { set; get; }

            public IndexIdentity()
            {
                chosen_categ = chosen_sort = null;
            }

            public void ExportToViewBag(dynamic viewBag)
            {
                viewBag.chosen_categ = chosen_categ;
                viewBag.chosen_sort = chosen_sort;
            }
            public void ExportToTempData(dynamic tempData)
            {
                tempData["chosen_categ"] = chosen_categ;
                tempData["chosen_sort"] = chosen_sort;
            }
            public int ImportFromTempData(dynamic tempData)
            {
                int succesful = 0;

                if (tempData.ContainsKey("chosen_categ"))
                {
                    this.chosen_categ = (SelectListItem)tempData["chosen_categ"];
                    succesful++;
                }
                else
                {
                    this.chosen_categ = new SelectListItem()
                        { Text = "All categories", Value = NO_CATEG_FILTER.ToString() };
                }

                if (tempData.ContainsKey("chosen_sort"))
                {
                    this.chosen_sort = (SelectListItem)tempData["chosen_sort"];
                    succesful++;
                }
                else
                {
                    this.chosen_sort = new SelectListItem()
                    {
                        Text = "No sorting",
                        Value = NO_SORTING
                    };
                }
                
                return succesful;
            }
            private List<SelectListItem> GetSortingMethods()
            {
                List<SelectListItem> selectLists = new List<SelectListItem>();


                selectLists.Add(new SelectListItem
                {
                    Text = "No sorting",
                    Value = "noo"
                });

                selectLists.Add(new SelectListItem
                {
                    Text = "Ordine alfabetica",
                    Value = "alf"
                });

                selectLists.Add(new SelectListItem
                {
                    Text = "Data aparitiei",
                    Value = "apr"
                });

                selectLists.Add(new SelectListItem
                {
                    Text = "Lungimea descrierii (c)",
                    Value = "ldc"
                });


                selectLists.Add(new SelectListItem
                {
                    Text = "Lungimea descrierii (d)",
                    Value = "ldd"
                });

                return selectLists;
            }
            private Comparison<News> SortCond(string type)
            {
                Comparison<News> lambd = (n1, n2) => 0;
                switch (type)
                {
                    case "noo":
                        lambd = (n1, n2) => 0;
                        break;

                    case "alf":
                        lambd = (n1, n2) => n1.Title.CompareTo(n2.Title);
                        break;

                    case "apr":
                        lambd = (n1, n2) => n1.Date.CompareTo(n2.Date);
                        break;

                    case "ldc":
                        lambd = (n1, n2) => n1.Content.Count().CompareTo(n2.Content.Count());
                        break;

                    case "ldd":
                        lambd = (n1, n2) => n2.Content.Count().CompareTo(n1.Content.Count());
                        break;

                    default:
                        break;
                }

                return lambd;
            }
            public void SortAndFilter(List<News> news)
            {
                int categ_index = chosen_categ.Value != null 
                    ? Int32.Parse(chosen_categ.Value)
                    : NO_CATEG_FILTER;

                if (chosen_sort.Value != NO_SORTING)
                    news.Sort(SortCond(chosen_sort.Value));

                //if (categ_index != NO_CATEG_FILTER)
                //{
                //    //news = news.FindAll(n => n.CategoryID == categ_index).ToList();
                //    List<News> filteredList = new List<News>();
                //    foreach (News currNews in news)
                //    {
                //        if (currNews.CategoryID == categ_index)
                //            filteredList.Add(currNews);
                //    }
                //    news = filteredList;
                //}

                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[Filtering] categ-index = " + categ_index.ToString());
                System.Diagnostics.Debug.WriteLine("[Sorting] sort-mode = " + chosen_sort.Value.ToString());
            }
        }

        // GET: Stire
        public ActionResult Index()
        {
            // Trying weird stulff
            IndexIdentity identity = new IndexIdentity();
            identity.ImportFromTempData(TempData);

            // Query the database for all the news
            List<News> news = db.News.Include("Category").Include("User").Include("Comments").Include("Suggestions")
                    .AsEnumerable().ToList();
            if (identity.chosen_categ.Value != IndexIdentity.NO_CATEG_FILTER.ToString())
            {
                int theIndex = Int32.Parse(identity.chosen_categ.Value);
                Category cat = db.Categories.Include("News").SingleOrDefault(c => c.ID == theIndex);
                news = cat.News.ToList();
            }
            
            identity.SortAndFilter(news);
            identity.ExportToViewBag(ViewBag);


            // Motificatiton handling
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            /**
            // Filtering by categoies
            System.Func<News, bool> categ_condition = n => true;
            if (TempData.ContainsKey("chosen_categ"))
            {
                SelectListItem chosen_categ = (SelectListItem)TempData["chosen_categ"];
                if (chosen_categ.Value != null)
                {
                    System.Diagnostics.Debug.Write("[Categ] chosen_categ.Value = " + chosen_categ.Value);
                    ViewBag.chosen_categ = chosen_categ;
                    categ_condition = n => Int32.Parse(chosen_categ.Value).Equals(n.CategoryID);
                }
            }

            // Sorting 
            if (TempData.ContainsKey("chosen_sort"))
            {
                SelectListItem chosen_sort = (SelectListItem)TempData["chosen_sort"];
                SelectListItem chosen_sortttt = new SelectListItem { Text = "some", Value = "Oher" };
                ViewBag.chosen_categ = chosen_sort;
                news.Sort(SortCond(chosen_sort.Value));
                
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
                if(ind >= firstIndex && ind < lastIndex 
                    && categ_condition(dbNews))
                {
                    newstoShow.Add(dbNews);
                }

                if (ind >= lastIndex)
                    break;

                ind++;
            }
            */

            List<SelectListItem> categories = new List<SelectListItem>();
            categories.Add(new SelectListItem()
            { Text = "All Categories", Value = IndexIdentity.NO_CATEG_FILTER.ToString() });
            categories.AddRange(GetAllCategories());

            // Load trimmed list into viewbag
            ViewBag.news = news;
            ViewBag.categs = categories;
            ViewBag.sorts = GetSortingMethods();
            return View();
        }

        [HttpGet]
        public ActionResult PageSwitch (int currPage)
        {
            TempData["pageNr"] = currPage;
            return RedirectToAction("Index");
        }
        

        [HttpGet]
        public ActionResult SwitchSortOrFilter (string sortTxt, string sortVal, 
            string categTxt, string categVal)
        {
            SelectListItem chosen_sort = new SelectListItem { Text = sortTxt, Value = sortVal };
            SelectListItem chosen_categ = new SelectListItem { Text = categTxt, Value = categVal };

            IndexIdentity identity = new IndexIdentity();
            identity.chosen_sort = chosen_sort;
            identity.chosen_categ = chosen_categ;
            identity.ExportToTempData(TempData);

            return RedirectToAction("Index");
        }

        [NonAction]
        private int min(int v1, int v2)
        {
            return v1 < v2 ? v1 : v2;
        }

        [NonAction]
        private List<SelectListItem> GetSortingMethods ()
        {
            List<SelectListItem> selectLists = new List<SelectListItem>();


            selectLists.Add(new SelectListItem
            {
                Text = "No sorting",
                Value = "noo"
            });

            selectLists.Add(new SelectListItem
            {
                Text = "Ordine alfabetica",
                Value = "alf"
            });

            selectLists.Add(new SelectListItem
            {
                Text = "Data aparitiei",
                Value = "apr"
            });

            selectLists.Add(new SelectListItem
            {
                Text = "Lungimea descrierii (c)",
                Value = "ldc"
            });


            selectLists.Add(new SelectListItem
            {
                Text = "Lungimea descrierii (d)",
                Value = "ldd"
            });

            return selectLists;
        }

        [NonAction]
        private Comparison<News> SortCond (string type)
        {
            Comparison<News> lambd = (n1, n2) => 0;
            switch (type)
            {
                case "noo":
                    lambd = (n1, n2) => 0;
                    break;

                case "alf":
                    lambd = (n1, n2) => n1.Title.CompareTo(n2.Title);
                    break;

                case "apr":
                    lambd = (n1, n2) => n1.Date.CompareTo(n2.Date);
                    break;

                case "ldc":
                    lambd = (n1, n2) => n1.Content.Count().CompareTo(n2.Content.Count());
                    break;

                case "ldd":
                    lambd = (n1, n2) => n2.Content.Count().CompareTo(n1.Content.Count());
                    break;

                default:
                    break;
            }

            return lambd;
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
        public ActionResult New(News news, string cumstomCategory)
        {
            news.Categories = GetAllCategories();

            try
            {
                System.Diagnostics.Debug.WriteLine("Titlu stire: " + news.Title);
                if (news.CategoryID == -999)
                {
                    Category category = new Category();
                    category.Name = cumstomCategory;
                    db.Categories.Add(category);
                    news.CategoryID = category.ID;
                }
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