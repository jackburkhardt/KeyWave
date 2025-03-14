using UnityEngine;

namespace Project.Editor.Scripts.Attributes.DrawerAttributes
{
    /// <summary>
    /// Add [AppPopup] to a string show a popup of app titles in the inspector.
    /// </summary>
    public class PointsPopupAttribute : PropertyAttribute
    {

        public bool showReferenceDatabase = false;
        public bool showFilter = false;

        public PointsPopupAttribute(bool showReferenceDatabase = false, bool showFilter = false)
        {
            this.showReferenceDatabase = showReferenceDatabase;
            this.showFilter = showFilter;
        }
    }
}