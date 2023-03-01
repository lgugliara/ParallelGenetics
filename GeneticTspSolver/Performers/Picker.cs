using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace GeneticTspSolver
{
    public static class Picker<T>
    {
        public static void Pick(Population<T> population)
        {
            Parallel.ForEach(
                population.Chromosomes,
                c => Chromosome<T>.Copy(population.Best, c)
            );
            UnityEngine.Debug.Log(population.Best.Fitness.ToString());
        }

        public static void Initialize()
        {

        }
    }
}