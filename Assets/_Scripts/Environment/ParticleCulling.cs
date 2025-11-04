using UnityEngine;
using UnityEngine.Rendering;

public class ParticleCulling : MonoBehaviour
{
    public Transform player;             // Player để tính khoảng cách
    public float activationDistance = 200f; // Khoảng cách tối đa để active particle

    private ParticleSystem ps;
    private bool isSoundPlaying = false;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= activationDistance)
        {
            if (!ps.isPlaying)
                ps.Play();

            if (!isSoundPlaying && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("Waterfall", transform.position);
                isSoundPlaying = true;
            }
        }
        else
        {
            if (ps.isPlaying)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            if (isSoundPlaying && AudioManager.Instance != null)
            {
                AudioManager.Instance.StopSound("Waterfall");
                isSoundPlaying = false;
            }
        }
    }
}
