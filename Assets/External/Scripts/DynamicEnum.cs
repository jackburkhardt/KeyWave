using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicEnum : MonoBehaviour
{
    public enum Section
    {
        A,
        B
    }

    public enum SectionA
    {
        A1,
        A2,
        A3
    }

    public enum SectionB
    {
        B1,
        B2,
        B3
    }

    public Section section;
    public SectionA sectionA;
    public SectionB sectionB;

    public System.Enum Value
    {
        get
        {
            switch (section)
            {
                case Section.A:
                    return sectionA;
                case Section.B:
                    return sectionB;
                default:
                    return null;
            }
        }
    }
}




