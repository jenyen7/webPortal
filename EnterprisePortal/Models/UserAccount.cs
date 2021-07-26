using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnterprisePortal.Models
{
    public class UserAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "帳號名稱")]
        [Remote("IsAccountRepeated", "UserAccounts", AdditionalFields = "InitialAccount", HttpMethod = "POST", ErrorMessage = "{0}已存在，請重新取名。")]
        [StringLength(50)]
        public string Account { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "密碼")]
        [DataType(DataType.Password)]
        [StringLength(300, ErrorMessage = "密碼至少要5位數", MinimumLength = 5)]
        public string Password { get; set; }

        [Display(Name = "密碼鹽")]
        [StringLength(200)]
        public string PasswordSalt { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "手機號碼")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "手機號碼格式錯誤")]
        [StringLength(50)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email必填")]
        [Display(Name = "電子郵件")]
        [EmailAddress(ErrorMessage = "Email格式錯誤")]
        [Remote("IsEmailRepeated", "UserAccounts", AdditionalFields = "InitialEmail", HttpMethod = "POST", ErrorMessage = "{0}已存在，請重新填寫。")]
        [StringLength(50)]
        public string Email { get; set; }

        [Display(Name = "頭像")]
        [StringLength(100)]
        public string Avatar { get; set; }

        [Display(Name = "帳號存取")]
        public bool IsEnable { get; set; }

        [Required(ErrorMessage = "{0}必選")]
        [Display(Name = "隸屬部門")]
        public int DepId { get; set; }

        [ForeignKey("DepId")]
        public virtual Department Department { get; set; }

        [Display(Name = "角色")]
        public virtual ICollection<UserRole> UserRoles { get; set; }

        [Display(Name = "帳號建立時間")]
        public DateTime JoinedDate { get; set; }

        public ICollection<ToDoList> ToDoLists { get; set; }

        public ICollection<ActivitiesApplication> ActivitiesApplications { get; set; }

        public ICollection<VotingForm> VotingForms { get; set; }
    }
}