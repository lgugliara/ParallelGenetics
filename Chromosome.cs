using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ParallelGenetics.Abstracts;
using ParallelGenetics.Structs;

namespace ParallelGenetics
{
    public class Chromosome<T> : ChromosomeBase, IComparable
    {
        public Chromosome(
            Partition<T> partition,
            int id
        )
        {
            Partition = partition;
            Id = id;

            _Current = new ChromosomeData(Partition.Id, Id);

            Genes = Enumerable
                .Range(0, Partition.Population.GenesCount)
                .Select(i => new Gene<T>(this, i))
                .ToArray();
        }
    }
}