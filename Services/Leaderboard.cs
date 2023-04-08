using CodingCompetitionPlatform.Model;

namespace CodingCompetitionPlatform.Services
{
    public class Leaderboard
    {
        public static List<TeamModel> GetTop(int numberOfTopValues, DatabaseContext _dbContext)
        {
            List<TeamModel> top = (from tp in _dbContext.Teams orderby tp.teampoints descending select tp).Take(numberOfTopValues).ToList();
            return top;
        }
    }
}