#include <stdio.h>
#include <string.h>

#include "object.h"
#include "memory.h"
#include "value.h"

void initValueArray(ValueArray* array)
{
    array->values = NULL;
    array->capacity = 0;
    array->count = 0;
}

void writeValueArray(ValueArray* array, Value value)
{
    if (array->capacity < array->count + 1)
    {
        int oldCapacity = array->capacity;
        array->capacity = GROW_CAPACITY(oldCapacity);
        array->values = GROW_ARRAY(Value, array->values, oldCapacity, array->capacity);
    }

    array->values[array->count] = value;
    array->count++;
}

void freeValueArray(ValueArray* array)
{
    FREE_ARRAY(Value, array->values, array->capacity);
    initValueArray(array);
}

void printValue(Value value)
{
    switch (value.type)
    {
        case VAL_BOOL:
            printf(value.asBool() ? "true" : "false");
            break;
        case VAL_NIL: printf("nil"); break;
        case VAL_NUMBER: printf("%g", value.asNumber()); break;
        case VAL_OBJ: printObject(value); break;
    }
}

bool valuesEqual(Value a, Value b)
{
    if (a.type != b.type)
    {
        return false;
    }

    switch (a.type)
    {
        case VAL_BOOL:  
            return a.asBool() == b.asBool();

        case VAL_NIL:
            return true;

        case VAL_NUMBER:
            return a.asNumber() == b.asNumber();

        case VAL_OBJ:
            return a.asObject() == b.asObject();

        default:
            return false;
    }
}
