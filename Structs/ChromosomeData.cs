namespace ParallelGenetics.Structs
{
    public struct ChromosomeData
    {
        public int PartitionId;
        public int Id;
        public double Fitness;
        public int LastElite;

        public ChromosomeData(int partitionId, int id)
        {
            PartitionId = partitionId;
            Id = id;
            Fitness = 0;
            LastElite = -1;
        }
    }
}