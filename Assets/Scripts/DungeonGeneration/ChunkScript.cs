using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Класс чанка
/// </summary>
[SelectionBase]
public class ChunkScript : MonoBehaviour
{
    public class ChunkState
    {
        public Vector2Int Coords;
        public List<bool> DecorStates;
        public List<bool> ObstacleStates;
    }

    #region [Chunk config]

    [SerializeField]
    private int size;
    public int Size => size;

    public Vector2Int Coordinates;

    [SerializeField]
    private List<GameObject> Walls = new List<GameObject>();

    [SerializeField]
    private Transform DecorContainer;
    [SerializeField]
    private Transform ObstacleContainer;

    public List<ChunkState> states = new List<ChunkState>();

    public event Action OnPlayerEnter = delegate { }; 

    #endregion

    public void SetCoordinates(Vector2Int coords)
    {
        var state = states.FirstOrDefault(x => x.Coords == coords);

        if (state != null)
        {
            LoadState(state);
        }
        else
        {
            Coordinates = coords;
            SetDecorAndObstacles();
        }
    }

    public void SetWalls(bool[] states)
    {
        for (int i = 0; i < 4; ++i)
        {
            Walls[i].SetActive(!states[i]);
        }
    }

    public void SetDecorAndObstacles()
    {
        SetRandomActiveChild(DecorContainer);
        SetRandomActiveChild(ObstacleContainer);
        SaveState();
    }

    private void SetRandomActiveChild(Transform container)
    {
        foreach (Transform item in container)
        {
            bool active = UnityEngine.Random.Range(0, 2) == 0;

            item.gameObject.SetActive(active);
        }
    }

    private void SetActiveChild(List<bool> activeList, Transform container)
    {
        for (int i = 0; i < activeList.Count; i++)
        {
            container.GetChild(i).gameObject.SetActive(activeList[i]);
        }
    }

    private void SaveState()
    {
        var decor = DecorContainer.transform.Cast<Transform>().Select(t => t.gameObject.activeInHierarchy).ToList();
        var obstacle = ObstacleContainer.transform.Cast<Transform>().Select(t => t.gameObject.activeInHierarchy).ToList();

        var state = new ChunkState
        {
            Coords = Coordinates,
            DecorStates = decor,
            ObstacleStates = obstacle
        };

        states.Add(state);
    }

    private void LoadState(ChunkState state)
    {
        SetActiveChild(state.DecorStates, DecorContainer);
        SetActiveChild(state.ObstacleStates, ObstacleContainer);
        Coordinates = state.Coords;
    }

    public void OnTriggerEnter(Collider other) => OnPlayerEnter?.Invoke();

    private void OnDisable() => OnPlayerEnter = null;
}
