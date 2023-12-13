using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine;

 
	using System;
	
		using System.Collections;

	
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

        Banks.ForEach(b => FMODUnity.RuntimeManager.LoadBank(b, true, Callback));

        while (numberOfCompletedCallbacks < Banks.Count)
            yield return null;

        while (FMODUnity.RuntimeManager.AnySampleDataLoading())
            yield return null;

        AsyncOperation async = SceneManager.LoadSceneAsync(Scene);

        while (!async.isDone)
        {
            yield return null;
        }

        Banks.ForEach(b => b.ReleaseAsset());
    }
}