using System;
using System.Collections;
using UnityEngine;

public class DoorSwitch : MonoBehaviour
{
    [SerializeField] private Animator m_switch_animator;
    [SerializeField] private GameObject targeted_door;
    
    [SerializeField] private float toggleCooldown = 0.5f;
    private bool m_onCooldown;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.gameObject.name);
        if (m_onCooldown == true) return;
        if (!other.gameObject.CompareTag("Player") || other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        StartCoroutine(ToggleWithCooldown());
    }

    private IEnumerator ToggleWithCooldown()
    {
        m_onCooldown = true;

        // trigger lever anim
        m_switch_animator.SetTrigger("DoorToggle");
            
        // trigger door anim + collider
        targeted_door.GetComponent<Animator>().SetTrigger("DoorToggle");
        targeted_door.GetComponent<BoxCollider2D>().enabled = !targeted_door.GetComponent<BoxCollider2D>().enabled;

        yield return new WaitForSeconds(toggleCooldown);
        m_onCooldown = false;
    }
}
