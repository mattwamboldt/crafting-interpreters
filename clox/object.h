#pragma once

#include "common.h"
#include "chunk.h"
#include "table.h"
#include "value.h"

#define OBJ_TYPE(value)     ((value).asObject()->type)

#define IS_BOUND_METHOD(value)  isObjType(value, OBJ_BOUND_METHOD)
#define IS_CLASS(value)         isObjType(value, OBJ_CLASS)
#define IS_CLOSURE(value)       isObjType(value, OBJ_CLOSURE)
#define IS_FUNCTION(value)      isObjType(value, OBJ_FUNCTION)
#define IS_INSTANCE(value)      isObjType(value, OBJ_INSTANCE)
#define IS_NATIVE(value)        isObjType(value, OBJ_NATIVE)
#define IS_STRING(value)        isObjType(value, OBJ_STRING)

#define AS_BOUND_METHOD(value)  ((ObjBoundMethod*)((value).asObject()))
#define AS_CLASS(value)         ((ObjClass*)((value).asObject()))
#define AS_CLOSURE(value)       ((ObjClosure*)((value).asObject()))
#define AS_FUNCTION(value)      ((ObjFunction*)((value).asObject()))
#define AS_INSTANCE(value)      ((ObjInstance*)((value).asObject()))
#define AS_NATIVE(value)        (((ObjNative*)((value).asObject()))->function)
#define AS_STRING(value)        ((ObjString*)((value).asObject()))
#define AS_CSTRING(value)       (((ObjString*)((value).asObject()))->chars)

enum ObjType
{
    OBJ_BOUND_METHOD,
    OBJ_CLASS,
    OBJ_CLOSURE,
    OBJ_FUNCTION,
    OBJ_INSTANCE,
    OBJ_NATIVE,
    OBJ_STRING,
    OBJ_UPVALUE
};

struct Obj
{
    ObjType type;
    bool isMarked;
    Obj* next;
};

struct ObjFunction
{
    Obj obj;
    int arity;
    int upvalueCount;
    Chunk chunk;
    ObjString* name;
};

typedef Value(*NativeFn)(int argCount, Value* args);

struct ObjNative
{
    Obj obj;
    NativeFn function;
};

struct ObjString
{
    Obj obj;
    int length;
    char* chars;
    uint32_t hash;
};

struct ObjUpvalue
{
    Obj obj;
    Value* location;
    Value closed;
    ObjUpvalue* next;
};

struct ObjClosure
{
    Obj obj;
    ObjFunction* function;
    ObjUpvalue** upvalues;
    int upvalueCount;
};

struct ObjClass
{
    Obj obj;
    ObjString* name;
    Table methods;
};

struct ObjInstance
{
    Obj obj;
    ObjClass* klass;
    Table fields;
};

struct ObjBoundMethod
{
    Obj obj;
    Value receiver;
    ObjClosure* method;
};

ObjBoundMethod* newBoundMethod(Value receiver, ObjClosure* method);
ObjClass* newClass(ObjString* name);
ObjClosure* newClosure(ObjFunction* function);
ObjFunction* newFunction();
ObjInstance* newInstance(ObjClass* klass);
ObjNative* newNative(NativeFn function);
ObjString* takeString(char* chars, int length);
ObjString* copyString(const char* chars, int length);
ObjUpvalue* newUpvalue(Value* slot);
void printObject(Value value);

static inline bool isObjType(Value value, ObjType type)
{
    return value.isObj() && value.asObject()->type == type;
}
