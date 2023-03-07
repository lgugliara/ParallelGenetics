using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ParallelGenetics.Abstracts;
using ParallelGenetics.Performers;
using ParallelGenetics.Structs;
using Logger = ParallelGenetics.Utils.Logger;

namespace ParallelGenetics
{
    public class Population<T> : PopulationBase
    {
        public Population(
            Genetics genetics,
            int id,
            int partitionsCount,
            int chromosomesCount,
            int genesCount,
            T[] allValues
        )
        {
            Genetics = genetics;
            Id = id;
            Generation = 0;

            PartitionsCount = partitionsCount;
            ChromosomesCount = chromosomesCount;
            GenesCount = genesCount;

            AllPartitions = new PartitionData[PartitionsCount];
            AllChromosomes = new ChromosomeData[PartitionsCount * ChromosomesCount];
            AllGenes = new GeneData[PartitionsCount * ChromosomesCount * GenesCount];
            AllValues = allValues.Select(x => x as object).ToArray();

            Partitions = Enumerable
                .Range(0, PartitionsCount)
                .Select(i => new Partition<T>(this, i))
                .ToArray();
        }

        public void LoadAdam(T[] values)
        {
            Stopwatch.Restart();

            if (values.Length != GenesCount)
                throw new ArgumentException("Values must be same amount as Genes count.");

            Parallel.ForEach(
                Partitions,
                p =>
                {
                    Parallel.For(
                        0,
                        values.Length,
                        i =>
                        {
                            int index = Array.IndexOf(AllValues, values[i] as object);

                            if (index < 0)
                                throw new NullReferenceException("Value " + values[i].ToString() + " is invalid.");
                            p.Chromosomes[0].Genes[i].ValueId = index;
                        }
                    );
                    Parallel.ForEach(
                        p.Chromosomes,
                        c => ChromosomeBase.Copy(p.Chromosomes[0], c)
                    );
                }
            );

            Logger.Log("Adam(s) loaded. Elapsed: " + Stopwatch.Elapsed);
        }
    }
}