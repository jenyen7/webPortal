using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterprisePortal.Models
{
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageId { get; set; }

        [Display(Name = "訊息列表ID")]
        public int MsgListId { get; set; }

        [ForeignKey("MsgListId")]
        public virtual MessageList MessageList { get; set; }

        [Display(Name = "發送訊息者")]
        public int SenderId { get; set; }

        [Display(Name = "訊息")]
        [StringLength(500)]
        public string Text { get; set; }

        [Display(Name = "發送時間")]
        public DateTime Time { get; set; }
    }

    public class MessageViewModel
    {
        public IEnumerable<Message> Messages { get; set; }

        public int CurrentUser { get; set; }

        public int MsgListId { get; set; }

        public string Avatar { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public int TitleId { get; set; }

        public int SectionId { get; set; }
    }
}