using System;

using ParallelGenetics;
using ParallelGenetics.Enums;
using ParallelGenetics.Performers;
using ParallelGenetics.Structs;

namespace ParallelGenetics.Abstracts
{
    public abstract class GeneBase
    {
        public ChromosomeBase Chromosome { get; set; }
        public int Id { get; set; }

        internal ref GeneData _Current => ref Chromosome.Partition.Population.AllGenes[Chromosome.Partition.Id * Chromosome.Partition.Population.ChromosomesCount * Chromosome.Partition.Population.GenesCount + Chromosome.Id * Chromosome.Partition.Population.GenesCount + Id];
        public ref int ValueId => ref _Current.ValueId;

        public object GetValue()
        {
            if (ValueId < 0)
                throw new NullReferenceException("Value ID has not been set.");
            return Chromosome.Partition.Population.AllValues[ValueId];
        }
        
        public bool Equals(GeneBase other) => ValueId.Equals(other.ValueId);
    }
}