using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace GeneticTspSolver
{
    public class Chromosome<T> : IComparable
    {
        public Population<T> Parent { get; set; }
        public int Id { get; private set; }

        public Fitness<T> Fitness { get; set; } = new Fitness<T>();

        public ArraySegment<T> Values;
        public ArraySegment<Gene<T>> Genes;
        public Dictionary<T, int> Lookup;

        public int GenesCount => Values.Count;

        public static Chromosome<T> Adam { get; set; }

        public Chromosome(Population<T> parent, int id, int genes_count)
        {
            Parent = parent;
            Id = id;
            Fitness = new Fitness<T>();

            Values = new ArraySegment<T>(parent.AllValues, genes_count * id, genes_count);
            Genes = new ArraySegment<Gene<T>>(parent.AllGenes, genes_count * id, genes_count);

            _TransferTo(this, this);
        }

        private static void _TransferTo(Chromosome<T> chromosome, Chromosome<T> to)
        {
            //var keyValues = chromosome.Genes.AsParallel().Select((g, i) =>
            //{
            //    to.Values[i] = chromosome.Values[i];
            //    if (to.Genes[i] == null) to.Genes[i] = new Gene<T>(to, i, to.Values[i]);
            //    return new { v = to.Values[i], i };
            //}).ToArray();

            //UnityEngine.Debug.Log(to.Id + " | Length: " + keyValues.Length + "\t\tDistinct values: " + keyValues.Select(x => x.v).Distinct().Count());
            //UnityEngine.Debug.Log(to.Id + " | To chromosome: " + to.ToString());

            to.Lookup = chromosome.Genes.AsParallel().Select((g, i) =>
            {
                to.Values[i] = chromosome.Values[i];
                if (to.Genes[i] == null) to.Genes[i] = new Gene<T>(to, i, to.Values[i]);
                return new { v = to.Values[i], i };
            }).ToDictionary(x => x.v, x => x.i);
        }

        public override string ToString() => Genes.Select(g => g.ToString()).Aggregate((i, j) => i + ';' + j);

        public static Chromosome<T> From(Chromosome<T> from, int id) => new(from.Parent, id, from.GenesCount);

        public static void Copy(Chromosome<T> from, Chromosome<T> to) => _TransferTo(from, to);

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            Chromosome<T> other = obj as Chromosome<T>;
            if (other != null)
                return Fitness.Value.CompareTo(other.Fitness.Value);
            else
                throw new ArgumentException("Object is not a Chromosome");
        }
    }
}
