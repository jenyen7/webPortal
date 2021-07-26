using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EnterprisePortal.Models;
using EnterprisePortal.Utils;
using OfficeOpenXml;
using PagedList;
using static EnterprisePortal.Utils.Enum;

namespace EnterprisePortal.Controllers
{
    public class ActivitiesController : Controller
    {
        private readonly PortalModel db = new PortalModel();
        private readonly string eventFolder = ConfigurationManager.AppSettings["routeEventsFolder"];

        // GET: Activities
        public ActionResult Index(int? page, string sortOrder, string currentFilter, string searchString, string filterStatus, string endTimeStart, string endTimeEnd, string publishedDateStart, string publishedDateEnd)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.EndTimeSortParm = sortOrder == "endDate" ? "endDate_desc" : "endDate";
            ViewBag.PublishedDateSortParm = sortOrder == "publishedDate" ? "publishedDate_desc" : "publishedDate";
            ViewBag.StatusSortParm = sortOrder == "true" ? "false" : "true";

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
                                        new SelectListItem { Text = "進行中", Value = "True" },
                                        new SelectListItem { Text = "已結束", Value = "False"}
                                    }, "Value", "Text", filterStatus);

            var activities = db.Activities.Select(x => x);

            if (!String.IsNullOrEmpty(searchString))
            {
                activities = activities.Where(s => s.Title.Contains(searchString));
                currentFilterString += searchString;
            }
            if (!String.IsNullOrEmpty(filterStatus))
            {
                bool flag = filterStatus == "True";
                activities = activities.Where(f => f.ActivityStatus == flag);
                currentFilterString += "&filterStatus=" + flag;
            }
            if (!String.IsNullOrEmpty(endTimeStart) && !String.IsNullOrEmpty(endTimeEnd))
            {
                DateTime startDate = DateTime.ParseExact(endTimeStart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(endTimeEnd, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                activities = activities.Where(e => e.EndTime >= startDate && e.EndTime <= endDate);
                currentFilterString += "&endTimeStart=" + endTimeStart + "&endTimeEnd=" + endTimeEnd;
            }
            if (!String.IsNullOrEmpty(publishedDateStart) && !String.IsNullOrEmpty(publishedDateEnd))
            {
                DateTime startpDate = DateTime.ParseExact(publishedDateStart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime endpDate = DateTime.ParseExact(publishedDateEnd, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                activities = activities.Where(p => p.PublishedDate >= startpDate && p.PublishedDate <= endpDate);
                currentFilterString += "&publishedDateStart=" + publishedDateStart + "&publishedDateEnd=" + publishedDateEnd;
            }
            ViewBag.CurrentFilter = currentFilterString;

            switch (sortOrder)
            {
                case "true":
                    activities = activities.OrderByDescending(o => o.ActivityStatus);
                    break;

                case "false":
                    activities = activities.OrderBy(o => o.ActivityStatus);
                    break;

                case "endDate_desc":
                    activities = activities.OrderByDescending(o => o.EndTime);
                    break;

                case "endDate":
                    activities = activities.OrderBy(o => o.EndTime);
                    break;

                case "publishedDate":
                    activities = activities.OrderBy(o => o.PublishedDate);
                    break;

                default:  // publishedDate_desc
                    activities = activities.OrderByDescending(o => o.PublishedDate);
                    break;
            }

            int pageNumber = (page ?? 1);
            int pageSize = 10;
            Session["pageActivity"] = pageNumber;
            return View(activities.ToPagedList(pageNumber, pageSize));
        }

        // GET: Activities/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Activity activity = db.Activities.FirstOrDefault(f => f.ActivityId == id);
            if (activity == null)
            {
                return HttpNotFound();
            }
            return View(activity);
        }

        public ActionResult ExportExcel(int id)
        {
            Activity activity = db.Activities.FirstOrDefault(f => f.ActivityId == id);
            int currentUserId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
            if (activity.UserId != currentUserId)
            {
                TempData["warningMsg"] = "您沒有下載此檔案的權限，只有活動發起者才能下載報名資料。";
                return RedirectToAction("Edit", "Activities", new { id });
            }

            List<ActivitiesApplication> list = db.ActivitiesApplications.Where(w => w.ActivityId == id).ToList();

            ExcelPackage ep = new ExcelPackage();
            ExcelWorksheet sheet = ep.Workbook.Worksheets.Add("ApplicationForms");

            int col = 1;
            sheet.Cells[1, col++].Value = "活動名稱";
            sheet.Cells[1, col++].Value = "部門名稱";
            sheet.Cells[1, col++].Value = "帳號";
            sheet.Cells[1, col++].Value = "名字";
            sheet.Cells[1, col++].Value = "身分證字號";
            sheet.Cells[1, col++].Value = "電子郵件";
            sheet.Cells[1, col++].Value = "手機號碼";
            sheet.Cells[1, col++].Value = "是否素食";
            sheet.Cells[1, col++].Value = "是否家人陪同";
            sheet.Cells[1, col++].Value = "陪同人數";
            sheet.Cells[1, col++].Value = "交通方式";
            sheet.Cells[1, col++].Value = "填寫日期";

            string activityTitle = list.First().Activity.Title;

            int row = 2;
            foreach (var item in list)
            {
                col = 1;

                sheet.Cells[row, col++].Value = activityTitle;
                sheet.Cells[row, col++].Value = item.UserAccount.Department.DepartmentName;
                sheet.Cells[row, col++].Value = item.UserAccount.Account;
                sheet.Cells[row, col++].Value = item.Name;
                sheet.Cells[row, col++].Value = item.CitizenId;
                sheet.Cells[row, col++].Value = item.Email;
                sheet.Cells[row, col++].Value = item.CellNumber;
                sheet.Cells[row, col++].Value = item.IsVegetarian;
                sheet.Cells[row, col++].Value = item.IsAccompanied;
                sheet.Cells[row, col++].Value = item.PeopleNum;
                sheet.Cells[row, col++].Value = item.Transportation;
                sheet.Cells[row, col++].Value = item.DateCompleted;

                row++;
            }
            MemoryStream fileStream = new MemoryStream();
            ep.SaveAs(fileStream);
            ep.Dispose();
            fileStream.Position = 0;
            return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ApplicationForms_" + activityTitle + ".xlsx");
        }

        [HttpGet]
        public ActionResult ApplicationList(int id)
        {
            var applications = db.ActivitiesApplications.Where(w => w.ActivityId == id);
            ViewBag.Total = applications.Count();
            return PartialView("_ApplicationsListPartial", applications.ToList());
        }

        public ActionResult ApplicationForm(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int currentUserId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
            ToDoList toDoList = db.ToDoLists.FirstOrDefault(f => f.UserId == currentUserId && f.Summary.Equals(id.ToString()));
            Activity activity = db.Activities.FirstOrDefault(f => f.ActivityId == id);
            ActivitiesApplication activitiesApplication = db.ActivitiesApplications.FirstOrDefault(f => f.ActivityId == id && f.UserId == currentUserId);
            activity.Picture = eventFolder + activity.Picture;
            if (activity == null)
            {
                return HttpNotFound();
            }
            var activityApplicationFormViewModel = new ActivityApplicationFormViewModel
            {
                ActivitiesApplication = activitiesApplication,
                Activity = activity,
                ToDoList = toDoList
            };
            return View(activityApplicationFormViewModel);
        }

        [HttpPost, ActionName("ApplicationForm")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateApplicationForm(ActivitiesApplication activitiesApplication, int toDoListId, int userId, int activityId)
        {
            if (!Utility.IsValidEmail(activitiesApplication.Email) ||
                //!Utility.IsValidCitizenId(activitiesApplication.CitizenId) ||
                !Utility.IsValidPhoneNumber(activitiesApplication.CellNumber))
            {
                TempData["modalTitle"] = "資料不符合規格。";
                TempData["modalBody"] = "電子郵件或手機號碼不符合規格，請重新填寫。";
                return RedirectToAction("ApplicationForm", new { id = activityId });
            }
            activitiesApplication.ActivityId = activityId;
            activitiesApplication.UserId = userId;
            activitiesApplication.CitizenId = activitiesApplication.CitizenId ?? "not_required";
            activitiesApplication.PeopleNum = activitiesApplication.IsAccompanied ? activitiesApplication.PeopleNum : 0;
            activitiesApplication.DateCompleted = DateTime.Now;
            db.ActivitiesApplications.Add(activitiesApplication);

            ToDoList toDoList = db.ToDoLists.FirstOrDefault(f => f.ToDoListId == toDoListId);
            toDoList.ListStatus = ToDoListStatus.已完成;

            db.SaveChanges();
            TempData["modalTitle"] = "報名成功。";
            TempData["modalBody"] = "如果填寫錯誤或者有其他問題，請洽詢主辦者，謝謝您~";

            return RedirectToAction("ApplicationForm", new { id = activitiesApplication.ActivityId });
        }

        // GET: Activities/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public JsonResult SearchAccount(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Json(new { success = false, message = "您沒有輸入Email。" }, JsonRequestBehavior.AllowGet);

            var user = db.UserAccounts.FirstOrDefault(f => f.Email.Equals(email));
            if (user == null)
                return Json(new { success = false, message = "此email不存在。" }, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                success = true,
                hiddenId = user.UserId,
                accounts = user.Account,
                accountDep = user.DepId
            }, JsonRequestBehavior.AllowGet);
        }

        // POST: Activities/Create
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Activity activity, string editor, string croppedImage)
        {
            if (ModelState.IsValid)
            {
                int currentUserId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
                activity.Summary = editor;
                activity.ActivityStatus = true;
                activity.UserId = currentUserId;
                activity.PublishedDate = DateTime.Now;
                string picture = ProcessImage(croppedImage);
                activity.Picture = string.IsNullOrEmpty(picture) ? DefaultImage((int)activity.ActivityType) : picture;

                db.Activities.Add(activity);
                db.SaveChanges();

                CurrentUser.RecordActivityWithSaveChanges(LogAction.新增, LogArea.活動, $"{activity.Title}(id= {activity.ActivityId})");
                return Json(activity.ActivityId);
            }
            return View(activity);
        }

        // GET: Activities/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Activity activity = db.Activities.Find(id);
            if (activity == null)
            {
                return HttpNotFound();
            }

            return View(activity);
        }

        // POST: Activities/Edit/5
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Activity activity, string editor, string croppedImage)
        {
            if (ModelState.IsValid)
            {
                Activity thisActivity = db.Activities.FirstOrDefault(f => f.ActivityId == activity.ActivityId);
                thisActivity.Title = activity.Title ?? thisActivity.Title;
                thisActivity.Summary = editor ?? thisActivity.Summary;
                thisActivity.ActivityType = activity.ActivityType;
                thisActivity.ActivityStatus = activity.ActivityStatus;
                thisActivity.EndTime = activity.EndTime;
                string currentPicture = ProcessImage(croppedImage);
                thisActivity.Picture = String.IsNullOrEmpty(currentPicture) ? thisActivity.Picture : currentPicture;
                var log = CurrentUser.RecordActivity(LogAction.修改, LogArea.活動, $"{activity.Title}(id= {activity.ActivityId})");
                db.UserLogs.Add(log);

                db.SaveChanges();
                return RedirectToAction("Index", new { page = Session["pageActivity"] });
            }
            return View(activity);
        }

        // GET: Activities/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Activity activity = db.Activities.Find(id);
            if (activity == null)
            {
                return HttpNotFound();
            }
            return View(activity);
        }

        // POST: Activities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Activity activity = db.Activities.Find(id);
            db.Activities.Remove(activity);
            var messageLists = db.MessageLists.Where(w => w.ActivityId == id);
            db.MessageLists.RemoveRange(messageLists);
            var log = CurrentUser.RecordActivity(LogAction.刪除, LogArea.活動, $"{activity.Title}(id= {activity.ActivityId})");
            db.UserLogs.Add(log);

            db.SaveChanges();
            return Json(Url.Action("Index", new { page = Session["pageActivity"] }));
        }

        private string ProcessImage(string croppedImage)
        {
            if (!string.IsNullOrWhiteSpace(croppedImage))
            {
                string filePath = "~" + eventFolder + DateTime.Now.ToString("yyyyMMddhhmm") + ".png";
                string base64 = croppedImage;
                byte[] bytes = Convert.FromBase64String(base64.Split(',')[1]);
                using (FileStream stream = new FileStream(Server.MapPath(filePath), FileMode.Create))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }
                return filePath;
            }
            return string.Empty;
        }

        private string DefaultImage(int type)
        {
            string filename = $"event{type}.jpg";
            return filename;
        }

        [HttpGet]
        public ActionResult RenderRecipients(int? id)
        {
            var departments = new List<DepartmentViewModel>();
            var departmentsAndIndiv = new ActivityDepartmentsViewModel();

            Activity activity = db.Activities.FirstOrDefault(f => f.ActivityId == id);
            departmentsAndIndiv.Activity = activity;
            if (activity.DepartmentsIds != null)
            {
                string[] selectedDeps = activity.DepartmentsIds.Split(',');
                foreach (var item in db.Departments)
                {
                    DepartmentViewModel department = new DepartmentViewModel()
                    {
                        DepName = item.DepartmentName,
                        DepValue = item.DepartmentId,
                        IsChecked = selectedDeps.Contains(item.DepartmentId.ToString())
                    };
                    departments.Add(department);
                }
                departmentsAndIndiv.DepartmentViewModels = departments;
            }
            else
            {
                foreach (var item in db.Departments)
                {
                    DepartmentViewModel department = new DepartmentViewModel()
                    {
                        DepName = item.DepartmentName,
                        DepValue = item.DepartmentId,
                        IsChecked = false
                    };
                    departments.Add(department);
                }
                departmentsAndIndiv.DepartmentViewModels = departments;
            }
            return PartialView("_SendFormsPartial", departmentsAndIndiv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendGroup(int? activityId, DepartmentDataModel data)
        {
            Activity activity = db.Activities.FirstOrDefault(f => f.ActivityId == activityId);
            activity.DepartmentsIds = GetDepsString(data.Departments);
            activity.ColleaguesIds = GetUserAccountsString(data.UserAccounts, data.UserIds);

            db.SaveChanges();
            return RedirectToAction("Index", "Activities");
        }

        private string GetDepsString(string[] deps)
        {
            string result = string.Empty;
            if (deps != null)
            {
                foreach (string item in deps)
                {
                    result += item + ",";
                }
            }
            return result.TrimEnd(',');
        }

        private string GetUserAccountsString(string[] accounts, string[] userIds)
        {
            string result = string.Empty;
            if (accounts != null)
            {
                foreach (string item in accounts)
                {
                    result += item + ",";
                }
            }
            return result.TrimEnd(',');
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