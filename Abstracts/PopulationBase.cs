using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using ParallelGenetics;
using ParallelGenetics.Enums;
using ParallelGenetics.Performers;
using ParallelGenetics.Structs;
using Logger = ParallelGenetics.Utils.Logger;

namespace ParallelGenetics.Abstracts
{
    public abstract class PopulationBase
    {
        public Genetics Genetics { get; set; }
        public int Id { get; set; }
        public int Generation { get; set; }

        public int PartitionsCount { get; set; }
        public int ChromosomesCount { get; set; }
        public int GenesCount { get; set; }

        public PartitionData[] AllPartitions { get; set; }
        public ChromosomeData[] AllChromosomes { get; set; }
        public GeneData[] AllGenes { get; set; }
        public object[] AllValues { get; set; }

        public PartitionBase[] Partitions { get; set; }

        public Stopwatch Stopwatch { get; set; } = Stopwatch.StartNew();

        public override string ToString()
        {
            return string.Join("\t\t", new string[]
                {
                    "(GEN) " + Generation.ToString(),
                    "(Partitions) " + Partitions.Length,
                    "(Best) " + Partitions.Select(p => p.GetBest()).Max().Fitness.ToString(),
                    string.Join(" | ", Partitions.Select(p => p.Mutator.ToString()))
                });
        }

        public void Next()
        {
            Generation++;

            switch (Genetics.ExecutionEnvironment)
            {
                case ExecutionEnvironment.Linear:
                    _LinearNext();
                    break;
                case ExecutionEnvironment.Parallel:
                    _ParallelNext();
                    break;
            }

            Logger.Log(this.ToString());
        }

        private void _LinearNext()
        {
            foreach (var partition in Partitions)
                partition.Next();
        }

        private void _ParallelNext()
        {
            Parallel.ForEach(
                Partitions,
                p => p.Next()
            );
        }

        public void SetPickers(double eliteFactor)
        {
            foreach (var partition in Partitions)
            {
                partition.Picker.Partition = partition;
                partition.Picker.EliteFactor = eliteFactor;
            }
        }

        public void SetMutators()
        {
            // nothing to set
        }

        public void SetCrossovers()
        {
            // todo
        }

        public void SetEvaluators(Func<ChromosomeBase, double> evaluate, double comparer = 1)
        {
            foreach (var partition in Partitions)
            {
                partition.Evaluator.Controller = evaluate;
                partition.Evaluator.Comparer = comparer;
            }
        }
    }
}