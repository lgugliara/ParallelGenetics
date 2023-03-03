using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using GeneticTspSolver.CG;

namespace GeneticTspSolver
{
    public class Picker<T>
    {
        public static double EliteFactor = 1;

        public static double EliteTheshold => (1 - EliteFactor);

        public void Pick(Population<T> population)
        {
            _PickElites(population);
            _PickTournament(population);
        }

        private void _PickElites(Population<T> population)
        {
            var elites = population.Chromosomes
                //.Where(c => c.Fitness.Value > (EliteTheshold * population.Best.Fitness.Value))
                .OrderByDescending(c => c.Fitness.Value)
                .Take((int)Math.Max(1, EliteFactor * population.ChromosomesCount))
                .ToArray();
            population.EliteIds = elites.Select(c => c.Id).ToArray();

            // Parallel
            Parallel.ForEach(
                population.Chromosomes.Where(c => !elites.Contains(c)),
                (c, s, i) => Chromosome<T>.Copy(elites[(int)i % elites.Length], c)
            );

            // GPU
            var cg = population.Parent.CGUtils;
            //cg.Compute.Dispatch(cg.Picker_KernelID, CGUtils<T>.GetThreadGroups(population.Best.GenesCount), 1, 1);
        }

        private void _PickTournament(Population<T> population)
        {

        }

        public static void Initialize(double elite_factor)
        {
            EliteFactor = elite_factor;
        }
    }
}