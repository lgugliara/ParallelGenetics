namespace ParallelGenetics.Structs
{
    public struct PartitionData
    {
        public int Id;
        public double Increase;

        public PartitionData(int id)
        {
            Id = id;
            Increase = 0;
        }
    }
}