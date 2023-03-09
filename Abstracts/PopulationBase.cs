using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

using ParallelGenetics;
using ParallelGenetics.Enums;
using ParallelGenetics.Performers;
using ParallelGenetics.Structs;
using ParallelGenetics.Utils;
using Logger = ParallelGenetics.Utils.Logger;
using System.Collections.Concurrent;

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

        public ComputeBuffer CGRandomStates;
        public ComputeBuffer CGPartitions;
        public ComputeBuffer CGChromosomes;
        public ComputeBuffer CGGenes;
        public ComputeBuffer CGMutationCounts;
        public ComputeBuffer CGMutationExists;
        public ComputeBuffer CGMutationPool;

        public Stopwatch Stopwatch { get; set; } = Stopwatch.StartNew();

        public PopulationBase(
            Genetics genetics,
            int id,
            int partitionsCount,
            int chromosomesCount,
            int genesCount,
            object[] allValues
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
            AllValues = allValues;

            if (Genetics.Compute != null)
            {
                CGRandomStates = new ComputeBuffer(PartitionsCount * ChromosomesCount * GenesCount, sizeof(uint));

                CGPartitions = new ComputeBuffer(PartitionsCount, Marshal.SizeOf(typeof(PartitionData)));
                CGChromosomes = new ComputeBuffer(ChromosomesCount * PartitionsCount, Marshal.SizeOf(typeof(ChromosomeData)));
                CGGenes = new ComputeBuffer(GenesCount * ChromosomesCount * PartitionsCount, Marshal.SizeOf(typeof(GeneData)));
            }
        }

        public void SetCG()
        {
            if (Genetics.Compute != null)
            {
                if (Generation <= 1 || Genetics.Populations.Count > 1)
                {
                    CGRandomStates.SetData(new FastRandoms().GetInts(PartitionsCount * ChromosomesCount * GenesCount, 0, Int32.MaxValue));

                    Genetics.Compute.SetInt("PartitionsCount", PartitionsCount);
                    Genetics.Compute.SetInt("ChromosomesCount", ChromosomesCount);
                    Genetics.Compute.SetInt("GenesCount", GenesCount);
                    Genetics.Compute.SetInt("ValuesCount", AllValues.Length);
                }

                CGPartitions.SetData(AllPartitions);
                CGChromosomes.SetData(AllChromosomes);
                CGGenes.SetData(AllGenes);
            }
        }

        public void GetCG()
        {
            CGPartitions.GetData(AllPartitions);
            CGChromosomes.GetData(AllChromosomes);
            CGGenes.GetData(AllGenes);
        }

        public override string ToString()
        {
            return string.Join("\t ", new string[]
                {
                    "(GEN) " + Generation.ToString(),
                    "(Partitions) " + Partitions.Length,
                    "(Best) " + string.Join(" | ", Partitions.Select(p => p.GetBest().Fitness.ToString())),
                    "(Mutations) " + string.Join(" | ", Partitions.Select(p => p.Mutator.ToString()))
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
                default:
                    _LinearNext();
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
                partition.Picker.EliteFactor = eliteFactor;
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