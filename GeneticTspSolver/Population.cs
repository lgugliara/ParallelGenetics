using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace GeneticTspSolver
{
    public class Population<T>
    {
        public GeneticAlgorithm<T> Parent { get; set; }
        public int Id { get; private set; }

        public T[] Pool;
        public T[] AllValues;
        public Gene<T>[] AllGenes;
        public Chromosome<T>[] Chromosomes { get; set; }

        public int ChromosomesCount => Chromosomes.Length;

        public Stopwatch Stopwatch { get; set; } = Stopwatch.StartNew();

        public Chromosome<T> Best
        {
            get => Chromosomes[_bestId];
            set => _bestId = value.Id;
        }

        private int _bestId = 0;

        public Population(GeneticAlgorithm<T> parent, int id, int chromosomes_count, int genes_count, T[] pool, T[] adam_values)
        {
            Parent = parent;
            Id = id;

            Pool = pool;

            AllValues = new T[chromosomes_count * genes_count];
            Parallel.For(
                0,
                AllValues.Length,
                i => AllValues[i] = adam_values[i % genes_count]
            );
            AllGenes = new Gene<T>[chromosomes_count * genes_count];

            Chromosomes = new Chromosome<T>[chromosomes_count];
            Chromosome<T>.Adam = new Chromosome<T>(this, 0, genes_count);
            Fitness<T>.Evaluate(Chromosome<T>.Adam);
            Parallel.For(
                0,
                chromosomes_count,
                i => Chromosomes[i] = Chromosome<T>.From(Chromosome<T>.Adam, i)
            );

            UnityEngine.Debug.LogAssertion("Adam fitness: " + Chromosome<T>.Adam.Fitness.Value);
        }

        // TODO
        public void PerformCrossover()
        {
            // Stopwatch.Restart();
            var creators = this.Chromosomes.OrderByDescending(x => x.Fitness.Value).Take(2).ToList();

            Parallel.ForEach(
                this.Chromosomes.Skip(2),
                c => Crossover<T>.Cross(creators[0], creators[1])
            );
            // UnityEngine.Debug.Log("Crossover done in " + Stopwatch.Elapsed);
        }

        public void PerformMutate()
        {
            Stopwatch.Restart();
            Parallel.ForEach(
                Chromosomes.AsParallel().Where(c => c.Id != _bestId),
                Mutation<T>.Mutate
            );
            UnityEngine.Debug.Log("Mutation done in " + Stopwatch.Elapsed);
        }

        public bool PerformEvaluate(EventHandler e)
        {
            Stopwatch.Restart();
            var hasChanged = Fitness<T>.Evaluate(this);
            UnityEngine.Debug.Log("Evaluation done in " + Stopwatch.Elapsed);

            if(hasChanged)
                e?.Invoke(this, EventArgs.Empty);

            return hasChanged;
        }

        public void PerformPick()
        {
            this.Stopwatch.Restart();
            Picker<T>.Pick(this);
            UnityEngine.Debug.Log("Picking done in " + Stopwatch.Elapsed);
        }
    }
}
