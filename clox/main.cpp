#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "common.h"
#include "chunk.h"
#include "debug.h"
#include "vm.h"

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
* - Chapter 17.3: Add support for a ternary operator
* - Chapter 21.1: Only add global variable names to the constant table once
* - Chapter 21.2: Come up with a more efficient way to store and access global variables without changing semantics (Lookup only in parse phase?)
* - Chapter 22.1: Make a faster way for local variable resolution in compiler, other than linear scan
* - Chapter 22.3: Add const variables
* - Chapter 22.4: Allow more than 256 variables in scope at a time
*/

static void repl()
{
    char line[1024];
    for (;;)
    {
        printf("> ");

        if (!fgets(line, sizeof(line), stdin))
        {
            printf("\n");
            break;
        }

        interpret(line);
    }
}

static char* readFile(const char* path)
{
    FILE* file = fopen(path, "rb");
    if (file == NULL)
    {
        fprintf(stderr, "Could not open file \"%s\".\n", path);
        exit(74);
    }

    fseek(file, 0L, SEEK_END);
    size_t fileSize = ftell(file);
    rewind(file);

    char* buffer = (char*)malloc(fileSize + 1);
    if (buffer == NULL)
    {
        fprintf(stderr, "Not enough memory to read \"%s\".\n", path);
        exit(74);
    }

    size_t bytesRead = fread(buffer, sizeof(char), fileSize, file);
    if (bytesRead < fileSize)
    {
        fprintf(stderr, "Could not read file \"%s\".\n", path);
        exit(74);
    }

    buffer[bytesRead] = '\0';

    fclose(file);
    return buffer;
}

static void runFile(const char* path)
{
    char* source = readFile(path);
    InterpretResult result = interpret(source);
    free(source);

    if (result == INTERPRET_COMPILE_ERROR) exit(65);
    if (result == INTERPRET_RUNTIME_ERROR) exit(70);
}

int main(int argc, const char* argv[])
{
    initVM();

    if (argc == 1)
    {
        repl();
    }
    else if (argc == 2)
    {
        runFile(argv[1]);
    }
    else
    {
        fprintf(stderr, "Usage: clox [path]\n");
        exit(64);
    }

    freeVM();
    return 0;
}