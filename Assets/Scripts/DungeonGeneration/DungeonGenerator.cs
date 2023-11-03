using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Класс генерации карты
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    #region [Main]

    [Header("Main config")]
    [SerializeField]
    private GameObject Player;

    [SerializeField]
    private int BaseMapSize;

    [SerializeField]
    private int DrawDistance;

    [SerializeField]
    private ChunkScript chunkPrefab;

    #endregion

    #region [Perlin noise and generation]

    [Header("Perlin noise and generation")]
    [SerializeField]
    [Range(0.01f, 0.99f)]
    private float noiseFixer;

    [SerializeField]
    private int xOffset, yOffset;

    [SerializeField]
    [Range (-1, 1)]
    private int TESTMODX, TESTMODY;

    #endregion

    #region [Chunks placement]

    [Header ("Chunks placement")]
    [SerializeField]
    [Range(0, 1)]
    private float ChunkPlacementValue = 0.35f;

    [SerializeField]
    private List<ChunkScript> PlacedChunks = new List<ChunkScript>();
    [SerializeField]
    private List<ChunkScript> UnusedChunks = new List<ChunkScript>(); 

    #endregion

    private void Start() => Generate();

    [ContextMenu("Generate")]
    private void Generate()
    {
        Random.InitState(xOffset + yOffset + 1);

        xOffset = Random.Range(-1000000, 1000000);
        yOffset = Random.Range(-1000000, 1000000);

        ClearDungeon();
        InstantiateChunks();
        UpdateChunks(UnusedChunks.FirstOrDefault());
        SetChunks();
        SetPlayerPosition();
    }

    /// <summary>
    /// Метод создания начального пула чанков
    /// </summary>
    private void InstantiateChunks()
    {
        for (int i = 0; i < BaseMapSize * BaseMapSize; i++)
        {
            var chunk = Instantiate(chunkPrefab, transform);
            UnusedChunks.Add(chunk);
            chunk.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Метод для очищения и удаления элементов списков установленных и неиспользуемых чанков
    /// </summary>
    [ContextMenu("Clear")]
    private void ClearDungeon()
    {
        ClearChunks(PlacedChunks);
        ClearChunks(UnusedChunks);
    }

    /// <summary>
    /// Метод для очищения и удаления элементов списка чанков
    /// </summary>
    /// <param name="chunks">Список чанков</param>
    private void ClearChunks(List<ChunkScript> chunks)
    {
        if (chunks.Count > 0)
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                if (chunks[i] != null)
                {
                    DestroyImmediate(chunks[i].gameObject);
                }
            }

            chunks.Clear();
        }
    }


    /// <summary>
    /// Метод обновления чанков
    /// </summary>
    /// <param name="chunk">Центральный чанк, вокруг которого происходит обновление</param>
    private void UpdateChunks(ChunkScript chunk)
    {
        var list = new List<ChunkScript>();

        for (int y = -DrawDistance; y < DrawDistance + 1; y++)
        {
            for (int x = -DrawDistance; x < DrawDistance + 1; x++)
            {
                int neighborY = chunk.Coordinates.y + y;
                int neighborX = chunk.Coordinates.x + x;

                Vector2Int coords = new Vector2Int(neighborX, neighborY);

                float noise = Mathf.PerlinNoise(neighborX + xOffset + noiseFixer, neighborY + yOffset + noiseFixer);
                var newChunk = PlacedChunks.FirstOrDefault(x => x.Coordinates == coords);

                if (noise >= ChunkPlacementValue && newChunk == null)
                {
                    newChunk = UnusedChunks.FirstOrDefault(x => x.Coordinates == coords);
                    newChunk = newChunk != null ? newChunk : UnusedChunks.FirstOrDefault();

                    PlaceChunk(neighborX, neighborY, newChunk);
                }

                list.Add(newChunk);
            }
        }

        foreach (var item in PlacedChunks.Except(list).ToList())
        {
            item.gameObject.SetActive(false);

            PlacedChunks.Remove(item);
            UnusedChunks.Add(item);
        }

        SetChunks();
    }

    /// <summary>
    /// Метод установки чанка
    /// </summary>
    /// <param name="x">Положение по X оси</param>
    /// <param name="y">Положение по Y оси</param>
    /// <param name="chunk">Чанк из пула</param>
    private void PlaceChunk(int x, int y, ChunkScript chunk)
    {
        chunk.gameObject.SetActive(true);

        chunk.transform.position = new Vector3(x * chunk.Size * TESTMODX, 0, y * chunk.Size * TESTMODY);

        chunk.name = $"Chunk {x} {y}";
        chunk.SetCoordinates(new Vector2Int(x, y));

        chunk.OnPlayerEnter += () => UpdateChunks(chunk);

        PlacedChunks.Add(chunk);
        UnusedChunks.Remove(chunk);
    }

    private void SetChunks()
    {
        foreach (var chunk in PlacedChunks)
        {
            var neighbors = GetNeighbors(chunk.Coordinates);
            chunk.SetWalls(neighbors);
        }
    }

    /// <summary>
    /// Метод поиска соседних чанков
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns>Массив булевых значений, обозначающих наличие или отсутствие соседа</returns>
    private bool[] GetNeighbors(Vector2Int coordinates)
    {
        List<bool> neighbors = new List<bool>();

        int[] IndexArray = new int[4] { 0, 1, 0, -1 };

        for (int i = 0; i < 4; i++)
        {
            bool result = false;

            int x = IndexArray[i];
            int y = IndexArray[IndexArray.Length - 1 - i];

            int neighborX = coordinates.x + x;
            int neighborY = coordinates.y + y;

            float noise = Mathf.PerlinNoise(neighborX + xOffset + noiseFixer, neighborY + yOffset + noiseFixer);

            if (noise >= ChunkPlacementValue)
            {
                result = true;
            }

            neighbors.Add(result);
        }

        return neighbors.ToArray();
    }

    /// <summary>
    /// Установка позиции игрока после генерации
    /// </summary>
    private void SetPlayerPosition()
    {
        Player.transform.position = PlacedChunks[0].transform.position;
    }
}