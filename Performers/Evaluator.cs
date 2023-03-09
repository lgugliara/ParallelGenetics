using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ParallelGenetics.Abstracts;
using ParallelGenetics.Enums;
using ParallelGenetics.Structs;

namespace ParallelGenetics.Performers
{
    public class Evaluator
    {
        public Func<ChromosomeBase, double> Controller = delegate { return 0; };
        public double Comparer = 1;

        public void Evaluate(PartitionBase partition)
        {
            switch (partition.Population.Genetics.ExecutionEnvironment)
            {
                case ExecutionEnvironment.Linear:
                    _LinearEvaluate(partition);
                    break;
                case ExecutionEnvironment.Parallel:
                    _ParallelEvaluate(partition);
                    break;
                default:
                    _ParallelEvaluate(partition);
                    break;
            }
        }

        private void _LinearEvaluate(PartitionBase partition)
        {
            foreach (var c in partition.Chromosomes)
                c.Fitness = Controller(c);
        }

        private void _ParallelEvaluate(PartitionBase partition)
        {
            Parallel.ForEach(
                partition.Chromosomes,
                c => c.Fitness = Controller(c)
            );
        }
    }
}