using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using static EnterprisePortal.Utils.Enum;

namespace EnterprisePortal.Models
{
    public class Voting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VotingId { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "投票主題")]
        [StringLength(100)]
        public string Title { get; set; }

        [Display(Name = "投票目的")]
        [StringLength(1000)]
        public string Summary { get; set; }

        [Required(ErrorMessage = "{0}必選")]
        [Display(Name = "投票主題類型")]
        public ActivityType ActivityType { get; set; }

        [Display(Name = "開放/關閉投票")]
        public bool VotingStatus { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "投票截止日")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndTime { get; set; }

        [Display(Name = "封面圖片")]
        [StringLength(100)]
        public string Picture { get; set; }

        public virtual ICollection<VotingContent> VotingContents { get; set; }

        public virtual ICollection<VotingForm> VotingForms { get; set; }

        public virtual ICollection<MessageList> MessageLists { get; set; }

        [Display(Name = "選擇發送部門")]
        [StringLength(100)]
        public string DepartmentsIds { get; set; }

        [Display(Name = "選擇個別帳號")]
        [StringLength(200)]
        public string ColleaguesIds { get; set; }

        [Display(Name = "發佈時間")]
        public DateTime PublishedDate { get; set; }

        [Display(Name = "哪個人發的投票")]
        public int UserId { get; set; }
    }

    public class VotingDepartmentsViewModel
    {
        public Voting Voting { get; set; }

        public IEnumerable<DepartmentViewModel> DepartmentViewModels { get; set; }
    }
}