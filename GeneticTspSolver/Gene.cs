using System;
using System.Collections.Generic;
using TreeEditor;

namespace GeneticTspSolver
{
    public class Gene<T> : IEquatable<T>
    {
        public Chromosome<T> Parent { get; set; }
        public int Id { get; private set; }

        public T Value
        {
            get => Parent.Values[Id];
            set => Parent.Values[Id] = value;
        }

        public Gene(Chromosome<T> parent, int id, T value)
        {
            Parent = parent;
            Id = id;
            Parent.Genes[id] = this;

            Value = value;
        }

        public override string ToString() => Value.ToString();

        public static Gene<T> From(Gene<T> from, Chromosome<T> parent) => new(parent, from.Id, from.Value);

        public bool Equals(T other) => Value.Equals(other);
    }
}
