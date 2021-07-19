using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using static EnterprisePortal.Utils.Enum;

namespace EnterprisePortal.Models
{
    public class VotingResult
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VotingResultId { get; set; }

        [Display(Name = "誰投的")]
        public int UserId { get; set; }

        [Display(Name = "投票選項")]
        public int ContentId { get; set; }

        [ForeignKey("ContentId")]
        public virtual VotingContent VotingContent { get; set; }
    }

    public class ResultsList : List<Results> { }

    public class Results
    {
        public Results()
        {
            Label = new List<string>();
            Data = new List<int>();
        }

        public string ChartType { get; set; }

        public string QuestionText { get; set; }

        public List<string> Label { get; set; }

        public List<int> Data { get; set; }
    }

    public class VotingResultViewModel
    {
        public Voting Voting { get; set; }

        public ResultsList Results { get; set; }
    }
}