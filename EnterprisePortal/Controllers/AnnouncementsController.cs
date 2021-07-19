using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using EnterprisePortal.Models;
using EnterprisePortal.Utils;
using OfficeOpenXml;
using PagedList;
using static EnterprisePortal.Utils.Enum;

namespace EnterprisePortal.Controllers
{
    public class AnnouncementsController : Controller
    {
        private readonly PortalModel db = new PortalModel();

        // GET: Announcements
        public ActionResult Index(int? page, string sortOrder, string currentFilter, string searchString, string filterStatus, string publishedDateStart, string publishedDateEnd)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.PublishedDateSortParm = sortOrder == "publishedDate" ? "publishedDate_desc" : "publishedDate";
            ViewBag.ViewsSortParm = sortOrder == "true" ? "false" : "true";

            if (searchString != null || filterStatus != null || publishedDateStart != null || publishedDateEnd != null)
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
                    if (currentFilter.Contains("publishedDateStart") && currentFilter.Contains("publishedDateEnd"))
                    {
                        publishedDateStart = filterSplit[2].Split('=')[1];
                        publishedDateEnd = filterSplit[3].Split('=')[1];
                    }
                }
            }

            string currentFilterString = "";
            ViewBag.SearchString = searchString;
            ViewBag.FilterStatus = filterStatus;
            ViewBag.PublishedStart = publishedDateStart;
            ViewBag.PublishedEnd = publishedDateEnd;
            ViewBag.PublicStatus = new SelectList(new List<SelectListItem>
                                    {
                                        new SelectListItem { Text = "單位", Value = "Dep" },
                                        new SelectListItem { Text = "全域", Value = "All"}
                                    }, "Value", "Text", filterStatus);

            string data = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Ticket.UserData;
            CurrentUser user = Newtonsoft.Json.JsonConvert.DeserializeObject<CurrentUser>(data);
            var announcements = db.Announcements.Where(w => w.DepId == user.Department);

            if (!String.IsNullOrEmpty(searchString))
            {
                announcements = announcements.Where(s => s.Title.Contains(searchString));
                currentFilterString += searchString;
            }
            if (!String.IsNullOrEmpty(filterStatus))
            {
                bool flag = filterStatus == "All";
                announcements = announcements.Where(f => f.IsPublic == flag);
                currentFilterString += "&filterStatus=" + filterStatus;
            }
            if (!String.IsNullOrEmpty(publishedDateStart) && !String.IsNullOrEmpty(publishedDateEnd))
            {
                DateTime startpDate = DateTime.ParseExact(publishedDateStart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime endpDate = DateTime.ParseExact(publishedDateEnd, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                announcements = announcements.Where(e => e.PublishedDate >= startpDate && e.PublishedDate <= endpDate);
                currentFilterString += "&publishedDateStart=" + publishedDateStart + "&publishedDateEnd=" + publishedDateEnd;
            }
            ViewBag.CurrentFilter = currentFilterString;

            switch (sortOrder)
            {
                case "true":
                    announcements = announcements.OrderByDescending(o => o.ViewNum);
                    break;

                case "false":
                    announcements = announcements.OrderBy(o => o.ViewNum);
                    break;

                case "publishedDate":
                    announcements = announcements.OrderBy(o => o.PublishedDate);
                    break;

                default:  // publishedDate_desc
                    announcements = announcements.OrderByDescending(o => o.PublishedDate);
                    break;
            }

            int pageNumber = (page ?? 1);
            int pageSize = 10;
            Session["pageAnnouncement"] = page;
            return View(announcements.ToPagedList(pageNumber, pageSize));
        }

        // GET: Announcements/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Announcement announcement = db.Announcements.FirstOrDefault(f => f.AnnouncementId == id);
            if (announcement == null)
            {
                return HttpNotFound();
            }
            announcement.ViewNum += 1;
            db.SaveChanges();
            return View(announcement);
        }

        // GET: Announcements/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Announcements/Create
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Announcement announcement, string editor)
        {
            if (String.IsNullOrWhiteSpace(announcement.Title) || String.IsNullOrWhiteSpace(editor))
            {
                TempData["message"] = "內容必填";
                return View(announcement);
            }

            string data = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Ticket.UserData;
            CurrentUser user = Newtonsoft.Json.JsonConvert.DeserializeObject<CurrentUser>(data);
            announcement.DepId = user.Department;
            announcement.AnnouncementContent = editor;
            announcement.PublishedDate = DateTime.Now;
            db.Announcements.Add(announcement);
            db.SaveChanges();

            CurrentUser.RecordActivityWithSaveChanges(LogAction.新增, LogArea.公告, $"{announcement.Title}(id= {announcement.AnnouncementId})");
            return RedirectToAction("Index");
        }

        // GET: Announcements/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Announcement announcement = db.Announcements.Find(id);
            if (announcement == null)
            {
                return HttpNotFound();
            }

            return View(announcement);
        }

        // POST: Announcements/Edit/5
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Announcement announcement, string editor)
        {
            Announcement thisAnnouncement = db.Announcements.FirstOrDefault(f => f.AnnouncementId == announcement.AnnouncementId);
            thisAnnouncement.IsPublic = announcement.IsPublic;
            thisAnnouncement.Title = announcement.Title ?? thisAnnouncement.Title;
            thisAnnouncement.AnnouncementContent = editor ?? thisAnnouncement.AnnouncementContent;
            var log = CurrentUser.RecordActivity(LogAction.修改, LogArea.公告, $"{announcement.Title}(id= {announcement.AnnouncementId})");
            db.UserLogs.Add(log);

            db.SaveChanges();
            return RedirectToAction("Index", new { page = Session["pageAnnouncement"] });
            //ViewBag.DepId = new SelectList(db.Departments, "DepartmentId", "DepartmentName", announcement.DepId);
        }

        // GET: Announcements/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Announcement announcement = db.Announcements.Find(id);
            if (announcement == null)
            {
                return HttpNotFound();
            }
            return View(announcement);
        }

        // POST: Announcements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Announcement announcement = db.Announcements.Find(id);
            db.Announcements.Remove(announcement);
            var log = CurrentUser.RecordActivity(LogAction.刪除, LogArea.公告, $"{announcement.Title}(id= {announcement.AnnouncementId})");
            db.UserLogs.Add(log);

            db.SaveChanges();
            return Json(Url.Action("Index", new { page = Session["pageAnnouncement"] }));
        }

        public ActionResult ExportExcel()
        {
            List<Announcement> announcementList = db.Announcements.ToList();

            ExcelPackage ep = new ExcelPackage();
            ExcelWorksheet sheet = ep.Workbook.Worksheets.Add("Announcements");

            //第1列是標題列
            int col = 1;
            sheet.Cells[1, col++].Value = "IsPublic";
            sheet.Cells[1, col++].Value = "Title";
            sheet.Cells[1, col++].Value = "Views";

            //資料從第2列開始
            int row = 2;
            foreach (Announcement item in announcementList)
            {
                col = 1;//每換一列，欄位要從1開始
                sheet.Cells[row, col++].Value = item.IsPublic;
                sheet.Cells[row, col++].Value = item.Title;
                sheet.Cells[row, col++].Value = item.ViewNum;
                row++;
            }

            //因為ep.Stream是加密過的串流，故要透過SaveAs將資料寫到MemoryStream，在將MemoryStream使用FileStreamResult回傳到前端
            MemoryStream fileStream = new MemoryStream();
            ep.SaveAs(fileStream);
            ep.Dispose();//如果這邊不下Dispose，建議此ep要用using包起來，但是要記得先將資料寫進MemoryStream在Dispose。
            fileStream.Position = 0;//不重新將位置設為0，excel開啟後會出現錯誤
            return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExportAnnouncements.xlsx");
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