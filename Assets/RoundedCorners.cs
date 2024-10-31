using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Nobi.UiRoundedCorners {
	[ExecuteInEditMode]                             //Required to check the OnEnable function
	[DisallowMultipleComponent]                     //You can only have one of these in every object.
	[RequireComponent(typeof(RectTransform))]
	public class RoundedCorners : MonoBehaviour {
		private static readonly int Props = Shader.PropertyToID("_WidthHeightRadius");
		private static readonly int prop_OuterUV = Shader.PropertyToID("_OuterUV");

		
		public bool proportional = true;
		
		[HideIf("proportional")]
		private float _radius = 40f;


		public float Radius
		{
			get => proportional ? proportionalRadius * GetComponent<RectTransform>().rect.width : _radius;
			set => _radius = value;
		}
		
		[ShowIf("proportional")]
		[Range(0, 0.5f)]
		public float proportionalRadius = 0.5f;
		
		
		
		private Material material;
		private Vector4 outerUV = new Vector4(0, 0, 1, 1);

		[HideInInspector, SerializeField] private MaskableGraphic image;

		private void OnValidate() {
			Validate();
			Refresh();
		}

		private void OnDestroy() {
			if (image != null) {
				image.material = null;      //This makes so that when the component is removed, the UI material returns to null
			}
			
			if (Application.isPlaying) {
				Object.Destroy(material);
			} else {
				Object.DestroyImmediate(material);
			}
			
			image = null;
			material = null;
		}

		private void OnEnable() {
			//You can only add either ImageWithRoundedCorners or ImageWithIndependentRoundedCorners
			//It will replace the other component when added into the object.
			var other = GetComponent<ImageWithIndependentRoundedCorners>();
			if (other != null) {
				Radius = other.r.x;                 //When it does, transfer the Radius value to this script
				if (Application.isPlaying) {
					Object.Destroy(material);
				} else {
					Object.DestroyImmediate(material);
				}
			}

			Validate();
			Refresh();
		}

		private void OnRectTransformDimensionsChange() {
			if (enabled && material != null) {
				Refresh();
			}
		}

		public void Validate() {
			if (material == null) {
				material = new Material(Shader.Find("UI/RoundedCorners/RoundedCorners"));
			}

			if (image == null) {
				TryGetComponent(out image);
			}

			if (image != null) {
				image.material = material;
			}

			if (image is Image uiImage && uiImage.sprite != null) {
				outerUV = UnityEngine.Sprites.DataUtility.GetOuterUV(uiImage.sprite);
			}
		}

		public void Refresh() {
			var rect = ((RectTransform)transform).rect;

			//Multiply Radius value by 2 to make the Radius value appear consistent with ImageWithIndependentRoundedCorners script.
			//Right now, the ImageWithIndependentRoundedCorners appears to have double the Radius than this.
			material.SetVector(Props, new Vector4(rect.width, rect.height, Radius * 2, 0));
			material.SetVector(prop_OuterUV, outerUV);
			
		}
	}
}