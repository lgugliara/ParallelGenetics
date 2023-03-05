using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using GeneticTspSolver.CG;
using GeneticTspSolver.Enums;
using UnityEditor.Sprites;

namespace GeneticTspSolver
{
    public class GeneticAlgorithm<T>
    {
        public Population<T> Population { get; set; }
        public List<Generation<T>> Generations = new List<Generation<T>>();

        public event EventHandler OnRan;
        public event EventHandler OnBestChange;
        public event EventHandler OnTerminate;

        public VerbosityLevel VerbosityLevel = Enums.VerbosityLevel.NewBests;

        public CGUtils<T> CGUtils;
        public ExecutionEnvironment ExecutionEnvironment = ExecutionEnvironment.Linear;

        public Stopwatch Stopwatch { get; set; } = Stopwatch.StartNew();

        public GeneticAlgorithm(
            int chromosomesCount,
            int genesCount,
            List<T> values,
            Func<Chromosome<T>, double> evaluate,
            double comparer,
            double eliteFactor,
            bool isUnique,
            VerbosityLevel verbosityLevel = VerbosityLevel.NewBests,
            ExecutionEnvironment executionEnvironment = ExecutionEnvironment.Linear,
            ComputeShader compute = null,
            EventHandler on_ran = null,
            EventHandler on_best_change = null,
            EventHandler on_terminate = null
        ) {
            // Benchmark
            Stopwatch.Restart();

            genesCount = Math.Min(genesCount, values.Count);

            Crossover<T>.Initialize(eliteFactor);
            Mutation<T>.Initialize();
            Fitness<T>.Initialize(evaluate, comparer);
            Picker<T>.Initialize(eliteFactor);

            var adam_values = values.Take(genesCount);

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

            ExecutionEnvironment = executionEnvironment;
            if (ExecutionEnvironment == ExecutionEnvironment.CGParallel && compute)
                CGUtils = new CGUtils<T>(compute, this);

            Population = new Population<T>(
                parent: this,
                id: 0,
                chromosomes_count: chromosomesCount,
                genes_count: genesCount,
                pool: values.ToArray(),
                adam_values: adam_values.ToArray()
            );

            if (ExecutionEnvironment == ExecutionEnvironment.CGParallel && compute)
            {
                CGUtils.InitVariables();
                //CGUtils.InitStates();
            }

            Population.Crossover = new Crossover<T>();
            Population.Mutation = new Mutation<T>();
            Population.Picker = new Picker<T>(Population);

            VerbosityLevel = verbosityLevel;

            OnRan = on_ran;
            OnBestChange = on_best_change;
            OnTerminate = on_terminate;

            UnityEngine.Debug.LogError("First population created in " + Stopwatch.Elapsed);
        }

        public void Run()
        {
            while(!ITermination<T>.IsTerminated(this))
                Next();

            OnTerminate?.Invoke(this, EventArgs.Empty);
        }

        public IEnumerator<Generation<T>> Next()
        {
            var gen = new Generation<T>(Generations.LastOrDefault(), Population);
            Generations.Add(gen);

            Population.PerformPick();
            // TODO
            //Population.PerformCrossover();
            Population.PerformMutate();
            Population.PerformEvaluate();

            gen.Save();

            if (gen.Number < 5 || gen.Number % 50 == 0 || gen.IsImproved)
            {
                UnityEngine.Debug.LogWarning(gen);
                if(gen.IsImproved)
                    OnBestChange?.Invoke(Population, EventArgs.Empty);
            }

            OnRan?.Invoke(this, EventArgs.Empty);

            yield return gen;
        }
    }
}
