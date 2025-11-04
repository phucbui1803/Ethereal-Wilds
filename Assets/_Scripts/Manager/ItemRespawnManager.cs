using System.Collections;
using UnityEngine;

public class ItemRespawnManager : MonoBehaviour
{
    public static ItemRespawnManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Respawn một object sau delay
    /// </summary>
    public void RespawnItem(GameObject item, float delay)
    {
        StartCoroutine(RespawnCoroutine(item, delay));
    }

    private IEnumerator RespawnCoroutine(GameObject item, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (item != null)
        {
            item.SetActive(true);

            // PickupItem component reset
            PickupItem pickup = item.GetComponent<PickupItem>();
            if (pickup != null)
            {
                var field = typeof(PickupItem).GetField("isPicked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null) field.SetValue(pickup, false);
            }
        }
    }
}
