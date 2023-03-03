using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace GeneticTspSolver
{
    public static class Picker<T>
    {
        public static double EliteFactor = 1;

        public static double EliteTheshold => (1 - EliteFactor);

        public static void Pick(Population<T> population)
        {
            _PickElites(population);
            _PickTournament(population);
        }

        private static void _PickElites(Population<T> population)
        {
            var elites = population.Chromosomes
                //.Where(c => c.Fitness.Value > (EliteTheshold * population.Best.Fitness.Value))
                .OrderByDescending(c => c.Fitness.Value)
                .Take((int)Math.Max(1, EliteFactor * population.ChromosomesCount))
                .ToArray();
            population.EliteIds = elites.Select(c => c.Id).ToArray();

            Parallel.ForEach(
                population.Chromosomes.Where(c => !elites.Contains(c)),
                (c, s, i) => Chromosome<T>.Copy(elites[(int)i % elites.Length], c)
            );
        }

        private static void _PickTournament(Population<T> population)
        {

        }

        public static void Initialize(double elite_factor)
        {
            EliteFactor = elite_factor;
        }
    }
}