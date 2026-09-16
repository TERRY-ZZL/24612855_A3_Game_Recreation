using UnityEngine;

public class PlayerShowcasePreview : MonoBehaviour
{
    private Animator playerAnimator;

    void Start()
    {
        // 开启自动展示
        playerAnimator = GetComponent<Animator>();
        playerAnimator.SetBool("PreviewMode", true);
    }
}