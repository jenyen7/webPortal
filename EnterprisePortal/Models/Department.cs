using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EnterprisePortal.Models
{
    public class Department
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "部門名稱")]
        [StringLength(50)]
        public string DepartmentName { get; set; }

        [Required(ErrorMessage = "{0}必選")]
        [Display(Name = "部門標籤底色")]
        [StringLength(30)]
        public string DepartmentColor { get; set; }

        //[JsonIgnore]
        public virtual ICollection<UserAccount> UserAccounts { get; set; }

        public virtual ICollection<Announcement> Announcements { get; set; }
    }

    public class DepartmentViewModel
    {
        public string DepName { get; set; }

        public int DepValue { get; set; }

        public bool IsChecked { get; set; }
    }

    public class DepartmentDataModel
    {
        public string[] Departments { get; set; }

        public string[] UserAccounts { get; set; }

        public string[] UserIds { get; set; }
    }
}