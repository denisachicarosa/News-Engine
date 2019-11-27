using Microsoft.AspNet.Identity;
using PlatformaDeStiri.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlatformaDeStiri.Controllers
{
    public class CommentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Comment
        public ActionResult Index()
        {
            var comm = db.Comments;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.comm = comm;
            return View();

        }

        [HttpGet]
        public ActionResult New(int newsID)
        {
            Comment comm = new Comment();

            comm.commUserID = User.Identity.GetUserId();
            comm.commDate = DateTime.Now;
            comm.commNewsID = newsID;
            return View(comm);
        }

        [HttpPost]
        public ActionResult New(Comment comm)
        {

            try
            {
                db.Comments.Add(comm);
                db.SaveChanges();
                TempData["message"] = "Comentariul a fost adaugat! ";
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
                Comment comm = db.Comments.Find(id);
                if (comm == null)
                    throw (new Exception());
                return View(comm);
            }
            catch (Exception)
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
                Comment comm = db.Comments.Find(id);
                if (comm == null)
                    throw (new Exception());
                return View(comm);
            }
            catch (Exception)
            {
                ViewBag.error = "Could not show element with ID=" +
                    id + ". ";
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult Edit(Comment comm)
        {
            try
            {
                Comment db_comm = db.Comments.Find(comm.commID);
                
                db_comm.commContent = comm.commContent;
                db_comm.commDate = DateTime.Now;
                db_comm.commUserID = User.Identity.GetUserId(); 
                db.SaveChanges();

                TempData["message"] = "Comentariul a fost actualizat.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.error = "Could not edit elementi with ID=" +
                    comm.commID + ". (Post)";
                return View("Error");
            }
        }
        [HttpDelete]
        public ActionResult Delete(int id)
        {

            Comment comm = db.Comments.Find(id);
            db.Comments.Remove(comm);
            db.SaveChanges();
            TempData["message"] = "Comentariul a fost sters";
            return RedirectToAction("Index");
        }
    }
}