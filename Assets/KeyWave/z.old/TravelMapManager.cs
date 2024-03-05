using Antlr4.Runtime.Misc;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class Landmark
{
    public readonly int Index;
    public readonly string Text;
    public float[] Position;

    Transform landmarkIcon;

    public Landmark(int index, string text, float[] position)
    {
        Index = index;
        Text = text;
        Position = position;
    }

    public void SetupLandmarkIcon(Transform icon)
    {
        string spritePath = $"Sprites/PerilsAndPitfalls/LandmarkIcons/{Index}";
        landmarkIcon = icon;
        icon.transform.localPosition = new Vector2(Position[0], Position[1]);
       // landmarkIcon.GetComponentInChildren<TMPro.TMP_Text>().text = Text;
        landmarkIcon.name = Text;

      //  Texture2D texture = Resources.Load<Texture2D>(spritePath);
       // Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

        landmarkIcon.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>(spritePath);
        Debug.Log(Index);
        //cardObject.transform.localScale = Vector3.zero;
    }

    public void SetDataFromInstance()
    {
        Debug.Log(landmarkIcon.localPosition.x);
       // SiblingIndex = cardObject.GetSiblingIndex();
        Position = new float[] { landmarkIcon.localPosition.x, landmarkIcon.localPosition.y };
    }

    public Transform returnGameObject()
    {
        return landmarkIcon;
    }
}

public class TravelMapManager : UIScreenManager
{

    [SerializeField] Volume volume;
    DepthOfField depthOfField;
    LensDistortion lensDistortion;
    [SerializeField] AnimationCurve depthOfFieldCurve;
    [SerializeField] AnimationCurve lensDistortionCurve;

    Image backgroundImage;
    string _path;


    public static List<Landmark> Landmarks = new List<Landmark>();

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEvent.OnGameSave += SaveOverlayData;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEvent.OnGameSave -= SaveOverlayData;
    }

    // Start is called before the first frame update
    void Start()
    {
        volume.profile.TryGet(out depthOfField);
        volume.profile.TryGet(out lensDistortion);      
    }

    protected override void Awake()
    {
        base.Awake();

        StartCoroutine(SetPath());
        IEnumerator SetPath()
        {
            yield return null;
            //  while (GameManager.currentModule == null) yield return null;
            //   _path = $"{Application.streamingAssetsPath}/GameData/{GameManager.currentModule}/Travelmap.json";
        }
    }

    protected override void SetupBackgroundProperties()
    {
        base.SetupBackgroundProperties();

        backgroundImage = backgroundContainer.GetComponent<Image>();
        backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 0);
    }

    protected override void SetupOverlayElementProperties()
    {
        base.SetupOverlayElementProperties();

        foreach (var landmark in Landmarks)
        {
            landmark.SetupLandmarkIcon(Instantiate(_overlayElementPrefab, overlayElementsContainer.transform).transform);
        }
    }

    protected override void OpenScreen(Transform screen)
    {
        if (screen != transform) return;

        Landmarks = DataManager.DeserializeData<List<Landmark>>(_path);

        if (Landmarks.Count == 0)
        {
            Debug.LogWarning($"No landmarks to load. Are you sure {_path} is populated?");
        }

        base.OpenScreen(screen);
    }

    IEnumerator BackgroundAnimation(float startAlpha, float targetAlpha)
    {
        
        float time = 0f;
        float duration = 2f;

        Color startColor = new Color (backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, startAlpha);
        Color targetColor = new Color (backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, targetAlpha); 

        while (time < duration)
        {

            if (time >= 0.5) backgroundImage.color = Color.Lerp(startColor, targetColor, time - 0.5f);

            time += Time.deltaTime;
            depthOfField.focalLength.value = depthOfFieldCurve.Evaluate(time) * 10;
            
            lensDistortion.intensity.value = -lensDistortionCurve.Evaluate(time);
            yield return null;
        }

        depthOfField.focalLength.value = depthOfFieldCurve.keys[depthOfFieldCurve.length - 1].value * 10;
        lensDistortion.intensity.value = -lensDistortionCurve.keys[lensDistortionCurve.length - 1].value;

    }


    protected override IEnumerator BackgroundAnimationOnOpen()
    {
        yield return StartCoroutine(BackgroundAnimation(0f, 1f));
    }

    protected override IEnumerator BackgroundAnimationOnClose()
    {
        yield return StartCoroutine(BackgroundAnimation(1f, 0f));
    }
    protected override IEnumerator OverlayElementsAnimationOnOpen()
    {
        yield return StartCoroutine(OverlayAnimation(0f, 1f));
    }

    protected override IEnumerator OverlayElementsAnimationOnClose()
    {
        yield return StartCoroutine(OverlayAnimation(1f, 0f));
    }


    IEnumerator OverlayAnimation(float startAlpha, float targetAlpha)
    {
        
        foreach (var landmark in Landmarks)
        {
            float time = 0f;
            float duration = 0.05f;

            var image = landmark.returnGameObject().GetComponentInChildren<Image>();

            Color startColor = new Color(image.color.r, image.color.g, image.color.b, startAlpha);
            Color targetColor = new Color(image.color.r, image.color.g, image.color.b, targetAlpha);


            while (time < duration)
            {
                image.color = Color.Lerp(startColor, targetColor, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
        }
    }



    private void SaveOverlayData()
    {
        if (Landmarks.Count == 0)
        {
            Debug.LogError("No landmarks to save. Aborting save operation.");
            return;
        }

        foreach (var landmark in Landmarks)
        {
            landmark.SetDataFromInstance();
        }

        Debug.Log("saving icons");

        DataManager.SerializeData(Landmarks, _path);
    }


}
