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

        // Kernels
        private static string _Crossover_KernelName = "Crossover";
        private static string _Mutation_KernelName = "Mutate";
        private static string _Evaluate_KernelName = "Evaluate";
        private static string _Picker_KernelName = "Pick";

        public int Crossover_KernelID;
        public int Mutation_KernelID;
        public int Evaluate_KernelID;
        public int Picker_KernelID;

        // Buffers
        private static string _States_BufferName = "AllStates";
        private static string _Values_BufferName = "AllValues";

        public int States_BufferID;
        public int Values_BufferID;

        public ComputeBuffer States_Buffer;
        public ComputeBuffer Values_Buffer;

        // Miscs
        private static int _GroupSize = 64;
        public int GetThreadGroups => (int)Math.Ceiling((double)(GeneticAlgorithm.Population.GenesCount / _GroupSize));

        public CGUtils(ComputeShader compute, GeneticAlgorithm<T> geneticAlgorithm)
        {
            GeneticAlgorithm = geneticAlgorithm;
            Compute = compute;

            // Kernel IDs
            Crossover_KernelID = Compute.FindKernel(_Crossover_KernelName);
            Mutation_KernelID = Compute.FindKernel(_Mutation_KernelName);
            Evaluate_KernelID = Compute.FindKernel(_Evaluate_KernelName);
            Picker_KernelID = Compute.FindKernel(_Picker_KernelName);
        }

        public void InitVariables()
        {
            Compute.SetInt("chromosomes_count", GeneticAlgorithm.Population.ChromosomesCount);
            Compute.SetInt("genes_count", GeneticAlgorithm.Population.GenesCount);
        }

        public void InitValues(int kernelID)
        {
            if(Values_Buffer == null)
                Values_Buffer = new ComputeBuffer(GeneticAlgorithm.Population.AllValues.Length, sizeof(uint));

            Values_BufferID = Shader.PropertyToID(_Values_BufferName);
            Values_Buffer.SetData(GeneticAlgorithm.Population.AllValues);
            Compute.SetBuffer(kernelID, Values_BufferID, Values_Buffer);
        }

        public void InitStates()
        {
            var random = new FastRandoms();

            States_BufferID = Shader.PropertyToID(_States_BufferName);

            uint[] all_states = Enumerable
                .Range(0, GeneticAlgorithm.Population.ChromosomesCount * GeneticAlgorithm.Population.GenesCount)
                .Select(x => (uint)random.GetInt(0, Int32.MaxValue))
                .ToArray();

            States_Buffer = new ComputeBuffer(GeneticAlgorithm.Population.ChromosomesCount * GeneticAlgorithm.Population.GenesCount, sizeof(uint));
            States_Buffer.SetData(
                Enumerable
                    .Range(0, GeneticAlgorithm.Population.ChromosomesCount * GeneticAlgorithm.Population.GenesCount)
                    .Select(x => (uint)random.GetInt(0, Int32.MaxValue))
                    .ToArray()
            );

            //_Compute.SetBuffer(_Crossover_KernelID, _States_BufferID, _States_Buffer);
            Compute.SetBuffer(Mutation_KernelID, States_BufferID, States_Buffer);
            //_Compute.SetBuffer(_Evaluate_KernelID, _States_BufferID, _States_Buffer);
            //_Compute.SetBuffer(_Picker_KernelID, _States_BufferID, _States_Buffer);
        }
    }
}