#include "UnityShaderVariables.cginc"

RWStructuredBuffer<uint> AllStates;

uint xorshift(uint s)
{
    s ^= 2747636419u;
    s *= 2654435769u;
    s ^= s >> 16;
    s *= 2654435769u;
    s ^= s >> 16;
    s *= 2654435769u;
    return s;
}

uint NextThreadState(uint id)
{
    uint next = xorshift(AllStates[id]);
    AllStates[id] = next;
    return next;
}

uint Random(uint id)
{
    return NextThreadState(id);
}

uint RandomRange(uint id, uint min, uint max)
{
	return (Random(id) % (max - min)) - min;
}

uint2 RandomRange2(uint id, uint min, uint max)
{
    return uint2(RandomRange(id, min, max), RandomRange(id, min, max));
}

// todo
uint RandomRangeDistinct(uint id, uint min, uint max)
{}