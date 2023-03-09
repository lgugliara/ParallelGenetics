#ifndef UTILS_CONTEXT
#define UTILS_CONTEXT

uint PartitionsCount;
uint ChromosomesCount;
uint GenesCount;
uint ValuesCount;

bool GranularityGeneCheck(uint ThreadID)
{
    return ThreadID < (PartitionsCount * ChromosomesCount * GenesCount);
}
bool GranularityChromosomeCheck(uint ThreadID)
{
    return ThreadID < (PartitionsCount * ChromosomesCount);
}
bool GranularityPartitionCheck(uint ThreadID)
{
    return ThreadID < PartitionsCount;
}

#endif