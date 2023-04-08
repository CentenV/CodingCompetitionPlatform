using CodingCompetitionPlatform.Model;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodingCompetitionPlatform.Model
{
    [Table("problemstatus")]
    public class ProblemStatusModel
    {
        [Key]
        [Column("id")]
        public int id { get; set; }
        [Column("teamid")]
        public string teamid { get; set; }
        [Column("problemid")]
        public int problemid { get; set; }
        [Column("problemcompleted")]
        public bool problemcompleted { get; set; }

        [ForeignKey("teamid")]
        public TeamModel team { get; set; }
    }
}
