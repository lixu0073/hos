using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContributorGenerator
{

    private struct ContributorInfo
    {
        string name;
        int level;
        int score;

        public ContributorInfo(string name, int level, int score)
        {
            this.name = name;
            this.level = level;
            this.score = score;
        }

        public string GetName()
        {
            return name;
        }

        public int GetLevel()
        {
            return level;
        }

        public int GetScore()
        {
            return score;
        }
    }

    private static List<ContributorInfo> contributorInfos = new List<ContributorInfo>()
    {
        new ContributorInfo("Rose Valley", 44, 17),
        new ContributorInfo("Harmony Grove", 12, 15),
        new ContributorInfo("Jenny's Place", 31, 14),
        new ContributorInfo("El Grande", 10, 12),
        new ContributorInfo("Orange Cats", 24, 9),
        new ContributorInfo("Doctor Why", 13, 7),
        new ContributorInfo("Caduceus USA", 15, 4),
        new ContributorInfo("LikeASurgeon", 17, 3),
        new ContributorInfo("Ghost Town", 15, 2),
        new ContributorInfo("Pizza Parlor", 27, 1)
    };

    public static List<Contributor> Fake(List<Contributor> contributors, int limit, int targetSum)
    {
        if (contributors.Count >= limit)
            return contributors;
        int contributorsScoreSum = 0;
        foreach (Contributor contributor in contributors)
            contributorsScoreSum += contributor.GetScore();
        if (contributorsScoreSum >= targetSum)
            return contributors;
        int numerOfContributorsToFake = limit - contributors.Count;
        for(int i = 0; i < numerOfContributorsToFake; ++i)
        {
            Contributor contributor = new Contributor(null, contributorInfos[i].GetScore());
            contributor.SetSave(new Hospital.PublicSaveModel()
            {
                SaveID = null,
                Level = contributorInfos[i].GetLevel(),
                Name = contributorInfos[i].GetName(),
                PlantationHelp = false,
                EpidemyHelp = false,
                TreatmentHelp = false,
                BestWonItem = null,
                FacebookID = null
            });
            contributors.Add(contributor);
        }
        return contributors.OrderByDescending(o => o.score).ToList();
    }

}
