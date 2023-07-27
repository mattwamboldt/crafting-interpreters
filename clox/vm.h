#pragma once

#include "chunk.h"
#include "value.h"

#define STACK_MAX 256

struct VM
{
    Chunk* chunk;
    uint8_t* ip;
    Value stack[STACK_MAX];
    Value* stackTop;
};

enum InterpretResult
{
    INTERPRET_OK,
    INTERPRET_COMPILE_ERROR,
    INTERPRET_RUNTIME_ERROR
};

void initVM();
void freeVM();
InterpretResult interpret(const char* source);
void push(Value value);
Value pop();
