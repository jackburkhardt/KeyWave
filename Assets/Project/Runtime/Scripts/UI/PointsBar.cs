using System.Collections.Generic;
using System.Linq;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.UI
{
    public class PointsBar : MonoBehaviour
    {
        [SerializeField] private HorizontalOrVerticalLayoutGroup layoutGroup;
        [SerializeField] private RectTransform pointsBarUnitTemplate;

        private List<RectTransform> PointsBarUnits => layoutGroup.GetComponentsInChildren<RectTransform>().Where(p => p.transform != layoutGroup.transform && p.gameObject.activeSelf).ToList();
        private float UnitMinWidth =>  layoutGroup.GetComponent<RectTransform>().rect.width / Points.TotalMaxScore;
        private float LayoutGroupWidth => layoutGroup.GetComponent<RectTransform>().rect.width;

        private float TotalUnitWidth
        {
            get
            {
                var currentWidth = 0f;
                foreach (var barUnit in PointsBarUnits)
                {
                    currentWidth += barUnit.rect.width;
                }
                return currentWidth;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            pointsBarUnitTemplate.gameObject.SetActive(false);
        }

        public void OnParticleDeath(ParticleSystem.Particle particle)
        {
            AddOrExpandUnit(particle.startColor);
        }

        void AddOrExpandUnit(Color color)
        {
            var unit = PointsBarUnits.FirstOrDefault(p => p.GetComponent<Image>().color == color);
        
            if (unit == null)
            {
                unit = Instantiate(pointsBarUnitTemplate, layoutGroup.transform);
                unit.gameObject.SetActive(true);
                unit.gameObject.name = color.ToString();
                unit.GetComponent<Image>().color = color;
                unit.GetComponent<RectTransform>().sizeDelta = new Vector2(0, unit.GetComponent<RectTransform>().rect.height);
            }
            
            var unitRectTransform = unit.GetComponent<RectTransform>();
            var rect = unitRectTransform.rect;
            unitRectTransform.sizeDelta = new Vector2(rect.width + UnitMinWidth,rect.height);

            if (TotalUnitWidth > LayoutGroupWidth)
            {
                foreach (var barUnit in PointsBarUnits)
                {
                    barUnit.sizeDelta = new Vector2(barUnit.rect.width * (LayoutGroupWidth) / (TotalUnitWidth), barUnit.rect.height);
                }
            }
        }

        public void ApplySaveData()
        {
            foreach (var barUnit in PointsBarUnits)
            {
                Destroy(barUnit.gameObject);
            }
            for (var i = 0; i < Points.WellnessScore; i++)
            {
                AddOrExpandUnit(Points.Color(Points.Type.Credibility));
            }
        
            for (var i = 0; i < Points.CredibilityScore; i++)
            {
                AddOrExpandUnit(Points.Color(Points.Type.Engagement));
            }
        
            for (var i = 0; i < Points.EngagementScore; i++)
            {
                AddOrExpandUnit(Points.Color(Points.Type.Commitment));
            }
        }
    }
}