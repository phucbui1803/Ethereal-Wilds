using UnityEngine;

public class WaterFloat : MonoBehaviour
{
    public float waterLevel = 2f;   // Mức Y mặt nước

    //private void OnTriggerEnter(Collider other)
    //{
    //    var player = other.GetComponent<StarterAssets.ThirdPersonController>();
    //    if (player != null)
    //    {
    //        player.inWater = true;
    //        player.waterLevel = waterLevel;
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<StarterAssets.ThirdPersonController>();
        if (player != null)
        {
            player.inWater = true;
            player.waterLevel = waterLevel;
            StartCoroutine(SinkPlayer(player.transform));
        }
    }

    private System.Collections.IEnumerator SinkPlayer(Transform player)
    {
        float duration = 0.3f; // thời gian chìm
        float sinkAmount = 0.3f;
        Vector3 startPos = player.position;
        Vector3 endPos = new Vector3(startPos.x, startPos.y - sinkAmount, startPos.z);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            player.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        player.position = endPos;
    }
}
