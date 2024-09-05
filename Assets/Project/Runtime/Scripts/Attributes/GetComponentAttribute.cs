using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

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

