#ifndef MUTATION_UTILS
#define MUTATION_UTILS

#include "./_BufferUtils.cginc"
#include "./_VariableUtils.cginc"

#include "../libs/Context.cginc"
#include "../libs/Random.cginc"

// todo: use "mutation_factor" to apply on some chromosomes only
int mutation_factor;

void Mutation_Insert(Context context)
{
    // todo: use "mutation_factor" to determine when to apply swapping
    uint2 insertion = Mutation_Insertions[context.ChromosomeID];
    uint new_value = AllValues[context._offset + context.GeneID];
    
    if(context.GeneID == insertion.y)
        new_value = insertion.x;
    else if(context.GeneID > insertion.y)
        new_value = AllValues[context._offset + context.GeneID - 1];

    AllMemoryBarrierWithGroupSync();

    AllValues[context._offset + context.GeneID] = new_value;
}

void Mutation_Remove(Context context)
{
    // todo: use "mutation_factor" to determine when to apply swapping
    uint2 remotion = Mutation_Remotions[context.ChromosomeID];
    uint new_value = AllValues[context._offset + context.GeneID];
    
    if(context.GeneID == genes_count - 1)
        new_value = remotion.x;
    else if(context.GeneID >= remotion.y)
        new_value = AllValues[context._offset + context.GeneID + 1];

    AllMemoryBarrierWithGroupSync();

    AllValues[context._offset + context.GeneID] = new_value;
}

void Mutation_Inner (Context context)
{
    // todo: use "mutation_factor" to determine when to apply swapping
    uint2 ids = uint2(context._offset, context._offset) + RandomRange2(context, 0, genes_count);
    uint2 values = uint2(AllValues[ids.x], AllValues[ids.y]);

    AllMemoryBarrierWithGroupSync();

    AllValues[ids.x] = values.y;
    AllValues[ids.y] = values.x;
}

void Mutation_Outer (Context context)
{

}

#endif