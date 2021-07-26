using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using static EnterprisePortal.Utils.Enum;

namespace EnterprisePortal.Models
{
    public class ToDoList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ToDoListId { get; set; }

        [Display(Name = "類型")]
        public ListType ListType { get; set; }

        [Display(Name = "狀態")]
        public ToDoListStatus ListStatus { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "標題")]
        [StringLength(100)]
        public string Title { get; set; }

        [Display(Name = "內容")]
        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string Summary { get; set; }

        [Display(Name = "截止日")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? EndTime { get; set; }

        [Display(Name = "發佈時間")]
        public DateTime PublishedDate { get; set; }

        [Display(Name = "屬於哪個使用者的代辦事項")]
        public int UserId { get; set; }
    }
}