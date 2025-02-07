using DG.Tweening;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

public class DayProgressBar : MonoBehaviour
{
    public enum AdjustMode
    {
        ScaleXToProgressPercent,
    }
    
    public AdjustMode adjustMode;
    
    public RectTransform progressBar;
    
    [Range(0, 1)]
    [Tooltip("Only used in editor mode. Gets overriden by Clock.DayProgress in play mode.")]
    public float editorModeProgressValue = 0.5f;


    private void Start()
    {
        Adjust();
        GetComponent<CanvasGroup>().alpha = 0;
        
    }

    private void OnValidate()
    {
        Adjust();
    }
    
    float _progress;

    private void Update()
    {
        var progress = Clock.DayProgress;
        if (progress != _progress)
        {
            Adjust();
            _progress = progress;
        }
    }

    private void Adjust()
    {
        if (progressBar == null) return;
        
        if (!Application.isPlaying && Application.isEditor)
        {
            AdjustImmediate();
        }
        
        else switch (adjustMode)
        {
            case AdjustMode.ScaleXToProgressPercent:
                DOTween.To(() => progressBar.localScale, x => progressBar.localScale = x, new Vector3(Clock.DayProgress, progressBar.localScale.y, progressBar.localScale.z), 1f);
                break;
        }
       
    }

    private void AdjustImmediate()
    {
        switch (adjustMode)
        {
            case AdjustMode.ScaleXToProgressPercent:
                progressBar.localScale = new Vector3(!Application.isPlaying && Application.isEditor ? editorModeProgressValue : Clock.DayProgress, progressBar.localScale.y, progressBar.localScale.z);
                break;
        }
    }
    
    public void Show()
    {
        GetComponent<CanvasGroup>().DOFade(1, 1f);
    }
    
    public void Hide()
    {
        GetComponent<CanvasGroup>().DOFade(0, 1f);
    }
}
