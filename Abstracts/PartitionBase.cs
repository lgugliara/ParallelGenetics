using System;
using System.Linq;

using ParallelGenetics;
using ParallelGenetics.Enums;
using ParallelGenetics.Performers;
using ParallelGenetics.Structs;

namespace ParallelGenetics.Abstracts
{
    public abstract class PartitionBase
    {
        public PopulationBase Population { get; set; }
        public int Id { get; set; }

        internal ref PartitionData _Current => ref Population.AllPartitions[Id];
        public ref double Increase => ref _Current.Increase;

        public Picker Picker { get; set; } = new Picker();
        public Mutator Mutator { get; set; } = new Mutator();
        public Crossover Crossover { get; set; } = new Crossover();
        public Evaluator Evaluator { get; set; } = new Evaluator();

        public ChromosomeBase[] Chromosomes { get; set; }

        public ChromosomeBase GetBest() => Chromosomes.Max();

        public void Next()
        {
            var old_best = GetBest();

            Picker.Pick(this);
            Mutator.Mutate(this);
            Crossover.Cross(this);
            Evaluator.Evaluate(this);

            var new_best = GetBest();

            if (new_best.Fitness == 0)
                Increase = 0;
            else
                Increase = Math.Min(0, 1 - old_best.Fitness / new_best.Fitness);

            Mutator.Update(this);
        }
    }
}