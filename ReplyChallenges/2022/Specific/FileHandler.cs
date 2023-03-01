using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeneticTspSolver;

namespace ReplyChallenge2022
{
    public static class FileHandler
    {
        internal static void ImportInputData(string fileName)
        {
            var lines = File.ReadAllLines(fileName);
            var splitLine = lines[0].Split();

            GameParameter.InitialStamina = int.Parse(splitLine[0]);
            GameParameter.MaxStamina = int.Parse(splitLine[1]);
            GameParameter.Turns = int.Parse(splitLine[2]);

            var nDemons = int.Parse(splitLine[3]);
            for (int i = 0; i < nDemons; i++)
            {
                splitLine = lines[i + 1].Split();
                Demon demon = new Demon
                {
                    Id = i,
                    StaminaToDefeat = int.Parse(splitLine[0]),
                    TurnBeforeStamina = int.Parse(splitLine[1]),
                    StaminaRecovered = int.Parse(splitLine[2])
                };

                var nFragments = int.Parse(splitLine[3]);
                for (int frag = 0; frag < nFragments; frag++)
                {
                    demon.Fragments.Add(int.Parse(splitLine[4 + frag]));
                }

                GameParameter.Demons.Add(demon);
            }
        }
        /* internal static void ImportAdamData<T>(string fileName, Chromosome<T> adam)
        {
            var lines = File.ReadAllLines(fileName);
            var pool = GameParameter.Demons.Select(x => x.Id).ToList();
            List<IGene<T>> genes = new List<IGene<T>>();

            foreach (var line in lines)
            {
                var adamGene = new Gene<T>();
                adamGene.Init(int.Parse(line), pool: pool);

                genes.Add(adamGene);
            }
            for (int i = 0; i < 1000; i++)
            {
                var adamGene = new Gene<T>();
                adamGene.Init(pool.First(), pool: pool);

                genes.Add(adamGene);
            }

            adam.Init(genes, genes.Count);
        } */

        public static void DrawAdam<T>(string fileName, Chromosome<T> adam)
        {
            File.WriteAllText(
                fileName,
                string.Join("\n", adam.Genes.Select(x => x.Value))
            );
        }
    }
}
