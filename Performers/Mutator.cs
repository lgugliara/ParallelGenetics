using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ParallelGenetics;
using ParallelGenetics.Abstracts;
using ParallelGenetics.Enums;
using ParallelGenetics.Structs;
using ParallelGenetics.Utils;

namespace ParallelGenetics.Performers
{
    public class Mutator
    {
        private int _MutationsCount = 1;

        public void Update(PartitionBase partition) =>
            _MutationsCount = (Math.Clamp((int)(partition.Population.GenesCount * partition.Increase), 1, partition.Population.GenesCount) + _MutationsCount) / 2;

        public void Mutate(PartitionBase partition)
        {
            switch (partition.Population.Genetics.ExecutionEnvironment)
            {
                case ExecutionEnvironment.Linear:
                    _LinearMutate(partition);
                    break;
                case ExecutionEnvironment.Parallel:
                    _ParallelMutate(partition);
                    break;
            }
        }

        private void _LinearMutate(PartitionBase partition)
        {
            foreach (var c in partition.Chromosomes.Where(c => !c.IsEliteOf(partition.Population.Generation)))
                Mutate(c);
        }

        private void _ParallelMutate(PartitionBase partition)
        {
            Parallel.ForEach(
                partition.Chromosomes.Where(c => !c.IsEliteOf(partition.Population.Generation)),
                Mutate
            );
        }

        public void Mutate(ChromosomeBase chromosome)
        {
            _InnerMove(chromosome);
            _InnerSwap(chromosome);
            _OuterSwap(chromosome);
        }

        private void _InnerMove(ChromosomeBase chromosome)
        {
            // todo: GPU Only

            /* var random = new FastRandoms();

            var old_values = chromosome.Values.ToList();

            var from_index = random.GetInt(0, chromosome.Values.Count);
            var to_index = random.GetInt(0, chromosome.Values.Count);

            old_values.RemoveAt(from_index);
            old_values.Insert(to_index, chromosome.Values[from_index]);

            chromosome.Values = old_values.ToArray(); */
        }

        private void _InnerSwap(ChromosomeBase chromosome)
        {
            var random = new FastRandoms();
            var swaps = Enumerable
                .Range(0, random.GetInt(0, _MutationsCount))
                .Select(x => new {
                    from = random.GetInt(0, chromosome.Partition.Population.GenesCount),
                    to = random.GetInt(0, chromosome.Partition.Population.GenesCount)
                });

            foreach (var s in swaps)
                (chromosome.Genes[s.from].ValueId, chromosome.Genes[s.to].ValueId) = (chromosome.Genes[s.to].ValueId, chromosome.Genes[s.from].ValueId);
        }
        
        private void _OuterSwap(ChromosomeBase chromosome)
        {
            if (chromosome.Partition.Population.GenesCount >= chromosome.Partition.Population.AllValues.Length)
                return;

            var random = new FastRandoms();
            var swaps = random
                .GetInts(random.GetInt(0, _MutationsCount), 0, chromosome.Partition.Population.AllValues.Length)
                .Distinct()
                .Where(i => !chromosome.Genes.Select(g => g.ValueId).Contains(i))
                .Select(v => new {
                    v,
                    i = random.GetInt(0, chromosome.Partition.Population.GenesCount)
                });

            foreach (var s in swaps)
                chromosome.Genes[s.i].ValueId = s.v;
        }

        public override string ToString()
        {
            return "(CURRENT MUTATIONS) " + _MutationsCount.ToString();
        }
    }
}