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
    public class VotingsController : Controller
    {
        private readonly PortalModel db = new PortalModel();
        private readonly string folder = "Events";

        // GET: Votings
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

            var votings = db.Votings.Select(x => x);

            if (!String.IsNullOrEmpty(searchString))
            {
                votings = votings.Where(s => s.Title.Contains(searchString));
                currentFilterString += searchString;
            }
            if (!String.IsNullOrEmpty(filterStatus))
            {
                bool flag = filterStatus == "True";
                votings = votings.Where(f => f.VotingStatus == flag);
                currentFilterString += "&filterStatus=" + flag;
            }
            if (!String.IsNullOrEmpty(endTimeStart) && !String.IsNullOrEmpty(endTimeEnd))
            {
                DateTime startDate = DateTime.ParseExact(endTimeStart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(endTimeEnd, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                votings = votings.Where(e => e.EndTime >= startDate && e.EndTime <= endDate);
                currentFilterString += "&endTimeStart=" + endTimeStart + "&endTimeEnd=" + endTimeEnd;
            }
            if (!String.IsNullOrEmpty(publishedDateStart) && !String.IsNullOrEmpty(publishedDateEnd))
            {
                DateTime startpDate = DateTime.ParseExact(publishedDateStart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime endpDate = DateTime.ParseExact(publishedDateEnd, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                votings = votings.Where(p => p.PublishedDate >= startpDate && p.PublishedDate <= endpDate);
                currentFilterString += "&publishedDateStart=" + publishedDateStart + "&publishedDateEnd=" + publishedDateEnd;
            }
            ViewBag.CurrentFilter = currentFilterString;

            switch (sortOrder)
            {
                case "true":
                    votings = votings.OrderByDescending(o => o.VotingStatus);
                    break;

                case "false":
                    votings = votings.OrderBy(o => o.VotingStatus);
                    break;

                case "endDate_desc":
                    votings = votings.OrderByDescending(o => o.EndTime);
                    break;

                case "endDate":
                    votings = votings.OrderBy(o => o.EndTime);
                    break;

                case "publishedDate":
                    votings = votings.OrderBy(o => o.PublishedDate);
                    break;

                default:  // publishedDate_desc
                    votings = votings.OrderByDescending(o => o.PublishedDate);
                    break;
            }

            int pageNumber = (page ?? 1);
            int pageSize = 10;
            Session["pageVoting"] = page;
            return View(votings.ToPagedList(pageNumber, pageSize));
        }

        // GET: Votings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Voting voting = db.Votings.FirstOrDefault(f => f.VotingId == id);
            if (voting == null)
            {
                return HttpNotFound();
            }
            return View(voting);
        }

        public ActionResult VotingForm(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int currentUserId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
            VotingForm votingForm = db.VotingForms.FirstOrDefault(f => f.VotingId == id && f.UserId == currentUserId);
            if (votingForm == null)
            {
                return HttpNotFound();
            }
            if (votingForm.ListStatus == ToDoListStatus.已完成)
            {
                TempData["modalTitle"] = "您已投完票。";
                TempData["modalBody"] = "請靜待投票結果的出爐!";
            }
            return View(votingForm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VotingForm(int? id, string answers, string comments)
        {
            if (answers != null)
            {
                VotingForm votingForm = db.VotingForms.FirstOrDefault(f => f.VotingFormId == id);
                votingForm.ListStatus = ToDoListStatus.已完成;
                votingForm.Comments = comments;
                votingForm.DateCompleted = DateTime.Now;
                string[] theanswers = answers.Split(',');
                foreach (string answer in theanswers)
                {
                    int answerId = int.Parse(answer);
                    VotingResult result = new VotingResult
                    {
                        UserId = votingForm.UserId,
                        ContentId = answerId
                    };
                    db.VotingResults.Add(result);
                }
                db.SaveChanges();
                TempData["modalTitle"] = "投票已送出。";
                TempData["modalBody"] = "請靜待投票結果的出爐!";
                return Json(new
                {
                    success = true,
                    message = Url.Action("VotingForm", "Votings", new { id = votingForm.VotingId })
                });
            }
            return Json(new
            {
                success = false,
                message = "發生錯誤"
            });
        }

        public ActionResult VotingResult(int? id)
        {
            Voting voting = db.Votings.FirstOrDefault(f => f.VotingId == id);
            if (voting == null)
            {
                return HttpNotFound();
            }
            var resultList = new ResultsList();
            var result = new Results();
            int count = 0;

            if (voting.VotingContents.Any())
            {
                foreach (var item in voting.VotingContents)
                {
                    if (item.VoteNum != count)
                    {
                        count = item.VoteNum;
                        if (result.Label.Any())
                        {
                            resultList.Add(result);
                        }
                        result = new Results();
                        if (item.IsMultipleChoices)
                        {
                            result.ChartType = "bar";
                        }
                        else
                        {
                            result.ChartType = "doughnut";
                        }
                        result.QuestionText = item.VoteText;
                    }
                    else
                    {
                        result.Label.Add(item.VoteText);
                        result.Data.Add(item.VotingResults.Count);
                    }
                }
                resultList.Add(result);
            }

            VotingResultViewModel votingResult = new VotingResultViewModel
            {
                Voting = voting,
                Results = resultList
            };
            return View(votingResult);
        }

        public ActionResult ExportExcel(int id)
        {
            Voting voting = db.Votings.FirstOrDefault(f => f.VotingId == id);
            int currentUserId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
            if (voting.UserId != currentUserId)
            {
                TempData["warningMsg"] = "您沒有下載此檔案的權限，只有投票發起者才能下載記名結果。";
                return RedirectToAction("VotingResult", "Votings", new { id });
            }

            List<VotingContent> list = db.VotingContents.Where(w => w.VotingId == id).ToList();

            ExcelPackage ep = new ExcelPackage();
            ExcelWorksheet sheet = ep.Workbook.Worksheets.Add("VotingResults");

            int col = 1;
            sheet.Cells[1, col++].Value = "投票名稱";
            sheet.Cells[1, col++].Value = "題目編號";
            sheet.Cells[1, col++].Value = "題目/選項";
            sheet.Cells[1, col++].Value = "投票者";

            string votingTitle = list.First().Voting.Title;
            int count = 0;
            var users = db.UserAccounts.ToList();

            int row = 2;
            foreach (var item in list)
            {
                col = 1;
                sheet.Cells[row, col++].Value = votingTitle;
                if (item.VoteNum != count)
                {
                    count = item.VoteNum;
                    sheet.Cells[row, col++].Value = count;
                    sheet.Cells[row, col++].Value = "(第" + count + "題)" + item.VoteText;
                }
                else
                {
                    sheet.Cells[row, col++].Value = count;
                    sheet.Cells[row, col++].Value = item.VoteText;
                }
                var theUserIds = item.VotingResults.Join(users, v => v.UserId, u => u.UserId, (v, u) => u.Account);
                sheet.Cells[row, col++].Value = String.Join(",", theUserIds);
                row++;
            }

            List<VotingForm> formList = db.VotingForms.Where(w => w.VotingId == id).ToList();
            foreach (var filledForm in formList)
            {
                col = 1;
                sheet.Cells[row, col++].Value = votingTitle;
                sheet.Cells[row, col++].Value = "其他意見";
                sheet.Cells[row, col++].Value = filledForm.Comments;
                sheet.Cells[row, col++].Value = filledForm.UserAccount.Account;
                row++;
            }
            MemoryStream fileStream = new MemoryStream();
            ep.SaveAs(fileStream);
            ep.Dispose();
            fileStream.Position = 0;
            return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "VotingResults_" + votingTitle + ".xlsx");
        }

        // GET: Votings/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Votings/Create
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Voting voting, string editor, string croppedImage)
        {
            if (ModelState.IsValid)
            {
                int currentUserId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
                voting.Summary = editor;
                voting.VotingStatus = true;
                voting.UserId = currentUserId;
                voting.PublishedDate = DateTime.Now;
                string picture = ProcessImage(croppedImage);
                voting.Picture = string.IsNullOrEmpty(picture) ? DefaultImage((int)voting.ActivityType) : picture;

                db.Votings.Add(voting);
                db.SaveChanges();

                CurrentUser.RecordActivityWithSaveChanges(LogAction.新增, LogArea.投票, $"{voting.Title}(id= {voting.VotingId})");
                return RedirectToAction("Create", "VotingContents", new { id = voting.VotingId });
            }
            return View(voting);
        }

        // GET: Votings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Voting voting = db.Votings.Find(id);
            if (voting == null)
            {
                return HttpNotFound();
            }
            return View(voting);
        }

        // POST: Votings/Edit/5
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Voting voting, string editor, string croppedImage)
        {
            if (ModelState.IsValid)
            {
                Voting thisVoting = db.Votings.FirstOrDefault(f => f.VotingId == voting.VotingId);
                thisVoting.Title = voting.Title;
                thisVoting.Summary = editor;
                thisVoting.ActivityType = voting.ActivityType;
                thisVoting.VotingStatus = voting.VotingStatus;
                thisVoting.EndTime = voting.EndTime;
                string currentPicture = ProcessImage(croppedImage);
                thisVoting.Picture = string.IsNullOrEmpty(currentPicture) ? thisVoting.Picture : currentPicture;
                var log = CurrentUser.RecordActivity(LogAction.修改, LogArea.投票, $"{voting.Title}(id= {voting.VotingId})");
                db.UserLogs.Add(log);

                db.SaveChanges();
                return RedirectToAction("Index", new { page = Session["pageVoting"] });
            }
            return View(voting);
        }

        // GET: Votings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Voting voting = db.Votings.Find(id);
            if (voting == null)
            {
                return HttpNotFound();
            }
            return View(voting);
        }

        // POST: Votings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Voting voting = db.Votings.Find(id);
            db.Votings.Remove(voting);
            var messageLists = db.MessageLists.Where(w => w.VotingId == id);
            db.MessageLists.RemoveRange(messageLists);
            var log = CurrentUser.RecordActivity(LogAction.刪除, LogArea.投票, $"{voting.Title}(id= {voting.VotingId})");
            db.UserLogs.Add(log);

            db.SaveChanges();
            return Json(Url.Action("Index", new { page = Session["pageVoting"] }));
        }

        [HttpGet]
        public ActionResult VotesList(int? id)
        {
            var votes = db.VotingForms.Where(w => w.VotingId == id).ToList();
            return PartialView("_VotingFormsListPartial", votes);
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