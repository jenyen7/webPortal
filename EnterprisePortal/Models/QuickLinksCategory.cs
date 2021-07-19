using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EnterprisePortal.Models
{
    public class QuickLinksCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LinkCatId { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "分類名稱")]
        [StringLength(50)]
        public string LinkCatName { get; set; }

        public virtual ICollection<QuickLink> QuickLinks { get; set; }
    }
}