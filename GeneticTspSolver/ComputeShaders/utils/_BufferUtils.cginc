#ifndef BUFFER_LIBS
#define BUFFER_LIBS

// todo: Pool type is not int, in cs it is <T>
RWStructuredBuffer<int> Pool;
// todo: AllValues type is not int, in cs it is <T>
RWStructuredBuffer<int> AllValues;

// Random buffer (states)
RWStructuredBuffer<uint> AllStates;

#endif