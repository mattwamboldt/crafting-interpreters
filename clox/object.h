#pragma once

#include "common.h"
#include "value.h"

#define OBJ_TYPE(value)     ((value).asObject()->type)
#define IS_STRING(value)    isObjType(value, OBJ_STRING)

#define AS_STRING(value)    ((ObjString*)((value).asObject()))
#define AS_CSTRING(value)   (((ObjString*)((value).asObject()))->chars)

enum ObjType
{
    OBJ_STRING,
};

struct Obj
{
    ObjType type;
    Obj* next;
};

struct ObjString
{
    Obj obj;
    int length;
    char* chars;
    uint32_t hash;
};

ObjString* takeString(char* chars, int length);
ObjString* copyString(const char* chars, int length);
void printObject(Value value);

static inline bool isObjType(Value value, ObjType type)
{
    return value.isObj() && value.asObject()->type == type;
}
