using System.Collections.Generic;

namespace GeneticTspSolver
{
    public static class Crossover<T>
    {
        public static IEnumerable<Chromosome<T>> Cross(Chromosome<T> chromosome_1, Chromosome<T> chromosome_2)
        {
            return new[] { chromosome_1, chromosome_2 };
        }

        public static void Initialize()
        {

        }
    }
}
