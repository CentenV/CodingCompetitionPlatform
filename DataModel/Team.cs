using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodingCompetitionPlatform.Model
{
    [Table("teams")]
    public class Team
    {
        [Key]
        [Column("teamid")]
        public string teamid { get; set; }
        [Column("teampoints")]
        public int teampoints { get; set; }
        [Column("passphrase")]
        public string passphrase { get; set; }
        [Column("school")]
        public string school { get; set; }
    }
}
