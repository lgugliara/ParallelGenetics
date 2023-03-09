#ifndef UTILS_BUFFERS
#define UTILS_BUFFERS

#include "../Utils/Context.cginc"

#include "../Structs/PartitionData.cginc"
#include "../Structs/ChromosomeData.cginc"
#include "../Structs/GeneData.cginc"

RWStructuredBuffer<PartitionData> Partitions;
RWStructuredBuffer<ChromosomeData> Chromosomes;
RWStructuredBuffer<GeneData> Genes;

#endif