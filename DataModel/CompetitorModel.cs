using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodingCompetitionPlatform.Model
{
    [Table("competitors")]
    public class CompetitorModel
    {
        [Column("name")]
        public string name { get; set; }
        
        [Column("school")]
        public string school { get; set; }
        
        [Key]
        [Column("competitorid")]
        public string competitorID { get; set; }
        
        [Column("teamid")]
        public string? teamid { get; set; }

        [ForeignKey("teamid")]
        public Team team { get; set; }
    }
}
