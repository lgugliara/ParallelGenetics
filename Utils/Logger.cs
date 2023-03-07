using Debug = UnityEngine.Debug;
using ParallelGenetics;

namespace ParallelGenetics.Utils
{
    public static class Logger
    {
        public static void Log(string message, bool condition = true)
        {
            if(condition)
                Debug.Log(message);
        }
    }
}