using System;
using UnityEngine;
using System.Linq;
using GeneticTspSolver;
using GeneticTspSolver.Utils;

namespace GeneticTspSolver.CG
{
    public class CGUtils<T>
    {
        public GeneticAlgorithm<T> GeneticAlgorithm;
        public ComputeShader Compute;

        private static string _Crossover_KernelName = "Crossover";
        private static string _Mutation_KernelName = "Mutate";
        private static string _Evaluate_KernelName = "Evaluate";
        private static string _Picker_KernelName = "Pick";

        public int Crossover_KernelID;
        public int Mutation_KernelID;
        public int Evaluate_KernelID;
        public int Picker_KernelID;

        private static string _States_BufferName = "AllStates";

        private int _States_BufferID;

        private ComputeBuffer _States_Buffer;

        private static int _GroupSize = 64;
        public static int GetThreadGroups(int count) => (int)Math.Ceiling((double)(count / _GroupSize));

        public CGUtils(ComputeShader compute, GeneticAlgorithm<T> geneticAlgorithm)
        {
            GeneticAlgorithm = geneticAlgorithm;
            Compute = compute;

            // Kernel IDs
            Crossover_KernelID = Compute.FindKernel(_Crossover_KernelName);
            Mutation_KernelID = Compute.FindKernel(_Mutation_KernelName);
            Evaluate_KernelID = Compute.FindKernel(_Evaluate_KernelName);
            Picker_KernelID = Compute.FindKernel(_Picker_KernelName);

            _InitVariables();
            _InitStates();
        }

        private void _InitVariables()
        {
            Compute.SetInt("chromosomes_count", GeneticAlgorithm.Population.ChromosomesCount);
            Compute.SetInt("genes_count", GeneticAlgorithm.Population.Best.GenesCount);
        }

        private void _InitStates()
        {
            var random = new FastRandoms();

            _States_BufferID = Shader.PropertyToID(_States_BufferName);

            uint[] all_states = Enumerable
                .Range(0, GeneticAlgorithm.Population.ChromosomesCount * GeneticAlgorithm.Population.Best.GenesCount)
                .Select(x => (uint)random.GetInt(0, Int32.MaxValue))
                .ToArray();

            _States_Buffer = new ComputeBuffer(GeneticAlgorithm.Population.ChromosomesCount * GeneticAlgorithm.Population.Best.GenesCount, sizeof(uint));
            _States_Buffer.SetData(
                Enumerable
                    .Range(0, GeneticAlgorithm.Population.ChromosomesCount * GeneticAlgorithm.Population.Best.GenesCount)
                    .Select(x => (uint)random.GetInt(0, Int32.MaxValue))
                    .ToArray()
            );

            //_Compute.SetBuffer(_Crossover_KernelID, _States_BufferID, _States_Buffer);
            Compute.SetBuffer(Mutation_KernelID, _States_BufferID, _States_Buffer);
            //_Compute.SetBuffer(_Evaluate_KernelID, _States_BufferID, _States_Buffer);
            //_Compute.SetBuffer(_Picker_KernelID, _States_BufferID, _States_Buffer);
        }
    }
}