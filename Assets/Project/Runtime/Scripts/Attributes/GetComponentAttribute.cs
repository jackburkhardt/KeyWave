using System;
using NaughtyAttributes;

namespace Project
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class GetComponentAttribute : DrawerAttribute
    {
        public GetComponentAttribute()
        {
         
        }
    }
}

