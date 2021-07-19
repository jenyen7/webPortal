using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EnterprisePortal.Models;

namespace EnterprisePortal.Controllers
{
    public class MessageListsController : Controller
    {
        private readonly PortalModel db = new PortalModel();

        // GET: MessageLists
        public ActionResult Index()
        {
            var messageLists = db.MessageLists.Include(m => m.QuestionerUser).Include(m => m.Activity).Include(m => m.Voting);
            return View(messageLists.Where(w => w.IsVisible == true).ToList());
        }

        // GET: MessageLists/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MessageList messageList = db.MessageLists.Find(id);
            if (messageList == null)
            {
                return HttpNotFound();
            }
            return View(messageList);
        }

        // GET: MessageLists/Create
        public ActionResult Create()
        {
            ViewBag.ActivityId = new SelectList(db.Activities, "ActivityId", "Title");
            ViewBag.QuestionerId = new SelectList(db.UserAccounts, "UserId", "Account");
            ViewBag.VotingId = new SelectList(db.Votings, "VotingId", "Title");
            return View();
        }

        // POST: MessageLists/Create
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MessageListId,ActivityId,VotingId,QuestionerId")] MessageList messageList)
        {
            if (ModelState.IsValid)
            {
                db.MessageLists.Add(messageList);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ActivityId = new SelectList(db.Activities, "ActivityId", "Title", messageList.ActivityId);
            ViewBag.QuestionerId = new SelectList(db.UserAccounts, "UserId", "Account", messageList.QuestionerId);
            ViewBag.VotingId = new SelectList(db.Votings, "VotingId", "Title", messageList.VotingId);
            return View(messageList);
        }

        // GET: MessageLists/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MessageList messageList = db.MessageLists.Find(id);
            if (messageList == null)
            {
                return HttpNotFound();
            }
            ViewBag.ActivityId = new SelectList(db.Activities, "ActivityId", "Title", messageList.ActivityId);
            ViewBag.QuestionerId = new SelectList(db.UserAccounts, "UserId", "Account", messageList.QuestionerId);
            ViewBag.VotingId = new SelectList(db.Votings, "VotingId", "Title", messageList.VotingId);
            return View(messageList);
        }

        // POST: MessageLists/Edit/5
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MessageListId,ActivityId,VotingId,QuestionerId")] MessageList messageList)
        {
            if (ModelState.IsValid)
            {
                db.Entry(messageList).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ActivityId = new SelectList(db.Activities, "ActivityId", "Title", messageList.ActivityId);
            ViewBag.QuestionerId = new SelectList(db.UserAccounts, "UserId", "Account", messageList.QuestionerId);
            ViewBag.VotingId = new SelectList(db.Votings, "VotingId", "Title", messageList.VotingId);
            return View(messageList);
        }

        // GET: MessageLists/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MessageList messageList = db.MessageLists.Find(id);
            if (messageList == null)
            {
                return HttpNotFound();
            }
            return View(messageList);
        }

        // POST: MessageLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MessageList messageList = db.MessageLists.Find(id);
            messageList.IsVisible = false;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}