using ParallelGenetics;

namespace ParallelGenetics.Utils
{
    public class EndlessTermination : ITermination
    {
        public bool IsTerminated(Genetics genetics) => false;
    }

    public interface ITermination
    {
        public bool IsTerminated(Genetics genetics);
    }
}