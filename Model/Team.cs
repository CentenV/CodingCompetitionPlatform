using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodingCompetitionPlatform.Model
{
    public class Team
    {
        [Key]
        [Column("teamid")]
        public string teamId { get; set; }
        [Column("teampoints")]
        public int teamPoints { get; set; }
        [Column("passphrase")]
        public string passphrase { get; set; }
        [Column("school")]
        public string school { get; set; }
    }
}
