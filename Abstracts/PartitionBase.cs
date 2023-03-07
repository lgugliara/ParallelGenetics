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

        public Picker Picker = new Picker();
        public Mutator Mutator = new Mutator();
        public Crossover Crossover = new Crossover();
        public Evaluator Evaluator = new Evaluator();

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

            Increase = 0;
            if(new_best.Fitness > 0 && new_best.Fitness > old_best.Fitness)
                Increase = Math.Max(0, 1 - old_best.Fitness / new_best.Fitness);

            Mutator.Update(this);
        }
    }
}