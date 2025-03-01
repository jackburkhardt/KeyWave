using UnityEngine;

namespace Project.Editor.Scripts.Attributes.DrawerAttributes
{
    /// <summary>
    /// Add [AppPopup] to a string show a popup of app titles in the inspector.
    /// </summary>
    public class SmartWatchAppPopupAttribute : PropertyAttribute
    {

        public bool showReferenceDatabase = false;
        public bool showFilter = false;

        public SmartWatchAppPopupAttribute(bool showReferenceDatabase = false, bool showFilter = false)
        {
            this.showReferenceDatabase = showReferenceDatabase;
            this.showFilter = showFilter;
        }
    }
}