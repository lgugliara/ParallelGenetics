using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using GeneticTspSolver.CG;
using System.Collections;

namespace GeneticTspSolver
{
    public class Generation<T>
    {
        public int Number;
        public T[] Values;
        public double Fitness;
        public bool IsImproved;

        public Population<T> _Population;

        public Stopwatch Stopwatch { get; set; } = Stopwatch.StartNew();

        public Generation(Generation<T> previous, Population<T> population)
        {
            _Population = population;
            Number = previous != null ? (previous.Number + 1) : 0;
        }

        public void Save()
        {
            Values = _Population.Best.Values.ToArray();
            Fitness = _Population.Best.Fitness.Value;
            IsImproved = _Population.IsImproved;
        }

        public override string ToString()
        {
            var gen_log = "(GEN) " + Number + "\t (Fitness) " + Fitness;
            var mutation_log = _Population.Mutation.ToString();

            return string.Join("\t", new string[] {
                gen_log,
                mutation_log
            });
        }
    }
}
