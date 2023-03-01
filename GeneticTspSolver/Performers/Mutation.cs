using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using GeneticTspSolver.Utils;

namespace GeneticTspSolver
{
    public static class Mutation<T>
    {
        public static double MutationFactor = 1;

        public static void Mutate(Chromosome<T> chromosome)
        {
            var random = new FastRandoms();

            var out_swaps = random.GetInts((int)(MutationFactor * chromosome.GenesCount), 0, chromosome.Parent.Pool.Length)
                .Select(x => chromosome.Parent.Pool[x])
                .Distinct()
                .Where(v => !chromosome.Lookup.ContainsKey(v))
                .Select(v => new { v, i = new Random().Next(0, chromosome.Values.Count) })
                .ToList();

            var in_swaps = Enumerable.Range(0, (int)(MutationFactor * chromosome.GenesCount))
                .AsParallel()
                .Select(x => new { from = random.GetInt(0, chromosome.Values.Count), to = random.GetInt(0, chromosome.Values.Count) })
                .ToList();

            //UnityEngine.Debug.LogError("(OUTER SWAPS) " + out_swaps.Count() + " | " + out_swaps.Count() + "\t\t(INNER SWAPS) " + in_swaps.Count());

            //out_swaps.ForEach(s => chromosome.Genes[s.i].Value = s.v);
            in_swaps.ForEach(s => (chromosome.Genes[s.from].Value, chromosome.Genes[s.to].Value) = (chromosome.Genes[s.to].Value, chromosome.Genes[s.from].Value));
        }

        public static void Initialize(double mutation_factor)
        {
            MutationFactor = Math.Max(1, mutation_factor);
        }
    }
}
