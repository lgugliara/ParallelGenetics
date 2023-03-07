using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ParallelGenetics.Abstracts;
using ParallelGenetics.Performers;
using ParallelGenetics.Structs;

namespace ParallelGenetics
{
    public class Partition<T> : PartitionBase
    {
        public Partition(
            Population<T> population,
            int id
        )
        {
            Population = population;
            Id = id;

            _Current = new PartitionData(Id);

            Chromosomes = Enumerable
                .Range(0, Population.ChromosomesCount)
                .Select(i => new Chromosome<T>(this, i))
                .ToArray();
        }
    }
}