using System.Collections.Generic;

namespace GeneticTspSolver
{
    public class Crossover<T>
    {
        public static double EliteFactor = 0.1;

        public IEnumerable<Chromosome<T>> Cross(Chromosome<T> chromosome_1, Chromosome<T> chromosome_2)
        {
            return new[] { chromosome_1, chromosome_2 };
        }

        public static void Initialize(double elite_factor)
        {
            EliteFactor = elite_factor;
        }
    }
}
