using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using System;
using UnityEngine.UI;

namespace PerilsAndPitfalls {

public class Portrait
    {
        public string Name;
        public int PartyIndex;
        public bool IsTalking;
        public GameObject portraitObject;
        public Portrait(string name, int partyIndex, bool isTalking = false)
        {
            Name = name;
            PartyIndex = partyIndex;
            IsTalking = isTalking;
        }

        public IEnumerator SetScale(Vector2 size)
        {
            
            var rectTransform = portraitObject.GetComponent<RectTransform>();
            float animationProgress = 0;
            while (animationProgress < 1)
            {
                var x = Mathf.SmoothStep(rectTransform.sizeDelta.x, size.x, animationProgress);
                rectTransform.sizeDelta = new Vector2(x, x);
                animationProgress += Time.deltaTime / 0.2f;
                yield return null;
            }
        }
    }

public class PortraitView : DialogueViewBase
{

        private string _path;
        public static List<Portrait> Portraits;
        public GameObject PortraitPrefab;
        Vector2 smallPortraitSize = new Vector2(180, 180);
        Vector2 bigPortraitSize = new Vector2(250, 250);

        Vector2 PortraitSize(bool isTalking)
        {
            return isTalking ? bigPortraitSize : smallPortraitSize;
        }

        private void Awake()
        {
            StartCoroutine(SetPath());
            IEnumerator SetPath()
            {
                yield return null;
                //while (GameManager.currentModule == null) yield return null;
                // _path = $"{Application.streamingAssetsPath}/GameData/{GameManager.currentModule}/Party.json";
            }



        }

        private void Start()
        {

            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }

            Portraits = DataManager.DeserializeData<List<Portrait>>(_path);
            foreach(var Portrait in Portraits)
            {
                Portrait.portraitObject = Instantiate(PortraitPrefab, transform);
                Portrait.portraitObject.GetComponent<RectTransform>().sizeDelta = PortraitSize(Portrait.IsTalking);
              //  Portrait.portraitObject.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/{GameManager.currentModule}/Portraits/{Portrait.Name}");
            }
        }


        // Start is called before the first frame update
        public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            StopAllCoroutines();
            foreach(var Portrait in Portraits)
            {
                Portrait.IsTalking = Portrait.Name == dialogueLine.CharacterName;
                StartCoroutine(Portrait.SetScale(PortraitSize(Portrait.IsTalking)));
            }
            onDialogueLineFinished();
        }
    }

}
