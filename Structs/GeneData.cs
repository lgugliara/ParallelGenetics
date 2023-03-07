namespace ParallelGenetics.Structs
{
    public struct GeneData
    {
        public int PartitionId;
        public int ChromosomeId;
        public int Id;
        public int ValueId;

        public GeneData(int partitionId, int chromosomeId, int id)
        {
            PartitionId = partitionId;
            ChromosomeId = chromosomeId;
            Id = id;
            ValueId = -1;
        }
    }
}