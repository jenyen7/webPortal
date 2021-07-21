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
            var messageLists = db.MessageLists.Include(m => m.Activity).Include(m => m.Voting);
            int currentUser = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
            ViewBag.CurrentUser = currentUser;
            return View(messageLists.Where(w => (w.QuestionerId == currentUser || w.AnswererId == currentUser) && w.IsVisible == true).ToList());
        }

        public ActionResult Messages(int id)
        {
            int currentUser = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
            string title, name, avatar;
            int titleId, section;
            var msgList = db.MessageLists.FirstOrDefault(f => f.MessageListId == id);
            if (msgList.AnswererId != currentUser && msgList.QuestionerId != currentUser)
            {
                TempData["modalTitle"] = "你被發現惹。";
                TempData["modalBody"] = "想偷看別人的對話!";
                return RedirectToAction("Index");
            }

            if (msgList.QuestionerId == currentUser)
            {
                var userAnswerer = db.UserAccounts.FirstOrDefault(f => f.UserId == msgList.AnswererId);
                if (userAnswerer != null)
                {
                    avatar = userAnswerer.Avatar.Substring(1);
                    name = userAnswerer.Account;
                }
                else
                {
                    TempData["modalTitle"] = "對方帳號已遭移除。";
                    TempData["modalBody"] = "建議刪除此對話紀錄，謝謝您。";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                var userQuestionner = db.UserAccounts.FirstOrDefault(f => f.UserId == msgList.QuestionerId);
                if (userQuestionner != null)
                {
                    avatar = userQuestionner.Avatar.Substring(1);
                    name = userQuestionner.Account;
                }
                else
                {
                    TempData["modalTitle"] = "對方帳號已遭移除。";
                    TempData["modalBody"] = "建議刪除此對話紀錄，謝謝您。";
                    return RedirectToAction("Index");
                }
            }

            if (msgList.ActivityId != null)
            {
                title = msgList.Activity.Title;
                titleId = msgList.ActivityId ?? default;
                section = 1;
            }
            else
            {
                title = msgList.Voting.Title;
                titleId = msgList.VotingId ?? default;
                section = 2;
            }

            var messages = new MessageViewModel
            {
                Messages = msgList.Messages.ToList(),
                CurrentUser = currentUser,
                MsgListId = id,
                Avatar = avatar,
                Name = name,
                Title = title,
                TitleId = titleId,
                SectionId = section
            };
            return View(messages);
        }

        // POST: MessageLists/Create
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost, ActionName("StartConversation")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MessageList messageList)
        {
            if (ModelState.IsValid)
            {
                if (messageList.QuestionerId == messageList.AnswererId)
                {
                    TempData["modalTitle"] = "你很俏皮喔。";
                    TempData["modalBody"] = "自己無法跟自己對話QQ";
                    if (messageList.ActivityId != null)
                    {
                        return Json(Url.Action("ApplicationForm", "Activities", new { id = messageList.ActivityId }));
                    }
                    else
                    {
                        return Json(Url.Action("VotingForm", "Votings", new { id = messageList.VotingId }));
                    }
                }
                if (messageList.ActivityId != null)
                {
                    var checkPresence = db.MessageLists.FirstOrDefault(f => f.QuestionerId == messageList.QuestionerId && f.AnswererId == messageList.AnswererId && f.ActivityId == messageList.ActivityId);
                    if (checkPresence != null)
                    {
                        return Json(Url.Action("Messages", new { id = checkPresence.MessageListId }));
                    }
                }
                else if (messageList.VotingId != null)
                {
                    var checkPresence = db.MessageLists.FirstOrDefault(f => f.QuestionerId == messageList.QuestionerId && f.AnswererId == messageList.AnswererId && f.VotingId == messageList.VotingId);
                    if (checkPresence != null)
                    {
                        return Json(Url.Action("Messages", new { id = checkPresence.MessageListId }));
                    }
                }
                messageList.IsVisible = true;
                db.MessageLists.Add(messageList);
                db.SaveChanges();
                return Json(Url.Action("Messages", new { id = messageList.MessageListId }));
            }

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
        public ActionResult Edit(MessageList messageList)
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

        // POST: MessageLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MessageList messageList = db.MessageLists.Find(id);
            messageList.IsVisible = false;
            db.SaveChanges();
            return Json(Url.Action("Index"));
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