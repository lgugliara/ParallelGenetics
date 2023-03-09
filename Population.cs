using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ParallelGenetics.Abstracts;
using ParallelGenetics.Performers;
using ParallelGenetics.Structs;
using Logger = ParallelGenetics.Utils.Logger;

namespace ParallelGenetics
{
    public class Population<T> : PopulationBase
    {
        public Population(
            Genetics genetics,
            int id,
            int partitionsCount,
            int chromosomesCount,
            int genesCount,
            T[] allValues
        ) : base(
            genetics: genetics,
            id: id,
            partitionsCount: partitionsCount,
            chromosomesCount: chromosomesCount,
            genesCount: genesCount,
            allValues: allValues.Select(x => x as object).ToArray()
        )
        {
            Partitions = Enumerable
                .Range(0, PartitionsCount)
                .Select(i => new Partition<T>(this, i))
                .ToArray();
        }

        public void LoadAdams(IEnumerable<T[]> adams)
        {
            Stopwatch.Restart();

            var valuesList = adams.ToList();
            for (int i = 0; i < valuesList.Count; i++)
            {
                try
                {
                    var values = valuesList[i];
                    if (values.Length != GenesCount)
                        throw new ArgumentException("Values must be same amount as Genes count.");

                    Parallel.ForEach(
                        Partitions,
                        p =>
                        {
                            Parallel.For(
                                0,
                                values.Length,
                                j =>
                                {
                                    int index = Array.IndexOf(AllValues, values[j] as object);

                                    if (index < 0)
                                        throw new NullReferenceException("Value " + values[j].ToString() + " is invalid.");
                                    p.Chromosomes[i].Genes[j].ValueId = index;
                                }
                            );
                            Parallel.ForEach(
                                p.Chromosomes,
                                c =>
                                {
                                    ChromosomeBase.Copy(p.Chromosomes[i], c);
                                    p.Evaluator.Evaluate(c);
                                }
                            );
                        }
                    );
                }
                catch (Exception e)
                {
                    Logger.Log(e.Message);
                }
            }

            Logger.Log("Adam(s) loaded. Elapsed: " + Stopwatch.Elapsed);
        }
    }
}
