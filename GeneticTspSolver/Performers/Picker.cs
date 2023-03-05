using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Diagnostics;
using GeneticTspSolver.CG;
using GeneticTspSolver.Enums;
using System.Collections.Generic;

namespace GeneticTspSolver
{
    public class Picker<T>
    {
        public Population<T> Population;

        public int ElitesCount => (int)Math.Max(1, EliteFactor * Population.ChromosomesCount);
        public static double EliteFactor = 1;

        public Picker(Population<T> population)
        {
            Population = population;

            if (Population.Parent.ExecutionEnvironment == ExecutionEnvironment.CGParallel)
            {
                var cg = Population.Parent.CGUtils;
                cg.InitValues(cg.Picker_KernelID);
            }
        }

        public void Pick()
        {
            _PickElites();
            _PickTournament();
        }

        private void _PickElites()
        {
            var elites = Population.Chromosomes
                .OrderByDescending(c => c.Fitness.Value)
                .Take(ElitesCount)
                .ToArray();

            foreach (var e in elites) e.LastEliteGeneration = Population.Parent.Generations.LastOrDefault();

            List<Chromosome<T>> pickable_chromosome;
            switch (Population.Parent.ExecutionEnvironment)
            {
                // GPU
                case ExecutionEnvironment.CGParallel:
                    var cg = Population.Parent.CGUtils;

                    var elites_ids = elites
                        .AsParallel()
                        .Select(c => (uint)c.Id)
                        .ToArray();
                    var elites_ids_Buffer = new ComputeBuffer(elites_ids.Length, sizeof(uint));
                    elites_ids_Buffer.SetData(elites_ids);
                    cg.Compute.SetBuffer(cg.Picker_KernelID, "Elites", elites_ids_Buffer);

                    uint[] whoIsElite = Population.Chromosomes
                        .AsParallel()
                        .Select(c => c.IsEliteOf(Population.Parent.Generations.LastOrDefault()) ? (uint)1 : (uint)0)
                        .ToArray();
                    var whoIsElite_Buffer = new ComputeBuffer(whoIsElite.Length, sizeof(uint));
                    whoIsElite_Buffer.SetData(whoIsElite);
                    cg.Compute.SetBuffer(cg.Picker_KernelID, "WhoIsElite", whoIsElite_Buffer);

                    cg.Values_Buffer.SetData(Population.AllValues);
                    cg.Compute.SetInt("elites_count", ElitesCount);
                    cg.Compute.SetInt("generation", Population.Parent.Generations.LastOrDefault().Number);

                    cg.Compute.Dispatch(cg.Picker_KernelID, cg.GetThreadGroups, 1, 1);

                    cg.Values_Buffer.GetData(Population.AllValues);
                    Parallel.ForEach(
                        Population.Chromosomes,
                        Chromosome<T>.UpdateLookup
                    );

                    elites_ids_Buffer.Dispose();
                    whoIsElite_Buffer.Dispose();
                    break;

                // Parallel
                case ExecutionEnvironment.Parallel:
                    pickable_chromosome = Population.Chromosomes
                        .AsParallel()
                        .Where(c => !c.IsEliteOf(Population.Parent.Generations.LastOrDefault()))
                        .ToList();
                    Parallel.ForEach(
                        pickable_chromosome,
                        (c, s, i) => Chromosome<T>.Copy(elites[(int)i % elites.Length], c)
                    );
                    break;

                // Linear
                case ExecutionEnvironment.Linear:
                    pickable_chromosome = Population.Chromosomes
                        .Where(c => !c.IsEliteOf(Population.Parent.Generations.LastOrDefault()))
                        .ToList();
                    for (var i = 0; i < pickable_chromosome.Count; i++)
                        Chromosome<T>.Copy(elites[(int)i % elites.Length], pickable_chromosome[i]);
                    break;
            }
        }

        private void _PickTournament()
        {

        }

        public static void Initialize(double elite_factor)
        {
            EliteFactor = elite_factor;
        }
    }
}