using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static EnterprisePortal.Utils.Enum;

namespace EnterprisePortal.Models
{
    public class VotingForm
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VotingFormId { get; set; }

        [Display(Name = "狀態")]
        public ToDoListStatus ListStatus { get; set; }

        [Display(Name = "誰投的")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual UserAccount UserAccount { get; set; }

        [Display(Name = "哪個投票主題的表單")]
        public int VotingId { get; set; }

        [ForeignKey("VotingId")]
        public virtual Voting Voting { get; set; }

        [Display(Name = "填寫日期")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? DateCompleted { get; set; }

        [Display(Name = "補充意見")]
        [DataType(DataType.MultilineText)]
        [StringLength(500)]
        public string Comments { get; set; }
    }
}