using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ParallelGenetics.Abstracts;
using ParallelGenetics.Structs;

namespace ParallelGenetics
{
    public class Gene<T> : GeneBase, IEquatable<Gene<T>>
    {
        public Gene(
            Chromosome<T> chromosome,
            int id
        )
        {
            Chromosome = chromosome;
            Id = id;

            _Current = new GeneData(Chromosome.Partition.Id, Chromosome.Id, Id);
        }

        public bool Equals(Gene<T> other)
        {
            return base.Equals(other);
        }
    }
}