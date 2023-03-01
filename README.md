# ParallelGenetics
## GPU driven Genetic Algorithm (Unity/.NET)

Fast and light genetic algorithm library, inspired by:
> [giacomelli/GeneticSharp](https://github.com/giacomelli/GeneticSharp)

Long-term goals:
* `in-dev`  Avoid chromosomes and genes duplication between generations
* `reached` Make use of parallelization (multithreading)
* `in-dev`  Make use of CGPU for heavy operations
* `in-dev`  Generalize the algorithm to keep it easier to configure and start

> Designed and implemented with `<3`

## Usage
First, import the namespace and define the instance:

```cs
using GeneticTspSolver;

var ga = new GeneticAlgorithm<T>(
    chromosomes_count: chromosomes_count,
    genes_count: genes_count,
    values: values,
    evaluate: evaluate,
    comparer: best_score,
    mutation_factor: mutation_factor,
    isUnique: isUnique,
    on_best_change: BestChangeEvent
);
```
> replace `T` with your need type: `int`, `string`, or even your custom class or `Chromosome<T>` (not tested yet, teoretically it should work)

Then, start the algorithm with:

```cs
ga.Start();
```

> Note: since the `Start` method is sync, it is suggested to wrap it in a `Thread` instead, and start it like follows:

```cs
using System.Threading;

Thread t = new Thread(() => ga.Start());
 t.Start();
 ```

### Parameters

  * `chromosomes_count` [mandatory] number of chromosomes in the population
  * `genes_count` [mandatory] number of genes in a chromosome
  * `values` [mandatory] complete `List<T>` of all the available values
  * `evaluate` [mandatory] function (`Func<Chromosome<T>, double>`) used to evaluate a chromosome
  * `comparer` [mandatory] target score (used for comparison)
  * `mutation_factor` [mandatory] genes to apply mutation over the totality (range 0-1)
  * `isUnique` [mandatory] `boolean` if true, it manteins uniqueness in a chromosome' genes
  * `on_ran` [optional] `EventHandler` called at the end of each generation (if not `null`)
  * `on_best_change` [optional] `EventHandler` called every time a new best score has reached (if not `null`)
  * `on_terminate` [optional] `EventHandler` called when the termination condition has reached (if not `null`)

### Futures

  * `terminator` defines when to stop the algorithm

## Performers (operations)

### Evaluation

Perform the fitness function over the population.
Stores the fitness results into the Chromosome.

### Picking stages

#### Elite stage 
  Pass the best chromosomes (defined as `chromosomes_count * elite_factor`) from previous generation to the next one. No crossover is applied at this stage.
  
#### Tournement stage
  * Take 2 distinct chromosomes, define the winner as `best_1`
  * Take 2 other distinct chromosomes, define the winner as `best_2`
  * Apply crossover to `best_1` and `best_2`, obtaining `child_1` and `child_2`
  * Repeat until the new population is fully completed
  
### Mutation

#### Outer swapping (GPU-ready)
  Swaps values from the pool and genes (inserts new values).
  
#### Inner swapping (GPU-ready)
  Swaps values of the genes (re-orders some of the genes).
