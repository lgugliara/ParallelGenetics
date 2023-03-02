#ifndef CONTEXT_LIBS
#define CONTEXT_LIBS

#include "../utils/_VariableUtils.cginc"

struct Context 
{
    uint ThreadID;
    uint ChromosomeID;
    uint GeneID;
    uint _offset;
};

Context GetContext(uint ThreadID) {
    Context c;
    c.ThreadID = ThreadID;
    c.GeneID = ThreadID % genes_count;
    c._offset = ThreadID - c.GeneID;
    c.ChromosomeID = c._offset / genes_count;
    return c;
}

#endif