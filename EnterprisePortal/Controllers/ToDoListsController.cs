using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EnterprisePortal.Models;
using EnterprisePortal.Utils;
using PagedList;
using static EnterprisePortal.Utils.Enum;

namespace EnterprisePortal.Controllers
{
    public class ToDoListsController : Controller
    {
        private readonly PortalModel db = new PortalModel();

        // GET: ToDoLists
        public ActionResult Index(int? page, string sortOrder, string currentFilter, string searchString, string filterStatus, string endTimeStart, string endTimeEnd, string publishedDateStart, string publishedDateEnd)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.EndTimeSortParm = sortOrder == "endDate" ? "endDate_desc" : "endDate";
            ViewBag.PublishedDateSortParm = sortOrder == "publishedDate" ? "publishedDate_desc" : "publishedDate";
            ViewBag.StatusSortParm = sortOrder == "status" ? "status_desc" : "status";

            if (searchString != null || filterStatus != null || endTimeStart != null || endTimeEnd != null || publishedDateStart != null || publishedDateEnd != null)
            {
                page = 1;
            }
            else
            {
                if (currentFilter != null)
                {
                    string[] filterSplit = currentFilter.Split('&');
                    searchString = filterSplit[0];
                    filterStatus = filterSplit[1].Split('=')[1];
                    if (currentFilter.Contains("endTimeStart") && currentFilter.Contains("endTimeEnd"))
                    {
                        endTimeStart = filterSplit[2].Split('=')[1];
                        endTimeEnd = filterSplit[3].Split('=')[1];
                    }
                    if (currentFilter.Contains("publishedDateStart") && currentFilter.Contains("publishedDateEnd"))
                    {
                        if (filterSplit.Length == 6)
                        {
                            publishedDateStart = filterSplit[4].Split('=')[1];
                            publishedDateEnd = filterSplit[5].Split('=')[1];
                        }
                        else
                        {
                            publishedDateStart = filterSplit[2].Split('=')[1];
                            publishedDateEnd = filterSplit[3].Split('=')[1];
                        }
                    }
                }
            }

            string currentFilterString = "";
            ViewBag.SearchString = searchString;
            ViewBag.FilterStatus = filterStatus;
            ViewBag.EndTimeStart = endTimeStart;
            ViewBag.EndTimeEnd = endTimeEnd;
            ViewBag.PublishedStart = publishedDateStart;
            ViewBag.PublishedEnd = publishedDateEnd;
            ViewBag.Status = new SelectList(new List<SelectListItem>
                                    {
                                        new SelectListItem { Text = "待處理", Value = "0" },
                                        new SelectListItem { Text = "處理中", Value = "1"},
                                        new SelectListItem { Text = "已完成", Value = "2"}
                                    }, "Value", "Text", filterStatus);

            int currentUserId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
            var toDoLists = db.ToDoLists.Where(w => w.UserId == currentUserId);

            if (!String.IsNullOrEmpty(searchString))
            {
                toDoLists = toDoLists.Where(s => s.Title.Contains(searchString));
                currentFilterString += searchString;
            }
            if (!String.IsNullOrEmpty(filterStatus))
            {
                int statusInteger = int.Parse(filterStatus);
                toDoLists = toDoLists.Where(f => (int)f.ListStatus == statusInteger);
                currentFilterString += "&filterStatus=" + filterStatus;
            }
            if (!String.IsNullOrEmpty(endTimeStart) && !String.IsNullOrEmpty(endTimeEnd))
            {
                DateTime startDate = DateTime.ParseExact(endTimeStart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(endTimeEnd, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                toDoLists = toDoLists.Where(e => e.EndTime >= startDate && e.EndTime <= endDate);
                currentFilterString += "&endTimeStart=" + endTimeStart + "&endTimeEnd=" + endTimeEnd;
            }
            if (!String.IsNullOrEmpty(publishedDateStart) && !String.IsNullOrEmpty(publishedDateEnd))
            {
                DateTime startpDate = DateTime.ParseExact(publishedDateStart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime endpDate = DateTime.ParseExact(publishedDateEnd, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                toDoLists = toDoLists.Where(p => p.PublishedDate >= startpDate && p.PublishedDate <= endpDate);
                currentFilterString += "&publishedDateStart=" + publishedDateStart + "&publishedDateEnd=" + publishedDateEnd;
            }
            ViewBag.CurrentFilter = currentFilterString;

            switch (sortOrder)
            {
                case "status_desc":
                    toDoLists = toDoLists.OrderByDescending(o => o.ListStatus);
                    break;

                case "status":
                    toDoLists = toDoLists.OrderBy(o => o.ListStatus);
                    break;

                case "endDate_desc":
                    toDoLists = toDoLists.OrderByDescending(o => o.EndTime);
                    break;

                case "endDate":
                    toDoLists = toDoLists.OrderBy(o => o.EndTime);
                    break;

                case "publishedDate":
                    toDoLists = toDoLists.OrderBy(o => o.PublishedDate);
                    break;

                default:  // publishedDate_desc
                    toDoLists = toDoLists.OrderByDescending(o => o.PublishedDate);
                    break;
            }

            int pageNumber = (page ?? 1);
            int pageSize = 10;
            Session["pageToDoList"] = page;
            return View(toDoLists.ToPagedList(pageNumber, pageSize));
        }

        // GET: ToDoLists/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ToDoList toDoList = db.ToDoLists.FirstOrDefault(f => f.ToDoListId == id);
            if (toDoList == null)
            {
                return HttpNotFound();
            }
            return View(toDoList);
        }

        // GET: ToDoLists/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ToDoLists/Create
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ToDoList toDoList)
        {
            if (ModelState.IsValid)
            {
                int currentUserId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
                toDoList.UserId = currentUserId;
                toDoList.PublishedDate = DateTime.Now;
                db.ToDoLists.Add(toDoList);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(toDoList);
        }

        // GET: ToDoLists/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ToDoList toDoList = db.ToDoLists.Find(id);
            if (toDoList == null)
            {
                return HttpNotFound();
            }
            //ViewBag.Page = page;
            return View(toDoList);
        }

        // POST: ToDoLists/Edit/5
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ToDoList toDoList)
        {
            if (ModelState.IsValid)
            {
                ToDoList thisToDoList = db.ToDoLists.FirstOrDefault(f => f.ToDoListId == toDoList.ToDoListId);
                thisToDoList.Title = toDoList.Title ?? thisToDoList.Title;
                thisToDoList.Summary = toDoList.Summary ?? thisToDoList.Summary;
                thisToDoList.EndTime = toDoList.EndTime;
                thisToDoList.ListStatus = toDoList.ListStatus;
                db.SaveChanges();

                return RedirectToAction("Index", new { page = Session["pageToDoList"] });
            }
            //ViewBag.UserId = new SelectList(db.UserAccounts, "UserId", "Account", toDoList.UserId);
            return View(toDoList);
        }

        // GET: ToDoLists/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ToDoList toDoList = db.ToDoLists.Find(id);
            if (toDoList == null)
            {
                return HttpNotFound();
            }
            return View(toDoList);
        }

        // POST: ToDoLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ToDoList toDoList = db.ToDoLists.Find(id);
            db.ToDoLists.Remove(toDoList);
            db.SaveChanges();

            return Json(Url.Action("Index", new { page = Session["pageToDoList"] }));
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