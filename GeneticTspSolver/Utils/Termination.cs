using GeneticTspSolver;

namespace GeneticTspSolver
{    
    public interface ITermination<T>
    {
        public static bool IsTerminated(GeneticAlgorithm<T> geneticAlgorithm) => false;
    }
}
