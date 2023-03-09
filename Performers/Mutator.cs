using System;
using System.Collections.Concurrent;
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
        public int MutationsCount = 1;

        public void Update(PartitionBase partition) =>
            MutationsCount = (
                Math.Clamp(
                    (int)(partition.Population.GenesCount * partition.Increase),
                    1,
                    partition.Population.GenesCount
                ) +
            MutationsCount) / 2;

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
                case ExecutionEnvironment.CGParallel:
                    _CGParallelMutate(partition);
                    break;
                default:
                    _ParallelMutate(partition);
                    break;
            }
        }

        private void _LinearMutate(PartitionBase partition)
        {
            foreach (var c in partition.Chromosomes.Where(c => !c.IsEliteOf(partition.Population.Generation)))
            {
                _InnerSwap(c);
                //_InnerMove(c);
                _OuterSwap(c);
                //_OuterMove(c);
            }
        }

        private void _ParallelMutate(PartitionBase partition)
        {
            //_InnerMove(partition);

            Parallel.ForEach(
                partition.Chromosomes.Where(c => !c.IsEliteOf(partition.Population.Generation)),
                c =>
                {
                    _InnerSwap(c);
                    //_InnerMove(c);
                    _OuterSwap(c);
                    //_OuterMove(c);
                }
            );
        }

        private void _CGParallelMutate(PartitionBase partition)
        {
            var Compute = partition.Population.Genetics.Compute;

            // Inner swaps
            int kernelID = Compute.FindKernel("Mutate_InnerSwap");
            Compute.SetBuffer(kernelID, "RandomStates", partition.Population.CGRandomStates);
            Compute.SetBuffer(kernelID, "MutationExists", partition.Population.CGMutationExists);
            Compute.SetBuffer(kernelID, "MutationPool", partition.Population.CGMutationPool);
            Compute.SetBuffer(kernelID, "Partitions", partition.Population.CGPartitions);
            Compute.SetBuffer(kernelID, "Chromosomes", partition.Population.CGChromosomes);
            Compute.SetBuffer(kernelID, "Genes", partition.Population.CGGenes);
            Compute.SetBuffer(kernelID, "MutationCounts", partition.Population.CGMutationCounts);
            Compute.Dispatch(
                kernelID,
                (int)Math.Ceiling((double)partition.Population.AllGenes.Length / 1024),
                1, 1
            );
        }

        private void _InnerSwap(ChromosomeBase chromosome)
        {
            var random = new FastRandoms();
            var swaps = new int[random.GetInt(0, MutationsCount + 1)]
                .AsParallel()
                .Select(x => new {
                    from = random.GetInt(0, chromosome.Partition.Population.GenesCount),
                    to = random.GetInt(0, chromosome.Partition.Population.GenesCount)
                })
                .ToList();

            foreach (var s in swaps)
                (chromosome.Genes[s.from].ValueId, chromosome.Genes[s.to].ValueId) = (chromosome.Genes[s.to].ValueId, chromosome.Genes[s.from].ValueId);
        }

        private void _InnerMove(PartitionBase partition)
        {
            partition.Population.SetCG();
            var Compute = partition.Population.Genetics.Compute;

            int kernelID = Compute.FindKernel("Mutate_InnerMove");
            Compute.SetBuffer(kernelID, "RandomStates", partition.Population.CGRandomStates);
            Compute.SetBuffer(kernelID, "Partitions", partition.Population.CGPartitions);
            Compute.SetBuffer(kernelID, "Chromosomes", partition.Population.CGChromosomes);
            Compute.SetBuffer(kernelID, "Genes", partition.Population.CGGenes);
            Compute.Dispatch(
                kernelID,
                (int)Math.Ceiling((double)partition.Population.AllGenes.Length / 1024),
                1, 1
            );

            partition.Population.GetCG();
        }

        private void _OuterSwap(ChromosomeBase chromosome)
        {
            if (chromosome.Partition.Population.GenesCount >= chromosome.Partition.Population.AllValues.Length)
                return;

            var random = new FastRandoms();
            var swaps = random
                .GetInts(random.GetInt(0, MutationsCount + 1), 0, chromosome.Partition.Population.AllValues.Length)
                .Distinct()
                .AsParallel()
                .Where(i => !chromosome.Genes.AsParallel().Select(g => g.ValueId).Contains(i))
                .Select(v => new {
                    v,
                    i = random.GetInt(0, chromosome.Partition.Population.GenesCount)
                })
                .ToList();

            foreach (var s in swaps)
                chromosome.Genes[s.i].ValueId = s.v;
        }

        private void _OuterMove(ChromosomeBase chromosome)
        {
            // todo
        }

        public override string ToString()
        {
            return MutationsCount.ToString();
        }
    }
}