using UnityEngine;

public class AttackFaceFlashSMB : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var face = animator.GetComponentInChildren<ZombieFaceFlashByMaterial>();
        if (face == null) face = animator.GetComponentInParent<ZombieFaceFlashByMaterial>();

        if (face != null)
        {
            face.SetAngryPermanent();
            Debug.Log($"[AttackFaceFlashSMB] Angry set on: {animator.gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"[AttackFaceFlashSMB] ZombieFaceFlashByMaterial NOT found under: {animator.gameObject.name}");
        }
    }
}
