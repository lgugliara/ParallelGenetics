#ifndef RANDOM_LIBS
#define RANDOM_LIBS

#include "UnityShaderVariables.cginc"

#include "../utils/_BufferUtils.cginc"

#include "./Context.cginc"

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

uint NextThreadState(Context context)
{
    uint next = xorshift(AllStates[context.ThreadID]);
    AllStates[context.ThreadID] = next;
    return next;
}

uint Random(Context context)
{
    return NextThreadState(context);
}

uint RandomRange(Context context, uint min, uint max)
{
	return (Random(context) % (max - min)) - min;
}

uint2 RandomRange2(Context context, uint min, uint max)
{
    return uint2(RandomRange(context, min, max), RandomRange(context, min, max));
}

// todo
/* uint RandomRangeDistinct(uint id, uint min, uint max)
{} */

#endif