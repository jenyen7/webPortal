using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EnterprisePortal.Models
{
    public class Announcement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AnnouncementId { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "標題")]
        [StringLength(100)]
        public string Title { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "公告內容")]
        public string AnnouncementContent { get; set; }

        [Display(Name = "瀏覽次數")]
        public int ViewNum { get; set; }

        [Display(Name = "公告全警局")]
        public bool IsPublic { get; set; }

        [Display(Name = "發佈時間")]
        public DateTime PublishedDate { get; set; }

        [Display(Name = "哪個部門發出的公告")]
        public int DepId { get; set; }

        [ForeignKey("DepId")]
        public virtual Department Department { get; set; }
    }

    public class AnnouncementViewModel
    {
        public string DepartmentColor { get; set; }

        public string DepartmentName { get; set; }

        public string AnnouncementTitle { get; set; }

        public string TimeAgo { get; set; }
    }
}