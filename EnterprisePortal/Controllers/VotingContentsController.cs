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
    public class VotingContentsController : Controller
    {
        private PortalModel db = new PortalModel();

        // GET: VotingContents
        public ActionResult Index()
        {
            var votingContents = db.VotingContents.Include(v => v.Voting);
            return View(votingContents.ToList());
        }

        // GET: VotingContents/Create
        public ActionResult Create(int? id)
        {
            ViewBag.VotingId = id;
            return View();
        }

        // POST: VotingContents/Create
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int? id, string allQuestions)
        {
            if (!string.IsNullOrWhiteSpace(allQuestions))
            {
                int titleNum = 0;
                int votingId = Convert.ToInt16(id);
                bool multiple = false;
                string questionType;
                string[] questions = allQuestions.Split(',');

                for (int i = 0; i < questions.Length; i++)
                {
                    VotingContent content = new VotingContent();
                    if (questions[i] == "radioBtn" || questions[i] == "checkBoxes")
                    {
                        questionType = questions[i];
                        if (questionType == "checkBoxes")
                        {
                            multiple = true;
                        }
                        else
                        {
                            multiple = false;
                        }
                        titleNum++;
                    }
                    else
                    {
                        content.IsMultipleChoices = multiple;
                        content.VotingId = votingId;
                        content.VoteNum = titleNum;
                        content.VoteText = questions[i];
                        db.VotingContents.Add(content);
                    }
                }
                db.SaveChanges();
            }
            return Json(id);
        }

        // GET: VotingContents/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var votingContent = db.VotingContents.Where(w => w.VotingId == id);
            if (votingContent == null)
            {
                return HttpNotFound();
            }
            ViewBag.VotingId = id;
            TempData["modalTitle"] = "請注意!";
            TempData["modalBody"] = "隨意編輯問題將有可能會影響投票結果的準確性，如確定要繼續請按'關閉'鍵，否則請回上一頁。";

            return View(votingContent.ToList());
        }

        // POST: VotingContents/Edit/5
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, string allQuestions)
        {
            //var votingContents = db.VotingContents.Where(w => w.VotingId == id).ToList();
            //votingContents.ForEach(n => db.VotingContents.Remove(n));
            //db.SaveChanges();

            string sql = @"DELETE FROM VotingContents WHERE VotingId = @Id";
            using (var context = new PortalModel())
            {
                context.Database.ExecuteSqlCommand(sql, new System.Data.SqlClient.SqlParameter("@Id", id));
            };
            if (!string.IsNullOrWhiteSpace(allQuestions))
            {
                int titleNum = 0;
                int votingId = Convert.ToInt16(id);
                bool multiple = false;
                string questionType;
                string[] questions = allQuestions.Split(',');

                for (int i = 0; i < questions.Length; i++)
                {
                    VotingContent content = new VotingContent();
                    if (questions[i] == "radioBtn" || questions[i] == "checkBoxes")
                    {
                        questionType = questions[i];
                        if (questionType == "checkBoxes")
                        {
                            multiple = true;
                        }
                        else
                        {
                            multiple = false;
                        }
                        titleNum++;
                    }
                    else
                    {
                        content.IsMultipleChoices = multiple;
                        content.VotingId = votingId;
                        content.VoteNum = titleNum;
                        content.VoteText = questions[i];
                        db.VotingContents.Add(content);
                    }
                }
                db.SaveChanges();
            }
            return Json(Url.Action("Edit", "Votings", new { id }));
        }

        [ChildActionOnly]
        public ActionResult RenderRecipients(int? id)
        {
            var departments = new List<DepartmentViewModel>();
            var departmentsAndIndiv = new VotingDepartmentsViewModel();

            Voting voting = db.Votings.FirstOrDefault(f => f.VotingId == id);
            departmentsAndIndiv.Voting = voting;
            if (voting.DepartmentsIds != null)
            {
                string[] selectedDeps = voting.DepartmentsIds.Split(',');
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
            return PartialView("_SendVotesPartial", departmentsAndIndiv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendGroup(int? votingId, DepartmentDataModel data)
        {
            Voting voting = db.Votings.FirstOrDefault(f => f.VotingId == votingId);
            var VotingForms = DistributeVotingForms(voting, data.Departments, data.UserIds, data.UserAccounts);

            voting.DepartmentsIds = GetDepsString(data.Departments);
            voting.ColleaguesIds = GetUserAccountsString(data.UserAccounts);

            VotingForms.ForEach(n => db.VotingForms.Add(n));
            db.SaveChanges();
            return RedirectToAction("Index", "Votings");
        }

        private List<VotingForm> DistributeVotingForms(Voting voting, string[] deps, string[] userId, string[] userAccount)
        {
            var votingForms = new List<VotingForm>();
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
                    //check if user already received vote form
                    int chosenUserId = int.Parse(searchUser);
                    var votingSent = db.VotingForms.FirstOrDefault(f => f.UserId == chosenUserId && f.VotingId == voting.VotingId);
                    if (votingSent == null)
                    {
                        VotingForm votingForm = new VotingForm
                        {
                            UserId = chosenUserId,
                            VotingId = voting.VotingId
                        };
                        votingForms.Add(votingForm);
                    }
                }
            }

            if (deps != null)
            {
                //already sent, resend
                if (!String.IsNullOrEmpty(voting.DepartmentsIds))
                {
                    string[] DepsAlreadySent = voting.DepartmentsIds.Split(',');
                    foreach (string dep in deps)
                    {
                        bool IsDepSent = DepsAlreadySent.Contains(dep);
                        if (!IsDepSent)
                        {
                            int intDep = int.Parse(dep);
                            var chosenDep = db.Departments.FirstOrDefault(f => f.DepartmentId == intDep);
                            foreach (var user in chosenDep.UserAccounts)
                            {
                                bool IsIndivSent = IndivsAlreadySent.Contains(user.UserId);
                                if (!IsIndivSent)
                                {
                                    VotingForm votingForm = new VotingForm
                                    {
                                        UserId = user.UserId,
                                        VotingId = voting.VotingId
                                    };
                                    votingForms.Add(votingForm);
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
                                VotingForm votingForm = new VotingForm
                                {
                                    UserId = user.UserId,
                                    VotingId = voting.VotingId
                                };
                                votingForms.Add(votingForm);
                            }
                        }
                    }
                }
            }

            return votingForms;
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

        private string GetUserAccountsString(string[] userAccounts)
        {
            string result = string.Empty;
            if (userAccounts != null)
            {
                foreach (string item in userAccounts)
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