using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateRelativeToCurrentPosition : MonoBehaviour
{

    Vector3 startPosition;
    [SerializeField] string animationName = "Entry";
    Animator animator;

    // Start is called before the first frame update
    void Awake()
    {
        startPosition = transform.localPosition;
        animator = GetComponent<Animator>();
        if (animationName.Length == 0) animationName = "Entry";
        animator.Play(animationName);
      }

    private void LateUpdate()
    {
        transform.localPosition += startPosition;
    }


}
