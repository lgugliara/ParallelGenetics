using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using GeneticTspSolver.Utils;
using Palmmedia.ReportGenerator.Core;

namespace GeneticTspSolver
{
    public class Mutation<T>
    {
        public static int CurrentMutationsCount { get => _CurrentMutationsCount; }

        public static int _CurrentMutationsCount = 1;
        private static int _LastMutationsCount = 1;

        public static void Update(Population<T> population)
        {
            var increase = 1 - population.OldBest.Fitness.Value / population.Best.Fitness.Value;
            var current_mutations_count = Math.Max(1, ((int)(population.Best.GenesCount * Math.Min(1, increase)) + _LastMutationsCount) / 2);

            _LastMutationsCount = _CurrentMutationsCount;
            _CurrentMutationsCount = current_mutations_count;
        }

        public void Mutate(Population<T> population)
        {
            var mutate_chromosomes = population.Chromosomes.Where(c => !c.IsEliteOf(population.Parent.Generations.LastOrDefault()));
            Parallel.ForEach(mutate_chromosomes, Mutate);
        }

        public void Mutate(Chromosome<T> chromosome)
        {
            _Mutate_InnerMove(chromosome);
            _Mutate_OuterMove(chromosome);

            _Mutate_InnerRemove(chromosome);
            _Mutate_OuterRemove(chromosome);

            _Mutate_InnerSwap(chromosome);
            _Mutate_OuterSwap(chromosome);
        }

        private void _Mutate_InnerMove(Chromosome<T> chromosome)
        {
            var random = new FastRandoms();
            var old_values = chromosome.Values.ToList();

            var from_index = random.GetInt(0, chromosome.Values.Count);
            var to_index = random.GetInt(0, chromosome.Values.Count);

            old_values.RemoveAt(from_index);
            old_values.Insert(to_index, chromosome.Values[from_index]);

            chromosome.Values = old_values.ToArray();
        }
        // todo
        private void _Mutate_OuterMove(Chromosome<T> chromosome)
        {
            if (chromosome.GenesCount >= chromosome.Parent.Pool.Length)
                return;

            //var random = new FastRandoms();
            //var old_values = chromosome.Values.ToList();

            //var from_index = random.GetInt(0, chromosome.Values.Count);
            //var to_index = random.GetInt(0, chromosome.Values.Count);

            //old_values.RemoveAt(from_index);
            //old_values.Insert(to_index, chromosome.Values[from_index]);

            //chromosome.Values = old_values.ToArray();
        }

        // todo
        private void _Mutate_InnerRemove(Chromosome<T> chromosome)
        {

        }
        // todo
        private void _Mutate_OuterRemove(Chromosome<T> chromosome)
        {
            if (chromosome.GenesCount >= chromosome.Parent.Pool.Length)
                return;

        }

        private void _Mutate_InnerSwap(Chromosome<T> chromosome)
        {
            var random = new FastRandoms();
            Enumerable.Range(0, random.GetInt(0, _CurrentMutationsCount))
                .Select(x => new { from = random.GetInt(0, chromosome.Values.Count), to = random.GetInt(0, chromosome.Values.Count) })
                .ToList()
                .ForEach(s => (chromosome.Genes[s.from].Value, chromosome.Genes[s.to].Value) = (chromosome.Genes[s.to].Value, chromosome.Genes[s.from].Value));
        }
        // todo
        private void _Mutate_OuterSwap(Chromosome<T> chromosome)
        {
            if (chromosome.GenesCount >= chromosome.Parent.Pool.Length)
                return;

            var random = new FastRandoms();
            random.GetInts(random.GetInt(0, _CurrentMutationsCount), 0, chromosome.Parent.Pool.Length)
                .Select(x => chromosome.Parent.Pool[x])
                .Distinct()
                .Where(v => !chromosome.Lookup.ContainsKey(v))
                .Select(v => new { v, i = new Random().Next(0, chromosome.Values.Count) })
                .ToList()
                .ForEach(s => chromosome.Genes[s.i].Value = s.v);
        }

        public static void Initialize()
        {

        }

        public override string ToString()
        {
            return " (CURRENT MUTATIONS) " + CurrentMutationsCount;
        }
    }
}
