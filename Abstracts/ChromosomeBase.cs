using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ParallelGenetics;
using ParallelGenetics.Enums;
using ParallelGenetics.Performers;
using ParallelGenetics.Structs;

namespace ParallelGenetics.Abstracts
{
    public abstract class ChromosomeBase
    {
        public PartitionBase Partition { get; set; }
        public int Id { get; set; }

        internal ref ChromosomeData _Current => ref Partition.Population.AllChromosomes[Partition.Id * Partition.Population.ChromosomesCount + Id];
        public ref double Fitness => ref _Current.Fitness;
        public ref int LastElite => ref _Current.LastElite;

        public GeneBase[] Genes { get; set; }

        public bool IsEliteOf(int generation) => LastElite == generation;
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            ChromosomeBase other = obj as ChromosomeBase;
            if (other != null)
                return Fitness.CompareTo(other.Fitness);
            else
                throw new ArgumentException("Object is not a Chromosome");
        }

        public override string ToString()
        {
            return string.Join("; ", Genes.Select(g => g.ValueId.ToString()));
        }

        public static void Copy(ChromosomeBase from, ChromosomeBase to)
        {
            Parallel.ForEach(
                from.Genes,
                (g, s, i) => to.Genes[(int)i].ValueId = from.Genes[(int)i].ValueId
            );
        }

        public static void Copy(IEnumerable<ChromosomeBase> from, IEnumerable<ChromosomeBase> to, Crossover crossover)
        {
            Parallel.ForEach(
                from,
                f =>
                {

                }
            );
        }
    }
}