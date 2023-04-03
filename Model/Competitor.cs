using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodingCompetitionPlatform.Model
{
    public class Competitor
    {
        [Column("name")]
        public string name { get; set; }
        [Column("school")]
        public string school { get; set; }
        [Key]
        [Column("competitorid")]
        public string competitorID { get; set; }

        //[ForeignKey("teams")]
        public virtual Team team { get; set; }

        [Column("teamid")]
        public string teamID { get; set; }
    }
}
