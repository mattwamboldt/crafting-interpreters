#pragma once

#include "common.h"

enum ValueType
{
    VAL_BOOL,
    VAL_NIL,
    VAL_NUMBER,
};

// NOTE: Doing the macros as functions cause I'm using cpp compilation
struct Value
{
    ValueType type;
    union
    {
        bool boolean;
        double number;
    } as;

    Value()
    {
        type = VAL_NIL;
        as.number = 0;
    }

    Value(double value)
    {
        type = VAL_NUMBER;
        as.number = value;
    }

    Value(bool value)
    {
        type = VAL_BOOL;
        as.boolean = value;
    }

    bool isBool() { return type == VAL_BOOL; }
    bool isNil() { return type == VAL_NIL; }
    bool isNumber() { return type == VAL_NUMBER; }

    bool asBool() { return as.boolean; };
    double asNumber() { return as.number; };
};

#define BOOL_VAL(value)   (Value((bool)(value)))
#define NIL_VAL           (Value())
#define NUMBER_VAL(value) (Value((double)(value)))

struct ValueArray
{
    int capacity;
    int count;
    Value* values;;
};

bool valuesEqual(Value a, Value b);
void initValueArray(ValueArray* array);
void writeValueArray(ValueArray* array, Value value);
void freeValueArray(ValueArray* array);
void printValue(Value value);
