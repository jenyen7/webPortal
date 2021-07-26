using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using static EnterprisePortal.Utils.Enum;

namespace EnterprisePortal.Models
{
    public class ActivitiesApplication
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ApplicationId { get; set; }

        [Display(Name = "誰報名的")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual UserAccount UserAccount { get; set; }

        [Display(Name = "哪個活動主題的")]
        public int ActivityId { get; set; }

        [ForeignKey("ActivityId")]
        public virtual Activity Activity { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "名字")]
        [StringLength(50)]
        public string Name { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "身分證字號")]
        //[RegularExpression(@"^[A-Z][12]\d{8}$", ErrorMessage = "身分證字號格式錯誤")]
        [StringLength(30)]
        public string CitizenId { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "電子郵件")]
        [StringLength(50)]
        [EmailAddress(ErrorMessage = "Email格式錯誤")]
        public string Email { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "手機號碼")]
        [StringLength(30)]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "手機號碼格式錯誤")]
        public string CellNumber { get; set; }

        [Display(Name = "是否親人隨行")]
        public bool IsAccompanied { get; set; }

        [Display(Name = "隨行人數", Prompt = "陪同人數")]
        [Range(0, 10, ErrorMessage = "最少 1人，最多 {2}人")]
        //[UIHint("Num")]
        public int PeopleNum { get; set; }

        [Required(ErrorMessage = "{0}必選")]
        [Display(Name = "交通方式")]
        public TransportType Transportation { get; set; }

        [Display(Name = "是否有素食需求")]
        public bool IsVegetarian { get; set; }

        [Display(Name = "填寫日期")]
        public DateTime? DateCompleted { get; set; }
    }
}