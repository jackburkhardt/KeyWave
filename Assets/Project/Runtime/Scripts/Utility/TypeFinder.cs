using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class TypeFinder
{
    public static List<Type> FindAllClassesOfType<T>()
    {
        return Assembly.GetAssembly(typeof(T))
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(T)))
            .ToList();
    }
    
    
}