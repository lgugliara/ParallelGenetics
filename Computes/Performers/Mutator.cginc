#ifndef PERFORMERS_MUTATION
#define PERFORMERS_MUTATION

#include "../Utils/Context.cginc"
#include "../Utils/Buffers.cginc"
#include "../Utils/Random.cginc"

// todo: Use constant buffer
// size: PartitionsCount
StructuredBuffer<int> MutationCounts;

[numthreads(GENE_GROUP_SIZE,1,1)]
void Mutate_InnerSwap (uint3 id : SV_DispatchThreadID)
{
    //if(!GranularityGeneCheck(id.x))
    //{
    //    return;
    //}

    GeneData gene = Genes[id.x];

    NextThreadState(id.x);
    uint rnd = RandomRange(0, GenesCount, RandomStates[id.x]);

    uint index_swap = rnd + (gene.PartitionId * ChromosomesCount * GenesCount) + (gene.ChromosomeId * GenesCount);
    GeneData gene_swap = Genes[index_swap];
    uint value_swap = gene_swap.ValueId;

    AllMemoryBarrier();
    gene_swap.ValueId = gene.ValueId;
    Genes[index_swap] = gene_swap;

    AllMemoryBarrier();
    if(Genes[index_swap].ValueId == gene.ValueId)
    {
        gene.ValueId = value_swap;
        Genes[id.x] = gene;
    }
}

[numthreads(GENE_GROUP_SIZE,1,1)]
void Mutate_InnerMove (uint3 id : SV_DispatchThreadID)
{
    GeneData gene = Genes[id.x];

    uint start_from = id.x - uint(gene.Id);
    uint seed_from = RandomStates[start_from], seed_to = RandomStates[start_from + 1];
    uint geneID_from = RandomRange(0, GenesCount, seed_from), geneID_to = RandomRange(0, GenesCount, seed_to);
    uint offset = 0;

    if(geneID_from < geneID_to && gene.Id >= geneID_from && gene.Id < geneID_to)
        offset++;
    else if(geneID_from > geneID_to && gene.Id <= geneID_from && gene.Id > geneID_to)
        offset--;

    gene.ValueId = (gene.Id == geneID_to) ? Genes[start_from + geneID_from].ValueId : Genes[id.x + offset].ValueId;
    Genes[id.x] = gene;

    if(gene.Id == 0)
        NextThreadState(start_from);
    if(gene.Id == 1)
        NextThreadState(start_from + 1);
}

// size: ValuesCount
RWStructuredBuffer<uint> MutationExists;
// size: ValuesCount
RWStructuredBuffer<uint> MutationPool;

[numthreads(GENE_GROUP_SIZE,1,1)]
void Mutate_OuterSwap (uint3 id : SV_DispatchThreadID)
{
    for(uint i = 0; i < PartitionsCount; i++)
    {
        for(uint j = 0; j < ChromosomesCount; j++)
        {
            uint offset = (i * ChromosomesCount * GenesCount) + (j * GenesCount);
            if(id.x >= offset && id.x < offset + GenesCount)
            {
                GeneData gene = Genes[id.x];
                MutationExists[gene.ValueId] = 1;

                NextThreadState(id.x);
                uint rnd = RandomRange(0, ValuesCount, RandomStates[id.x]);

                AllMemoryBarrier();
                if(MutationExists[rnd] == 0)
                {
                    MutationPool[rnd] = gene.Id;
                }

                AllMemoryBarrier();
                if(MutationPool[rnd] == gene.Id)
                {
                    gene.ValueId = rnd;
                    Genes[id.x] = gene;
                }

                AllMemoryBarrier();
                MutationExists[gene.ValueId] = 0;
                MutationPool[rnd] = -1;
            }
            
            AllMemoryBarrier();
        }
    }
}

[numthreads(GENE_GROUP_SIZE,1,1)]
void Mutate_OuterMove (uint3 id : SV_DispatchThreadID)
{
    for(uint i = 0; i < PartitionsCount; i++)
    {
        uint mutations = uint(MutationCounts[i]);
        for(uint j = 0; j < ChromosomesCount; j++)
        {
            uint offset = (i * ChromosomesCount * GenesCount) + (j * GenesCount);
            uint seed = RandomStates[offset];

            for(uint k = 0; k < mutations; k++)
            {
                if(id.x >= offset && id.x < offset + GenesCount)
                {
                    GeneData gene = Genes[id.x];
                    MutationExists[gene.ValueId] = 1;

                    uint rnd = RandomRange(uint(0), ValuesCount, seed);

                    AllMemoryBarrier();
                    if(MutationExists[rnd] == 0)
                    {
                        MutationPool[rnd] = gene.Id;
                    }

                    AllMemoryBarrier();
                    uint _mutable = MutationPool[rnd];
                    if(gene.Id >= _mutable)
                    {
                        gene.ValueId = (gene.Id > _mutable) ? Genes[id.x - 1].ValueId : rnd;
                    }

                    AllMemoryBarrier();
                    Genes[id.x] = gene;

                    AllMemoryBarrier();
                    MutationExists[gene.ValueId] = 0;
                    MutationPool[rnd] = -1;
                }
                
                seed = xorshift(seed);
                AllMemoryBarrier();
            }
        }
    }
}

#endif