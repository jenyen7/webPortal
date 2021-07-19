using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EnterprisePortal.Models
{
    public class QuickLink
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LinkId { get; set; }

        [Required(ErrorMessage = "{0}必選")]
        [Display(Name = "連結分類")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual QuickLinksCategory QuickLinksCategory { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "連結名稱")]
        [StringLength(100)]
        public string LinkName { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "連結網址")]
        [DataType(DataType.Url)]
        [StringLength(200)]
        public string LinkUrl { get; set; }
    }

    public class QuickLinksViewModel
    {
        public IEnumerable<QuickLinksCategory> QuickLinksCategories { get; set; }

        public IEnumerable<QuickLink> QuickLinks { get; set; }
    }
}