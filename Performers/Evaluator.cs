using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ParallelGenetics.Abstracts;
using ParallelGenetics.Structs;

namespace ParallelGenetics.Performers
{
    public class Evaluator
    {
        public Func<ChromosomeBase, double> Controller = delegate { return 0; };
        public double Comparer = 1;

        public void Evaluate(PartitionBase partition)
        {
            Parallel.ForEach(
                partition.Chromosomes,
                Evaluate
            );
        }

        public void Evaluate(ChromosomeBase chromosome)
        {
            chromosome.Fitness = Controller(chromosome);
        }
    }
}