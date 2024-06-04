// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace UnityEditor.Enemeteen {
	internal class AnimationWindowStyles {
		
		public static Texture2D pointIcon = EditorGUIUtility.LoadIcon("animationkeyframe");

		public static GUIContent playContent = EditorGUIUtility.TrIconContent("Animation.Play", "Play the animation clip.");
		public static GUIContent recordContent = EditorGUIUtility.TrIconContent("Animation.Record", "Enable/disable keyframe recording mode.");
		public static GUIContent previewContent = EditorGUIUtility.TrTextContent("Preview", "Enable/disable scene preview mode.");
		public static GUIContent prevKeyContent = EditorGUIUtility.TrIconContent("Animation.PrevKey", "Go to previous keyframe.");
		public static GUIContent nextKeyContent = EditorGUIUtility.TrIconContent("Animation.NextKey", "Go to next keyframe.");
		public static GUIContent firstKeyContent = EditorGUIUtility.TrIconContent("Animation.FirstKey", "Go to the beginning of the animation clip.");
		public static GUIContent lastKeyContent = EditorGUIUtility.TrIconContent("Animation.LastKey", "Go to the end of the animation clip.");
		public static GUIContent addKeyframeContent = EditorGUIUtility.TrIconContent("Animation.AddKeyframe", "Add keyframe.");
		public static GUIContent addEventContent = EditorGUIUtility.TrIconContent("Animation.AddEvent", "Add event.");
		public static GUIContent filterBySelectionContent = EditorGUIUtility.TrIconContent("Animation.FilterBySelection", "Filter by selection.");
		public static GUIContent sequencerLinkContent = EditorGUIUtility.TrIconContent("Animation.SequencerLink", "Animation Window is linked to Timeline Editor.  Press to Unlink.");

		public static GUIContent noAnimatableObjectSelectedText = EditorGUIUtility.TrTextContent("No animatable object selected.");
		public static GUIContent formatIsMissing = EditorGUIUtility.TrTextContent("To begin animating {0}, create {1}.");
		public static GUIContent animatorAndAnimationClip = EditorGUIUtility.TrTextContent("an Animator and an Animation Clip");
		public static GUIContent animationClip = EditorGUIUtility.TrTextContent("an Animation Clip");
		public static GUIContent create = EditorGUIUtility.TrTextContent("Create");
		public static GUIContent dopesheet = EditorGUIUtility.TrTextContent("Dopesheet");
		public static GUIContent curves = EditorGUIUtility.TrTextContent("Curves");
		public static GUIContent samples = EditorGUIUtility.TrTextContent("Samples");
		public static GUIContent createNewClip = EditorGUIUtility.TrTextContent("Create New Clip...");

		public static GUIContent animatorOptimizedText = EditorGUIUtility.TrTextContent("Editing and playback of animations on optimized game object hierarchy is not supported.\nPlease select a game object that does not have 'Optimize Game Objects' applied.");
		public static GUIContent readOnlyPropertiesLabel = EditorGUIUtility.TrTextContent("Animation Clip is Read-Only");
		public static GUIContent readOnlyPropertiesButton = EditorGUIUtility.TrTextContent("Show Read-Only Properties");

		public static GUIContent optionsContent = EditorGUIUtility.IconContent("_Menu");

		public static GUIStyle playHead = "AnimationPlayHead";

		public static GUIStyle animPlayToolBar = "AnimPlayToolbar";
		public static GUIStyle animClipToolBar = "AnimClipToolbar";
		public static GUIStyle animClipToolbarButton = "AnimClipToolbarButton";
		public static GUIStyle animClipToolbarPopup = "AnimClipToolbarPopup";
		public static GUIStyle timeRulerBackground = "TimeRulerBackground";
		public static GUIStyle curveEditorBackground = "CurveEditorBackground";
		public static GUIStyle curveEditorLabelTickmarks = "CurveEditorLabelTickmarks";
		public static GUIStyle eventBackground = "AnimationEventBackground";
		public static GUIStyle eventTooltip = "AnimationEventTooltip";
		public static GUIStyle eventTooltipArrow = "AnimationEventTooltipArrow";
		public static GUIStyle keyframeBackground = "AnimationKeyframeBackground";
		public static GUIStyle timelineTick = "AnimationTimelineTick";
		public static GUIStyle dopeSheetKeyframe = "Dopesheetkeyframe";
		public static GUIStyle dopeSheetBackground = "DopesheetBackground";
		public static GUIStyle popupCurveDropdown = "PopupCurveDropdown";
		public static GUIStyle popupCurveEditorBackground = "PopupCurveEditorBackground";
		public static GUIStyle popupCurveEditorSwatch = "PopupCurveEditorSwatch";
		public static GUIStyle popupCurveSwatchBackground = "PopupCurveSwatchBackground";
		public static GUIStyle separator = new GUIStyle("AnimLeftPaneSeparator");

		public static GUIStyle toolbarBottom = "ToolbarBottom";
		public static GUIStyle optionsButton = new GUIStyle(EditorStyles.toolbarButtonRight);
		public static GUIStyle miniToolbarButton = new GUIStyle(EditorStyles.toolbarButton);
		public static GUIStyle toolbarLabel = new GUIStyle(AnimationWindowStyles.animClipToolbarPopup);

		public static void Initialize() {
			//Debug.Log("point icon width: " + pointIcon.width + ", point icon height: " + pointIcon.height);

			EditorGUIUtility.SetIconSize(new Vector2(12, 12));
			toolbarLabel.normal.background = null;
			optionsButton.padding = new RectOffset();
			optionsButton.imagePosition = ImagePosition.ImageOnly;
			optionsButton.contentOffset = new Vector2(-7, 0);
		}
	}
}
