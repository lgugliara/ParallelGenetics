using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ParallelGenetics.Abstracts;
using ParallelGenetics.Enums;
using ParallelGenetics.Structs;

namespace ParallelGenetics.Performers
{
    public class Picker
    {
        public PartitionBase Partition;

        public double EliteFactor = 1;
        private int _ElitesCount => (int)Math.Max(1, EliteFactor * Partition.Population.ChromosomesCount);

        public void Pick(PartitionBase partition)
        {
            var elites = partition.Chromosomes
                .OrderByDescending(c => c.Fitness)
                .Take(_ElitesCount)
                .ToList();

            switch (partition.Population.Genetics.ExecutionEnvironment)
            {
                case ExecutionEnvironment.Linear:
                    _LinearPick(partition, elites);
                    break;
                case ExecutionEnvironment.Parallel:
                    _ParallelPick(partition, elites);
                    break;
            }
        }

        private static void _LinearPick(PartitionBase partition, List<ChromosomeBase> elites)
        {
            foreach (var c in elites)
                c.LastElite = partition.Population.Generation;

            var others = partition.Chromosomes
                .Where(c => !c.IsEliteOf(partition.Population.Generation))
                .ToList();

            for (var i = 0; i < others.Count; i++)
                ChromosomeBase.Copy(elites[i % elites.Count], others[i]);
        }

        private static void _ParallelPick(PartitionBase partition, List<ChromosomeBase> elites)
        {
            Parallel.ForEach(
                elites,
                c => c.LastElite = partition.Population.Generation
            );

            var others = partition.Chromosomes
                .AsParallel()
                .Where(c => !c.IsEliteOf(partition.Population.Generation))
                .ToList();

            Parallel.ForEach(
                others,
                (c, s, i) => ChromosomeBase.Copy(elites[(int)i % elites.Count], c)
            );
        }
    }
}