using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using GeneticTspSolver.Utils;
using Palmmedia.ReportGenerator.Core;

namespace GeneticTspSolver
{
    public static class Mutation<T>
    {
        public static double MutationFactor = 1;

        public static double _Increase = 1;
        public static double _CurrentMutationFactor = 1;
        private static double _LastMutationFactor = 1;

        public static void Update(Population<T> population)
        {
            _Increase = population.OldBest.Fitness.Value / population.Best.Fitness.Value;
            var current_mutation_factor = Math.Min(1, Math.Max(MutationFactor, _Increase));
            _CurrentMutationFactor = current_mutation_factor;

            UnityEngine.Debug.LogWarning(
                "\t(OLD BEST) " + population.OldBest.Fitness.Value +
                "\t(NEW BEST) " + population.Best.Fitness.Value +
                "\t(INCREASE) " + _Increase +
                "\t(MUT FAC) " + _CurrentMutationFactor
            );
            return;

            var last_mutation_factor = _LastMutationFactor;

            _LastMutationFactor = _CurrentMutationFactor;
            _CurrentMutationFactor = (current_mutation_factor + last_mutation_factor) / 2;
        }

        public static void Mutate(Population<T> population)
        {
            Parallel.ForEach(
                // todo
                population.Chromosomes.Where(c => !population.EliteIds.Contains(c.Id)),
                Mutate
            );
        }

        public static void Mutate(Chromosome<T> chromosome)
        {
            _Mutate_InnerMove(chromosome);
            _Mutate_OuterMove(chromosome);

            _Mutate_InnerRemove(chromosome);
            _Mutate_OuterRemove(chromosome);

            _Mutate_InnerSwap(chromosome);
            _Mutate_OuterSwap(chromosome);
        }

        private static void _Mutate_InnerMove(Chromosome<T> chromosome)
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
        private static void _Mutate_OuterMove(Chromosome<T> chromosome)
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
        private static void _Mutate_InnerRemove(Chromosome<T> chromosome)
        {

        }
        // todo
        private static void _Mutate_OuterRemove(Chromosome<T> chromosome)
        {
            if (chromosome.GenesCount >= chromosome.Parent.Pool.Length)
                return;

        }

        private static void _Mutate_InnerSwap(Chromosome<T> chromosome)
        {
            var random = new FastRandoms();
            //Enumerable.Range(0, (int)(_CurrentMutationFactor * chromosome.GenesCount))
            Enumerable.Range(0, random.GetInt(0, 10))
                .Select(x => new { from = random.GetInt(0, chromosome.Values.Count), to = random.GetInt(0, chromosome.Values.Count) })
                .ToList()
                .ForEach(s => (chromosome.Genes[s.from].Value, chromosome.Genes[s.to].Value) = (chromosome.Genes[s.to].Value, chromosome.Genes[s.from].Value));
        }
        // todo
        private static void _Mutate_OuterSwap(Chromosome<T> chromosome)
        {
            if (chromosome.GenesCount >= chromosome.Parent.Pool.Length)
                return;

            var random = new FastRandoms();
            //random.GetInts((int)(_CurrentMutationFactor * chromosome.GenesCount), 0, chromosome.Parent.Pool.Length)
            random.GetInts(random.GetInt(0, 10), 0, chromosome.Parent.Pool.Length)
                .Select(x => chromosome.Parent.Pool[x])
                .Distinct()
                .Where(v => !chromosome.Lookup.ContainsKey(v))
                .Select(v => new { v, i = new Random().Next(0, chromosome.Values.Count) })
                .ToList()
                .ForEach(s => chromosome.Genes[s.i].Value = s.v);
        }

        public static void Initialize(double mutation_factor)
        {
            MutationFactor = Math.Min(1, Math.Max(0, mutation_factor));
        }
    }
}
