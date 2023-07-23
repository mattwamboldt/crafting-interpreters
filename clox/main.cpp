#include "common.h"
#include "chunk.h"
#include "debug.h"

/*
NOTE: This whole thing is in a very C only style. Normally I'd take advantage
of C++ extensions that make things more readable, like structs having functions.

This code is essentially from the book with few modifications so bare that in mind.
*/

/*
* Challenges:
* - Chapter 14.1: find a way to rle the line numbers so they can be stored more efficiently.
* - Chapter 14.2: add an OP_CONSTANT_LONG that stores the operand as a larger 24bit num
* - Chapter 14.3: (sounds fun) implement realloc without library functions, only one large malloc at boot
*/

int main(int argc, const char* argv[])
{
    Chunk chunk;
    initChunk(&chunk);

    int constant = addConstant(&chunk, 1.2);
    writeChunk(&chunk, OP_CONSTANT, 123);
    writeChunk(&chunk, constant, 123);

    writeChunk(&chunk, OP_RETURN, 123);

    disassembleChunk(&chunk, "test chunk");
    freeChunk(&chunk);
    return 0;
}