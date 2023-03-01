using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GeneticTspSolver;
using ReplyChallenge2022;
using Unity.Mathematics;
using System.Diagnostics;

// ensure class initializer is called whenever scripts recompile
[InitializeOnLoadAttribute]
public class Test : MonoBehaviour
{
    public string file_name = "5";
    public int best_score = 19_913_031;
    public int chromosomes_count = 20;
    public int genes_count = 10000;
    public double elite_factor = 0.1;
    public double mutation_factor = 0.01;
    public bool isUnique = true;

    public ComputeShader compute;

    Thread t;

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
            @"Assets/ReplyChallenges/2022/Out/" + file_name + ".txt",
            (sender as Population<int>).Best
        );
    }

    void TestParallel()
    {
        int chromosomes_count = 4;
        int genes_count = 16;
        var KernelId = compute.FindKernel("InnerMutate");

        System.Random rnd = new System.Random();
        var all_states_bufferId = Shader.PropertyToID("AllStates");
        uint[] all_states = Enumerable.Range(0, chromosomes_count * genes_count).Select(x => (uint)rnd.Next()).ToArray();
        var AllStates_Buffer = new ComputeBuffer(chromosomes_count * genes_count, sizeof(uint));
        AllStates_Buffer.SetData(all_states);
        compute.SetBuffer(KernelId, all_states_bufferId, AllStates_Buffer);

        var all_values_bufferId = Shader.PropertyToID("AllValues");
        uint[] all_values = Enumerable.Range(0, chromosomes_count * genes_count).Select(x => (uint)x).ToArray();
        var AllValues_Buffer = new ComputeBuffer(chromosomes_count * genes_count, sizeof(uint));
        AllValues_Buffer.SetData(all_values);
        compute.SetBuffer(KernelId, all_values_bufferId, AllValues_Buffer);

        compute.SetFloat("chromosomes_count", chromosomes_count);
        compute.SetFloat("genes_count", genes_count);

        for (int r = 0; r < 16; r++)
        {
            UnityEngine.Debug.LogWarning("----- (Test number " + r + ") -----");

            compute.Dispatch(KernelId, chromosomes_count * genes_count / 64, 1, 1);

            AllValues_Buffer.GetData(all_values);

            for (int i = 0; i < chromosomes_count; i++)
                UnityEngine.Debug.Log(string.Join(";", all_values.Skip(i * genes_count).Take(genes_count)));

            //for (int i = 0; i <= all_values.Max(); i++)
            //    UnityEngine.Debug.Log(i + ": " + all_values.Where(x => x == i).Count() + "\t/\t" + all_values.Length);

            UnityEngine.Debug.Log("Distinct values: " + all_values.Distinct().Count() + " | Total values: " + all_values.Length);
        }

        AllValues_Buffer.Dispose();
        UnityEngine.Debug.LogWarning("----- (Test ends) -----");
    }

    void Start()
    {
        TestParallel();

        FileHandler.ImportInputData(@"Assets/ReplyChallenges/2022/In/" + file_name + ".txt");

        var values = GameParameter.Demons.Select(x => x.Id).ToList();

        //FileHandler.ImportAdamData(@"Assets/ReplyChallenges/2022/Out/" + file_name + ".txt", adam);

        //var ga = new GeneticAlgorithm<int>(
        //    chromosomes_count: chromosomes_count,
        //    genes_count: genes_count,
        //    values: values,
        //    evaluate: evaluate,
        //    comparer: best_score,
        //    elite_factor: elite_factor,
        //    mutation_factor: mutation_factor,
        //    isUnique: isUnique,
        //    on_best_change: BestChangeEvent,
        //    compute: compute
        //);

        //t = new Thread(() => ga.Start());
        //t.Start();
    }

    void OnApplicationQuit()
    {
        t.Abort();
    }
}