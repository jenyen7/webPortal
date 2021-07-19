using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using static EnterprisePortal.Utils.Enum;

namespace EnterprisePortal.Models
{
    public class UserLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogId { get; set; }

        [Display(Name = "使用者")]
        public int TheUser { get; set; }

        [ForeignKey("TheUser")]
        public virtual UserAccount UserAccount { get; set; }

        [Display(Name = "動作")]
        public LogAction Action { get; set; }

        [Display(Name = "區域")]
        public LogArea Area { get; set; }

        [Display(Name = "項目")]
        [StringLength(100)]
        public string TheId { get; set; }

        [Display(Name = "時間")]
        public DateTime Time { get; set; }
    }
}