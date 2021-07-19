using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterprisePortal.Models
{
    public class MessageList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageListId { get; set; }

        public bool IsVisible { get; set; }

        [Display(Name = "活動ID")]
        public int? ActivityId { get; set; }

        [ForeignKey("ActivityId")]
        public virtual Activity Activity { get; set; }

        [Display(Name = "投票ID")]
        public int? VotingId { get; set; }

        [ForeignKey("VotingId")]
        public virtual Voting Voting { get; set; }

        [Display(Name = "提問者")]
        public int QuestionerId { get; set; }

        [ForeignKey("QuestionerId")]
        public virtual UserAccount QuestionerUser { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
    }
}