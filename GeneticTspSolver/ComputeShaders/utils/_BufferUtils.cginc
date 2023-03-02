#ifndef BUFFER_LIBS
#define BUFFER_LIBS

// todo: Pool type is not int, in cs it is <T>
RWStructuredBuffer<int> Pool;
// todo: AllValues type is not int, in cs it is <T>
RWStructuredBuffer<uint> AllValues;

// Random buffer (states)
RWStructuredBuffer<uint> AllStates;

// Mutations (x: value, y: index)
RWStructuredBuffer<uint2> Mutation_Insertions;
RWStructuredBuffer<uint2> Mutation_Remotions;

#endif