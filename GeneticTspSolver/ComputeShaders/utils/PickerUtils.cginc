#ifndef PICKER_UTILS
#define PICKER_UTILS

#include "./_BufferUtils.cginc"
#include "./_VariableUtils.cginc"

#include "../libs/Context.cginc"
#include "../libs/Random.cginc"

StructuredBuffer<uint> Elites;
StructuredBuffer<uint> WhoIsElite;

int elites_count;
int generation;

void PickElites (Context context)
{
    bool isElite = WhoIsElite[context.ChromosomeID];
    if(!isElite)
    {
        uint copyFrom = Elites[context.ChromosomeID % elites_count];
        uint new_value = AllValues[genes_count * copyFrom + context.GeneID];
        AllValues[context._offset + context.GeneID] = new_value;
    }
}

void PickTournament (Context context)
{

}

#endif