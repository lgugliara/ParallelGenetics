using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

using ParallelGenetics.Abstracts;
using ParallelGenetics.Enums;
using ParallelGenetics.Utils;
using Logger = ParallelGenetics.Utils.Logger;

namespace ParallelGenetics
{
    public class Genetics
    {
        public string Id = Guid.NewGuid().ToString().Substring(0, 8);

        public ITermination Termination = new EndlessTermination();
        public VerbosityLevel VerbosityLevel;
        public ExecutionEnvironment ExecutionEnvironment;

        public event EventHandler OnRan;
        public event EventHandler OnBestChange;
        public event EventHandler OnTerminate;

        public List<PopulationBase> Populations = new List<PopulationBase>();

        public ComputeShader Compute;

        public Stopwatch Stopwatch { get; set; } = Stopwatch.StartNew();

        public Genetics(
            ITermination termination = null,
            VerbosityLevel verbosityLevel = VerbosityLevel.NewBests,
            ExecutionEnvironment executionEnvironment = ExecutionEnvironment.Linear,
            EventHandler on_ran = null,
            EventHandler on_best_change = null,
            EventHandler on_terminate = null,
            ComputeShader compute = null
        ) {
            Stopwatch.Restart();

            if(termination != null)
                Termination = termination;
            VerbosityLevel = verbosityLevel;
            ExecutionEnvironment = executionEnvironment;

            OnRan = on_ran;
            OnBestChange = on_best_change;
            OnTerminate = on_terminate;

            Compute = compute;

            Logger.Log("Genetics created. Elapsed: " + Stopwatch.Elapsed);
        }

        public PopulationBase AddPopulation<T>(
            int chromosomesCount,
            int genesCount,
            T[] values,
            int partitionsCount = 1
        )
        {
            Stopwatch.Restart();

            var population = new Population<T>(
                genetics: this,
                id: Populations.Count,
                partitionsCount: partitionsCount,
                chromosomesCount: chromosomesCount,
                genesCount: genesCount,
                allValues: values
            );

            Populations.Add(population);

            Logger.Log("Population added. Elapsed: " + Stopwatch.Elapsed);

            return population;
        }

        public IEnumerator Run()
        {
            while (!Termination.IsTerminated(this))
            {
                Parallel.ForEach(
                    Populations,
                    p => p.Next()
                );

                yield return null;
            }

            OnTerminate?.Invoke(this, EventArgs.Empty);
        }

        public void Save(ChromosomeBase chromosome)
        {
            OnBestChange?.Invoke(chromosome, EventArgs.Empty);
        }
        public void Stop()
        {
            OnTerminate?.Invoke(this, EventArgs.Empty);
        }
    }
}