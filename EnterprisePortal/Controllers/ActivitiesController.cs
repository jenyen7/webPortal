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
        private readonly string folder = "Events";

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

        [HttpGet]
        public ActionResult ApplicationList(int id)
        {
            var applications = db.ActivitiesApplications.Where(w => w.ActivityId == id).Take(50);
            ViewBag.Total = applications.Count();
            return PartialView("_ApplicationsListPartial", applications.ToList());
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
            sheet.Cells[1, col++].Value = "是否已提交";
            sheet.Cells[1, col++].Value = "是否參加";
            sheet.Cells[1, col++].Value = "名字";
            sheet.Cells[1, col++].Value = "身分證字號";
            sheet.Cells[1, col++].Value = "電子郵件";
            sheet.Cells[1, col++].Value = "手機號碼";
            sheet.Cells[1, col++].Value = "是否素食";
            sheet.Cells[1, col++].Value = "是否家人陪同";
            sheet.Cells[1, col++].Value = "陪同人數";
            sheet.Cells[1, col++].Value = "交通方式";

            string activityTitle = list.First().Activity.Title;
            string isParticipating = "";
            string name = "";
            string email = "";
            string citizenId = "";
            string cellNum = "";

            int row = 2;
            foreach (var item in list)
            {
                col = 1;
                if ((int)item.ListStatus == 2)
                {
                    if (item.IsJoiningIn)
                    {
                        isParticipating = "參加";
                        name = item.Name;
                        citizenId = item.CitizenId;
                        email = item.Email;
                        cellNum = item.CellNumber;
                    }
                    else
                    {
                        isParticipating = "婉拒";
                    }
                }
                sheet.Cells[row, col++].Value = activityTitle;
                sheet.Cells[row, col++].Value = item.UserAccount.Department.DepartmentName;
                sheet.Cells[row, col++].Value = item.UserAccount.Account;
                sheet.Cells[row, col++].Value = item.ListStatus;
                sheet.Cells[row, col++].Value = isParticipating;
                sheet.Cells[row, col++].Value = name;
                sheet.Cells[row, col++].Value = citizenId;
                sheet.Cells[row, col++].Value = email;
                sheet.Cells[row, col++].Value = cellNum;
                sheet.Cells[row, col++].Value = item.IsVegetarian;
                sheet.Cells[row, col++].Value = item.IsAccompanied;
                sheet.Cells[row, col++].Value = item.PeopleNum;
                sheet.Cells[row, col++].Value = item.Transportation;

                row++;
                isParticipating = "";
                name = "";
                citizenId = "";
                email = "";
                cellNum = "";
            }
            MemoryStream fileStream = new MemoryStream();
            ep.SaveAs(fileStream);
            ep.Dispose();
            fileStream.Position = 0;
            return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ApplicationForms_" + activityTitle + ".xlsx");
        }

        public ActionResult ApplicationForm(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int currentUserId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
            ActivitiesApplication activitiesApplication = db.ActivitiesApplications.FirstOrDefault(f => f.ActivityId == id && f.UserId == currentUserId);
            if (activitiesApplication == null)
            {
                return HttpNotFound();
            }
            return View(activitiesApplication);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApplicationForm(ActivitiesApplication activitiesApplication)
        {
            //if (!Utility.IsValidEmail(activitiesApplication.Email) ||
            //    !Utility.IsValidCitizenId(activitiesApplication.CitizenId) ||
            //    !Utility.IsValidPhoneNumber(activitiesApplication.CellNumber))
            //{
            //    return View(applicationForm);
            //}
            ActivitiesApplication thisAppForm = db.ActivitiesApplications.FirstOrDefault(f => f.ApplicationId == activitiesApplication.ApplicationId);
            thisAppForm.ListStatus = ToDoListStatus.已完成;
            thisAppForm.IsJoiningIn = true;
            thisAppForm.CitizenId = thisAppForm.CitizenId ?? activitiesApplication.CitizenId;
            thisAppForm.Name = activitiesApplication.Name;
            thisAppForm.Email = activitiesApplication.Email;
            thisAppForm.CellNumber = activitiesApplication.CellNumber;
            thisAppForm.IsAccompanied = activitiesApplication.IsAccompanied;
            thisAppForm.PeopleNum = activitiesApplication.IsAccompanied ? activitiesApplication.PeopleNum : 0;
            thisAppForm.Transportation = activitiesApplication.Transportation;
            thisAppForm.IsVegetarian = activitiesApplication.IsVegetarian;
            thisAppForm.DateCompleted = DateTime.Now;
            db.SaveChanges();
            TempData["modalTitle"] = "報名成功。";
            TempData["modalBody"] = "如果填寫錯誤或者有其他問題，請洽詢主辦者，謝謝您~";

            return RedirectToAction("ApplicationForm", new { id = thisAppForm.ActivityId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApplicationRefused(int appsId)
        {
            ActivitiesApplication thisAppForm = db.ActivitiesApplications.FirstOrDefault(f => f.ApplicationId == appsId);
            thisAppForm.ListStatus = ToDoListStatus.已完成;
            thisAppForm.IsJoiningIn = false;
            thisAppForm.DateCompleted = DateTime.Now;
            db.SaveChanges();
            TempData["modalTitle"] = "婉拒成功。";
            TempData["modalBody"] = "如果改變主意或者有其他問題，請洽詢主辦者，謝謝您~";

            return Json(Url.Action("ApplicationForm", "Activities", new { id = thisAppForm.ActivityId }));
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
                thisActivity.Title = activity.Title;
                thisActivity.Summary = editor;
                thisActivity.ActivityType = activity.ActivityType;
                thisActivity.ActivityStatus = activity.ActivityStatus;
                thisActivity.EndTime = activity.EndTime;
                string currentPicture = ProcessImage(croppedImage);
                thisActivity.Picture = string.IsNullOrEmpty(currentPicture) ? thisActivity.Picture : currentPicture;
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
            var log = CurrentUser.RecordActivity(LogAction.刪除, LogArea.活動, $"{activity.Title}(id= {activity.ActivityId})");
            db.UserLogs.Add(log);

            db.SaveChanges();
            return Json(Url.Action("Index", new { page = Session["pageActivity"] }));
        }

        private string ProcessImage(string croppedImage)
        {
            if (!string.IsNullOrWhiteSpace(croppedImage))
            {
                string filePath = $"~/Upload/{folder}/{DateTime.Now:yyyyMMddhhmm}.png";
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
            string path = "~/Upload/" + folder + "/";
            string filename = $"event{type}.jpg";
            return path + filename;
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
            var ApplicationForms = DistributeApplicationForms(activity, data.Departments, data.UserIds, data.UserAccounts);

            activity.DepartmentsIds = GetDepsString(data.Departments);
            activity.ColleaguesIds = GetUserAccountsString(data.UserAccounts);

            ApplicationForms.ForEach(n => db.ActivitiesApplications.Add(n));
            db.SaveChanges();
            return RedirectToAction("Index", "Activities");
        }

        private List<ActivitiesApplication> DistributeApplicationForms(Activity activity, string[] deps, string[] userId, string[] userAccount)
        {
            string tempName = "tempJohnDoe";
            string tempId = "A123456789";
            string tempEmail = "temp@temp.com";
            string tempCell = "0912345678";

            var applicationForms = new List<ActivitiesApplication>();
            var IndivsAlreadySent = new List<int>();

            if (userAccount != null)
            {
                int total = 0;
                int totalLength = userAccount.Length;
                var accounts = db.UserAccounts.ToList();
                foreach (var item in accounts)
                {
                    if (userAccount.Contains(item.Account))
                    {
                        IndivsAlreadySent.Add(item.UserId);
                        total++;
                    }
                    if (total == totalLength)
                    {
                        break;
                    }
                }
            }

            if (userId != null)
            {
                foreach (string searchUser in userId)
                {
                    //check if user already received form
                    int chosenUserId = int.Parse(searchUser);
                    var formSent = db.ActivitiesApplications.FirstOrDefault(f => f.UserId == chosenUserId && f.ActivityId == activity.ActivityId);
                    if (formSent == null)
                    {
                        ActivitiesApplication applicationForm = new ActivitiesApplication
                        {
                            UserId = chosenUserId,
                            ActivityId = activity.ActivityId,
                            Name = tempName,
                            CitizenId = tempId,
                            Email = tempEmail,
                            CellNumber = tempCell
                        };
                        applicationForms.Add(applicationForm);
                    }
                }
            }

            if (deps != null)
            {
                //not first time sent
                if (activity.DepartmentsIds != null)
                {
                    string[] FormsAlreadySent = activity.DepartmentsIds.Split(',');
                    foreach (string dep in deps)
                    {
                        bool IsDepSent = FormsAlreadySent.Contains(dep);
                        if (!IsDepSent)
                        {
                            int intDep = int.Parse(dep);
                            var chosenDep = db.Departments.FirstOrDefault(f => f.DepartmentId == intDep);
                            foreach (var user in chosenDep.UserAccounts)
                            {
                                bool IsIndivSent = IndivsAlreadySent.Contains(user.UserId);
                                if (!IsIndivSent)
                                {
                                    ActivitiesApplication applicationForm = new ActivitiesApplication
                                    {
                                        UserId = user.UserId,
                                        ActivityId = activity.ActivityId,
                                        Name = tempName,
                                        CitizenId = tempId,
                                        Email = tempEmail,
                                        CellNumber = tempCell
                                    };
                                    applicationForms.Add(applicationForm);
                                }
                            }
                        }
                    }
                }
                //first time sent
                else
                {
                    foreach (string dep in deps)
                    {
                        int intDep = int.Parse(dep);
                        var chosenDep = db.Departments.FirstOrDefault(f => f.DepartmentId == intDep);
                        foreach (var user in chosenDep.UserAccounts)
                        {
                            bool IsIndivSent = IndivsAlreadySent.Contains(user.UserId);
                            if (!IsIndivSent)
                            {
                                ActivitiesApplication applicationForm = new ActivitiesApplication
                                {
                                    UserId = user.UserId,
                                    ActivityId = activity.ActivityId,
                                    Name = tempName,
                                    CitizenId = tempId,
                                    Email = tempEmail,
                                    CellNumber = tempCell
                                };
                                applicationForms.Add(applicationForm);
                            }
                        }
                    }
                }
            }

            return applicationForms;
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

        private string GetUserAccountsString(string[] searchUsers)
        {
            string result = string.Empty;
            if (searchUsers != null)
            {
                foreach (string item in searchUsers)
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