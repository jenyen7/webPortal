using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterprisePortal.Models
{
    public class RolePermission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PermissionId { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "權限名稱")]
        [StringLength(50)]
        public string PermissionName { get; set; }

        public int? Pid { get; set; }

        //遞迴
        [Display(Name = "父類別")]
        [ForeignKey("Pid")]
        public virtual RolePermission Permission { get; set; }

        [Display(Name = "子類別")]
        public virtual ICollection<RolePermission> PermissionList { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "權限代號")]
        [StringLength(50)]
        public string PValue { get; set; }

        [Display(Name = "圖樣")]
        [StringLength(50)]
        public string Icon { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "連結")]
        [StringLength(200)]
        //[DataType(DataType.Url)]
        public string Url { get; set; }
    }
}