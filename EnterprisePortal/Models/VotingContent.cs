using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EnterprisePortal.Models
{
    public class VotingContent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VotingContentId { get; set; }

        [Display(Name = "是否可多票")]
        public bool IsMultipleChoices { get; set; }

        [Display(Name = "投票第幾題")]
        public int VoteNum { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "題目")]
        [StringLength(500)]
        public string VoteText { get; set; }

        [Display(Name = "哪個投票活動底下的問題")]
        public int VotingId { get; set; }

        [ForeignKey("VotingId")]
        public virtual Voting Voting { get; set; }

        public virtual ICollection<VotingResult> VotingResults { get; set; }
    }
}