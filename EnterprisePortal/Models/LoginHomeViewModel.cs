using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using static EnterprisePortal.Utils.Enum;

namespace EnterprisePortal.Models
{
    public class LoginHomeViewModel
    {
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "帳號")]
        public string Account { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; }

        [Display(Name = "記住我?")]
        public bool RememberMe { get; set; }
    }

    public class SendEmailViewModel
    {
        [Required(ErrorMessage = "{0}必填")]
        [EmailAddress(ErrorMessage = "{0}格式錯誤")]
        [Display(Name = "電子郵件")]
        public string Email { get; set; }
    }

    public class NavBarViewModel
    {
        public IEnumerable<QuickLinksCategory> QuickLinksCategory { get; set; }
    }

    public class HomePageViewModel
    {
        public IEnumerable<AllList> AllLists { get; set; }

        public IEnumerable<Announcement> Announcements { get; set; }

        public SettingsViewModel Settings { get; set; }

        public Activity Activity { get; set; }

        public Voting Voting { get; set; }
    }

    public class AllList
    {
        public int Id { get; set; }

        public ListType ListType { get; set; }

        [Display(Name = "標題")]
        public string Title { get; set; }

        [Display(Name = "狀態")]
        public ToDoListStatus ListStatus { get; set; }

        [Display(Name = "截止日期")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "發佈日期")]
        public DateTime PublishedDate { get; set; }
    }

    public class SettingsViewModel
    {
        [Display(Name = "Id")]
        public int UserId { get; set; }

        [Display(Name = "帳號名稱")]
        public string Account { get; set; }

        [Display(Name = "密碼")]
        [DataType(DataType.Password)]
        [StringLength(300, ErrorMessage = "密碼至少要5位數", MinimumLength = 5)]
        public string Password { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "手機號碼")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "手機號碼格式錯誤")]
        //regex "^(\d{10})$"
        [StringLength(50)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email必填")]
        [Display(Name = "電子郵件")]
        [EmailAddress(ErrorMessage = "Email格式錯誤")]
        [StringLength(50)]
        public string Email { get; set; }

        [Display(Name = "頭像")]
        public string Avatar { get; set; }

        [Display(Name = "部門")]
        public string Department { get; set; }

        [Display(Name = "加入時間")]
        public string JoinedDate { get; set; }
    }
}