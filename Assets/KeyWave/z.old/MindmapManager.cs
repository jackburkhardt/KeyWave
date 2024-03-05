using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[System.Serializable]
public class Card
{
    public readonly int Index;
    public int SiblingIndex;
    public readonly string Text;
    public float[] Position;

    Transform cardObject;

    public Card(int index, int siblingIndex, string text, float[] position)
    {
        Index = index;
        SiblingIndex = siblingIndex;
        Text = text;
        Position = position;
    }

    public void SetupCardTransform(Transform card)
    {
        cardObject = card;
        cardObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(Position[0], Position[1]);
        cardObject.GetComponentInChildren<TMPro.TMP_Text>().text = Text;
        cardObject.name = $"Item{Index}";
        cardObject.transform.localScale = Vector3.zero;
    }

    public void SetSiblingIndex()
    {
        cardObject.SetSiblingIndex(SiblingIndex);
    }

    public void SetDataFromCardObject()
    {
        SiblingIndex = cardObject.GetSiblingIndex();
        Position = new float[] {cardObject.GetComponent<RectTransform>().anchoredPosition.x, cardObject.GetComponent<RectTransform>().anchoredPosition.y};
    }

    public Transform returnGameObject()
    {
        return cardObject;
    }
}

public class MindmapManager : UIScreenManager
{
    // Start is called before the first frame update

    public static List<Card> Cards = new List<Card>();


    private static string _path;

    protected override void Awake()
    {
        base.Awake();

        StartCoroutine(SetPath());
        IEnumerator SetPath()
        {
            yield return null;
            //  while (GameManager.currentModule == null) yield return null;
            // _path = $"{Application.streamingAssetsPath}/GameData/{GameManager.currentModule}/Mindmap.json";
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEvent.OnGameSave += SaveCardData;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEvent.OnGameSave -= SaveCardData;
    }
 
    protected override void OpenScreen(Transform screen)
    {
        if (screen != transform) return;

        Cards = DataManager.DeserializeData<List<Card>>(_path);

        if (Cards.Count == 0)
        {
            Debug.LogWarning($"No cards to load. Are you sure {_path} is populated?");
        }

    

        base.OpenScreen(screen);      
    }

    protected override void CloseScreen(Transform screen)
    {
        if (screen != transform) return;

        base.CloseScreen(screen);
    }

    protected override void SetupBackgroundProperties()
    {
        base.SetupBackgroundProperties();
        backgroundContainer.GetComponent<RectTransform>().pivot = Vector2.up;
        backgroundContainer.localScale = Vector3.zero;
    }

    protected override void SetupOverlayElementProperties()
    {
        base.SetupOverlayElementProperties();

        foreach (var card in Cards)
        {
            card.SetupCardTransform(Instantiate(_overlayElementPrefab, overlayElementsContainer.transform).transform);
        }

        foreach (var card in Cards)
        {
            card.SetSiblingIndex();
        }
    }

    IEnumerator BackgroundAnimation(Vector3 targetScale)
    {
        LeanTween.scale(backgroundContainer.gameObject, targetScale, 0.2f).setEase(LeanTweenType.easeOutQuint);
        while (backgroundContainer.transform.localScale != targetScale) yield return null;
    }

    protected override IEnumerator BackgroundAnimationOnOpen()
    {
        yield return StartCoroutine(BackgroundAnimation(Vector3.one));
    }

    protected override IEnumerator BackgroundAnimationOnClose()
    {
        yield return StartCoroutine(BackgroundAnimation(Vector3.zero));
    }

    IEnumerator OverlayElementsAnimation(Vector3 targetScale)
    {
        for (int i = 0; i < overlayElementsContainer.transform.childCount; i++)
        {
            var card = overlayElementsContainer.transform.GetChild(i);
            LeanTween.scale(card.gameObject, targetScale, 0.1f).setEase(LeanTweenType.easeOutBack);
            yield return new WaitForSeconds(0.015f);
        }
    }

    protected override IEnumerator OverlayElementsAnimationOnOpen()
    {
        yield return StartCoroutine(OverlayElementsAnimation(Vector3.one));     
    }

    protected override IEnumerator OverlayElementsAnimationOnClose()
    {
        yield return StartCoroutine(OverlayElementsAnimation(Vector3.zero));
    }


    private void SaveCardData()
    {
        Debug.Log(Cards.Count);
        if (Cards.Count == 0)
        {
            Debug.LogError("No cards to save. Aborting save operation.");
            return;
        }

        foreach (var card in Cards)
        {
            card.SetDataFromCardObject();
        }
        DataManager.SerializeData(Cards, _path);
    }
}
