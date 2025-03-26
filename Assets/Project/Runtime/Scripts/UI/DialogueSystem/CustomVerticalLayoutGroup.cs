using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.UI
{
    [ExecuteInEditMode]

    public class CustomVerticalLayoutGroup : VerticalLayoutGroup
    {
        public static List<string> CustomFields = new List<string>
        {
            "leftPadding",
            "rightPadding",
            "topPadding",
            "bottomPadding",
            "spacing",
            "reverseArrangement",
            "childAlignment",
            "controlChildWidth",
            "controlChildHeight",
            "useChildWidth",
            "useChildHeight",
            "childForceExpandWidth",
            "childForceExpandHeight"
        };

        [InfoBox("This component is identical to a normal Vertical Layout Group, but exposes all of its properties to animators. If you're not using an animator, you should probably just use a regular Vertical Layout Group.")]

        public int leftPadding;

        public int rightPadding;

        public int topPadding;

        public int bottomPadding;

        public new float spacing;


        public new bool reverseArrangement;

        public new TextAnchor childAlignment;

        public bool controlChildWidth;

        public bool controlChildHeight;

        public bool useChildWidth;

        public bool useChildHeight;

        public new bool childForceExpandWidth;

        public new bool childForceExpandHeight;



        // Update is called once per frame
        private new void Update()
        {
            padding.left = leftPadding;
            padding.right = rightPadding;
            padding.top = topPadding;
            padding.bottom = bottomPadding;
            base.childAlignment = childAlignment;
            base.reverseArrangement = reverseArrangement;
            childControlHeight = controlChildHeight;
            childControlWidth = controlChildWidth;
            childScaleWidth = useChildWidth;
            childScaleHeight = useChildHeight;
            base.childForceExpandWidth = childForceExpandWidth;
            base.childForceExpandHeight = childForceExpandHeight;
            base.spacing = spacing;
        }
    }
}