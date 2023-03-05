using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using GeneticTspSolver;
using GeneticTspSolver.Enums;
using ReplyChallenge2022;

// ensure class initializer is called whenever scripts recompile
[InitializeOnLoadAttribute]
public class Test : MonoBehaviour
{
    public string fileName = "5";
    public int bestScore = 19_913_031;
    public int chromosomesCount = 4;
    public int genesCount = 16;
    public double eliteFactor = 0.1;
    public bool isUnique = true;

    public VerbosityLevel verbosityLevel = VerbosityLevel.NewBests;

    public ExecutionEnvironment executionEnvironment = ExecutionEnvironment.Parallel;
    public ComputeShader gaCompute;

    // Runtime
    GeneticAlgorithm<int> GeneticAlgorithm;
    Func<Chromosome<int>, double> evaluate = (Chromosome<int> chromosome) =>
    {
        var currentTurn = 0;
        var score = 0;
        var currStamina = GameParameter.InitialStamina;
        var nextActionsToTake = new Dictionary<int, int>();
        int geneIndex = 0;
        var demon = GameParameter.Demons[(chromosome.Genes[geneIndex]).Value];

        while (currentTurn < GameParameter.Turns)
        {
            if (nextActionsToTake.ContainsKey(currentTurn))
            {
                currStamina = Math.Min(currStamina + nextActionsToTake[currentTurn], GameParameter.MaxStamina);
                nextActionsToTake.Remove(currentTurn);
            }
            if (currStamina >= demon.StaminaToDefeat)
            {
                currStamina -= demon.StaminaToDefeat;
                score += demon.Fragments.Take(GameParameter.Turns - currentTurn).Sum();

                if (!nextActionsToTake.ContainsKey(currentTurn + demon.TurnBeforeStamina))
                    nextActionsToTake.Add(currentTurn + demon.TurnBeforeStamina, 0);

                nextActionsToTake[currentTurn + demon.TurnBeforeStamina] += demon.StaminaRecovered;
                currentTurn++;
                geneIndex++;

                if (geneIndex >= chromosome.Genes.Count)
                    break;
                
                demon = GameParameter.Demons[(chromosome.Genes[geneIndex]).Value];
            }
            else
            {
                if (nextActionsToTake.Any())
                    currentTurn = nextActionsToTake.Min(x => x.Key);
                else
                    break;
            }
        }

        return score;
    };

    void BestChangeEvent(object sender, EventArgs e)
    {
        FileHandler.DrawAdam(
            @"Assets/ParallelGenetics/ReplyChallenges/2022/Out/" + fileName + ".txt",
            (sender as Population<int>).Best
        );
    }

    void TestParallel()
    {
        var KernelId = gaCompute.FindKernel("Mutate");
        System.Random random = new System.Random();

        var all_states_bufferId = Shader.PropertyToID("AllStates");
        uint[] all_states = Enumerable.Range(0, chromosomesCount * genesCount).Select(x => (uint)random.Next()).ToArray();
        var AllStates_Buffer = new ComputeBuffer(chromosomesCount * genesCount, sizeof(uint));
        AllStates_Buffer.SetData(all_states);
        gaCompute.SetBuffer(KernelId, all_states_bufferId, AllStates_Buffer);

        var all_values_bufferId = Shader.PropertyToID("AllValues");
        uint[] all_values = Enumerable.Range(0, chromosomesCount * genesCount).Select(x => (uint)x).ToArray();
        var AllValues_Buffer = new ComputeBuffer(chromosomesCount * genesCount, sizeof(uint));
        AllValues_Buffer.SetData(all_values);
        gaCompute.SetBuffer(KernelId, all_values_bufferId, AllValues_Buffer);

        UnityEngine.Debug.Log(string.Join(";", all_values));

        gaCompute.SetInt("chromosomes_count", chromosomesCount);
        gaCompute.SetInt("genes_count", genesCount);

        var all_insertions_bufferId = Shader.PropertyToID("Mutation_Insertions");
        uint2[] all_insertions = Enumerable.Range(0, chromosomesCount).Select(x => new uint2((uint)(x * random.Next(0, genesCount)), (uint)random.Next(0, genesCount))).ToArray();
        var AllInsertions_Buffer = new ComputeBuffer(chromosomesCount, sizeof(uint) * 2);
        AllInsertions_Buffer.SetData(all_insertions);
        gaCompute.SetBuffer(KernelId, all_insertions_bufferId, AllInsertions_Buffer);

        var all_remotions_bufferId = Shader.PropertyToID("Mutation_Remotions");
        uint2[] all_remotions = Enumerable.Range(0, chromosomesCount).Select(x=> new uint2((uint)(x * random.Next(0, genesCount)), (uint)random.Next(0, genesCount))).ToArray();
        var AllRemotions_Buffer = new ComputeBuffer(chromosomesCount, sizeof(uint) * 2);
        AllRemotions_Buffer.SetData(all_remotions);
        gaCompute.SetBuffer(KernelId, all_remotions_bufferId, AllRemotions_Buffer);

        gaCompute.Dispatch(KernelId, chromosomesCount * genesCount / 64, 1, 1);

        AllValues_Buffer.GetData(all_values);

        for (int i = 0; i < chromosomesCount; i++)
            UnityEngine.Debug.Log(string.Join(";", all_values.Skip(i * genesCount).Take(genesCount)));

        //for (int i = 0; i <= all_values.Max(); i++)
        //    UnityEngine.Debug.Log(i + ": " + all_values.Where(x => x == i).Count() + "\t/\t" + all_values.Length);

        UnityEngine.Debug.Log("Distinct values: " + all_values.Distinct().Count() + " | Total values: " + all_values.Length);

        //for (int r = 0; r < chromosomes_count; r++)
        //{
        //    UnityEngine.Debug.LogWarning("----- (Test number " + r + ") -----");

        //    AllValues_Buffer.GetData(all_values);

        //    for (int i = 0; i < chromosomes_count; i++)
        //        UnityEngine.Debug.Log(string.Join(";", all_values.Skip(i * genes_count).Take(genes_count)));

        //    //for (int i = 0; i <= all_values.Max(); i++)
        //    //    UnityEngine.Debug.Log(i + ": " + all_values.Where(x => x == i).Count() + "\t/\t" + all_values.Length);

        //    UnityEngine.Debug.Log("Distinct values: " + all_values.Distinct().Count() + " | Total values: " + all_values.Length);
        //}

        AllValues_Buffer.Dispose();
        AllStates_Buffer.Dispose();
        AllInsertions_Buffer.Dispose();
        AllRemotions_Buffer.Dispose();
        UnityEngine.Debug.LogWarning("----- (Test ends) -----");
    }

    private void Start()
    {
        //TestParallel();
        //return;

        FileHandler.ImportInputData(@"Assets/ParallelGenetics/ReplyChallenges/2022/In/" + fileName + ".txt");
        var values = GameParameter.Demons.Select(x => x.Id).ToList();
        //FileHandler.ImportAdamData(@"Assets/ParallelGenetics/ReplyChallenges/2022/Out/" + file_name + ".txt", adam);

        GeneticAlgorithm = new GeneticAlgorithm<int>(
            chromosomesCount: chromosomesCount,
            genesCount: genesCount,
            values: values,
            evaluate: evaluate,
            comparer: bestScore,
            eliteFactor: eliteFactor,
            isUnique: isUnique,
            compute: gaCompute,
            verbosityLevel: verbosityLevel,
            executionEnvironment: executionEnvironment,
            on_best_change: BestChangeEvent
        );
    }

    private void Update()
    {
        StartCoroutine(GeneticAlgorithm.Next());
    }

    private void OnApplicationQuit()
    {
        //if (t != null)
        //    t.Abort();
    }
}