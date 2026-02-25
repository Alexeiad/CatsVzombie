using System.Collections;
using UnityEngine;

public class EnemyRadar : MonoBehaviour
{
    [Header("Radar Settings")]
    [SerializeField] private RenderTexture radarTexture;
    [SerializeField] private Transform playerTransform;  // ← Transform игрока
    [SerializeField] private float radarRadius = 40f;    // радиус обнаружения в юнитах
    [SerializeField] private float updateInterval = 0.1f;

    [Header("Dot Settings")]
    [SerializeField] private int dotSize = 4;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private Color playerColor = Color.green;

    private Texture2D radarCanvas;
    private int textureSize;

    private void Start()
    {
        if (playerTransform == null)
            playerTransform = transform;

        textureSize = radarTexture.width; // 300
        radarCanvas = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);

        StartCoroutine(UpdateRadar());
    }

    private IEnumerator UpdateRadar()
    {
        while (true)
        {
            radarCanvas.SetPixels(new Color[textureSize * textureSize]);

            Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);

            foreach (Enemy enemy in enemies)
                ProjectEnemyDot(enemy.transform.position);

            // Игрок всегда в центре
            int center = (textureSize - 1) / 2;
            DrawDot(center, center, playerColor);

            radarCanvas.Apply();
            Graphics.Blit(radarCanvas, radarTexture);

            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void ProjectEnemyDot(Vector3 enemyWorldPos)
    {
        float relX = enemyWorldPos.x - playerTransform.position.x;
        float relY = enemyWorldPos.y - playerTransform.position.y;

        if (relX * relX + relY * relY > radarRadius * radarRadius) return;

        float half = (textureSize - 1) * 0.5f;

        int pixelX = Mathf.RoundToInt(half + (relX / radarRadius) * half);
        int pixelY = Mathf.RoundToInt(half + (relY / radarRadius) * half);

        DrawDot(pixelX, pixelY, enemyColor);
    }

    private void DrawDot(int centerX, int centerY, Color color)
    {
        for (int x = -dotSize; x <= dotSize; x++)
        {
            for (int y = -dotSize; y <= dotSize; y++)
            {
                if (x * x + y * y <= dotSize * dotSize)
                {
                    int px = centerX + x;
                    int py = centerY + y;

                    if (px >= 0 && px < textureSize && py >= 0 && py < textureSize)
                        radarCanvas.SetPixel(px, py, color);
                }
            }
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        if (radarCanvas != null)
            Destroy(radarCanvas);
    }
}