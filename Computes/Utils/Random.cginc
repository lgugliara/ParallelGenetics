#ifndef UTILS_RANDOM
#define UTILS_RANDOM

// size: PartitionsCount * ChromosomesCount * GenesCount
RWStructuredBuffer<uint> RandomStates;

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
    uint next = xorshift(RandomStates[id]);
    RandomStates[id] = next;
    return next;
}

uint Random(uint seed)
{
    return xorshift(seed);
}

uint RandomRange(uint min, uint max, uint seed)
{
	return (Random(seed) % (max - min)) - min;
}

#endif