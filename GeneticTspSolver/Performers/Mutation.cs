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

        public static void Mutate(Population<T> population)
        {
            Parallel.ForEach(
                population.Chromosomes.Skip(1),
                Mutate
            );
        }

        public static void Mutate(Chromosome<T> chromosome)
        {
            //_MutateInsert(chromosome);
            //_MutateRemove(chromosome);
            _MutateInner(chromosome);
            //_MutateOuter(chromosome);
        }

        private static void _MutateInsert(Chromosome<T> chromosome)
        {

        }

        private static void _MutateRemove(Chromosome<T> chromosome)
        {

        }

        private static void _MutateInner(Chromosome<T> chromosome)
        {
            var random = new FastRandoms();
            Enumerable.Range(0, (int)(MutationFactor * chromosome.GenesCount))
                .AsParallel()
                .Select(x => new { from = random.GetInt(0, chromosome.Values.Count), to = random.GetInt(0, chromosome.Values.Count) })
                .ToList()
                .ForEach(s => (chromosome.Genes[s.from].Value, chromosome.Genes[s.to].Value) = (chromosome.Genes[s.to].Value, chromosome.Genes[s.from].Value));
        }

        private static void _MutateOuter(Chromosome<T> chromosome)
        {
            var random = new FastRandoms();
            random.GetInts((int)(MutationFactor * chromosome.GenesCount), 0, chromosome.Parent.Pool.Length)
                .Select(x => chromosome.Parent.Pool[x])
                .Distinct()
                .Where(v => !chromosome.Lookup.ContainsKey(v))
                .Select(v => new { v, i = new Random().Next(0, chromosome.Values.Count) })
                .ToList()
                .ForEach(s => chromosome.Genes[s.i].Value = s.v);
        }

        public static void Initialize(double mutation_factor)
        {
            MutationFactor = Math.Max(1, mutation_factor);
        }
    }
}
