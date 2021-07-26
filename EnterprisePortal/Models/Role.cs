using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnterprisePortal.Models
{
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "角色名稱")]
        [Remote("IsRoleAvailable", "Roles", AdditionalFields = "InitialRole", HttpMethod = "POST", ErrorMessage = "{0}已使用過，請更改名稱")]
        [StringLength(100)]
        public string RoleTitle { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "角色介紹")]
        [StringLength(200)]
        public string RoleSummary { get; set; }

        [Display(Name = "擁有的權限代號")]
        [StringLength(100)]
        public string RoleValue { get; set; }
    }
}