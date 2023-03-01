using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GeneticTspSolver
{
    public class GeneticAlgorithm<T>
    {
        public Population<T> Population { get; set; }

        public event EventHandler OnRan;
        public event EventHandler OnBestChange;
        public event EventHandler OnTerminate;

        public Stopwatch Stopwatch { get; set; } = Stopwatch.StartNew();

        public GeneticAlgorithm(
            int chromosomes_count,
            int genes_count,
            List<T> values,
            Func<Chromosome<T>, double> evaluate,
            double comparer,
            double mutation_factor,
            bool isUnique,
            EventHandler on_ran = null,
            EventHandler on_best_change = null,
            EventHandler on_terminate = null
        ) {
            // Benchmark
            Stopwatch.Restart();

            genes_count = Math.Min(genes_count, values.Count);

            Crossover<T>.Initialize();
            Mutation<T>.Initialize(Math.Max(1, (int)(Math.Min(1, mutation_factor) * genes_count)));
            Fitness<T>.Initialize(evaluate, comparer);
            Picker<T>.Initialize();

            var adam_values = values.Take(genes_count);

            if (isUnique)
            {
                System.Random rnd = new System.Random();
                var adam_pool = values.ToArray().ToList();
                adam_values = adam_values
                    .Select(x => {
                        var index = adam_pool[rnd.Next(0, adam_pool.Count)];
                        adam_pool.Remove(index);
                        return index;
                    });
            }

            Population = new Population<T>(this, 0, chromosomes_count, genes_count, values.ToArray(), adam_values.ToArray());

            OnRan = on_ran;
            OnBestChange = on_best_change;
            OnTerminate = on_terminate;

            UnityEngine.Debug.LogError("First population created in " + Stopwatch.Elapsed);
        }

        public async Task Start() => Run();

        public void Run()
        {
            for(int generation = 0; !ITermination<T>.IsTerminated(this); generation++)
            {
                if(generation % 1 == 0)
                    UnityEngine.Debug.LogWarning("(GEN) " + generation + "\t\t(BEST FITNESS) " + Population.Best.Fitness.Value);

                // TODO
                //Population.PerformCrossover();
                Population.PerformMutate();

                if(Population.PerformEvaluate(OnBestChange))
                    Population.PerformPick();

                OnRan?.Invoke(this, EventArgs.Empty);
            }

            OnTerminate?.Invoke(this, EventArgs.Empty);
        }
    }
}
