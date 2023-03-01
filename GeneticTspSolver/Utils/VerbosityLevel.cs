using System;

namespace GeneticTspSolver.Enums
{
    [Flags]
    public enum VerbosityLevel
    {
        None = 0,
        TimeStatistics = 1,
        TimeEvery10GenStatistics = 2,
        ShowActions = 4
    }
}
