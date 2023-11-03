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

    /// <summary>
    /// Метод установки координат и загрузки состояния, если 
    /// </summary>
    /// <param name="coords">Координаты чанка</param>
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


    /// <summary>
    /// Метод установки стен в следующем порядке: север, восток, юг, запад
    /// </summary>
    /// <param name="states">Массив булевых значение</param>
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


    /// <summary>
    /// Метода установки случайного состояния потомков контейнера
    /// </summary>
    /// <param name="container">Контейнер</param>
    private void SetRandomActiveChild(Transform container)
    {
        foreach (Transform item in container)
        {
            bool active = UnityEngine.Random.Range(0, 2) == 0;

            item.gameObject.SetActive(active);
        }
    }

    /// <summary>
    /// Метод установки состояния потомков контейнера в соответствии с массивом булевых значений
    /// </summary>
    /// <param name="activeList">Массив булевых значений</param>
    /// <param name="container">Контейнер</param>
    private void SetActiveChild(List<bool> activeList, Transform container)
    {
        for (int i = 0; i < activeList.Count; i++)
        {
            container.GetChild(i).gameObject.SetActive(activeList[i]);
        }
    }

    /// <summary>
    /// Сохранение состояния чанка. 
    /// Сохраняются координаты, состояние игровых объектов в контейнерах декора и препятствий
    /// </summary>
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

    /// <summary>
    /// Загрузка состояния чанка
    /// </summary>
    /// <param name="state">Состояние чанка</param>
    private void LoadState(ChunkState state)
    {
        SetActiveChild(state.DecorStates, DecorContainer);
        SetActiveChild(state.ObstacleStates, ObstacleContainer);
        Coordinates = state.Coords;
    }

    public void OnTriggerEnter(Collider other) => OnPlayerEnter?.Invoke();

    private void OnDisable() => OnPlayerEnter = null;
}
