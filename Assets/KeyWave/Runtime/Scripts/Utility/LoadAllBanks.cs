using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class LoadAllBanks : MonoBehaviour
{
    public List<AssetReference> Banks = new List<AssetReference>();
    public string Scene;

    private static int numberOfCompletedCallbacks;

    void Awake()
    {
        StartCoroutine(LoadBanksAsync());
    }

    private Action Callback = () =>
    {
        numberOfCompletedCallbacks++;
    };

    IEnumerator LoadBanksAsync()
    {

        Banks.ForEach(b => RuntimeManager.LoadBank(b, true, Callback));

        while (numberOfCompletedCallbacks < Banks.Count)
            yield return null;

        while (RuntimeManager.AnySampleDataLoading())
            yield return null;

        AsyncOperation async = SceneManager.LoadSceneAsync(Scene);

        while (!async.isDone)
        {
            yield return null;
        }

        Banks.ForEach(b => b.ReleaseAsset());
    }
}