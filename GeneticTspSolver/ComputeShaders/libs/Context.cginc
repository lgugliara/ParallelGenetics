struct Context 
{
    uint ThreadID;
    uint ChromosomeID;
    uint GeneID;
    uint _offset;
};

Context GetContext(uint ThreadID, uint chromosomes_count, uint genes_count) {
    Context c;
    c.ThreadID = ThreadID;
    c.GeneID = ThreadID % genes_count;
    c._offset = ThreadID - c.GeneID;
    c.ChromosomeID = c._offset / genes_count;
    return c;
}