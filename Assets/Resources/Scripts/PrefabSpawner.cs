using UnityEngine;
using System.Collections.Generic;
using TMPro;
using YG;
using System.Linq;
using Unity.VisualScripting;

public class PrefabSpawner : MonoBehaviour
{
    public static PrefabSpawner instance;
    public GameObject prefab;
    public Transform[] targetParents;
    public const int GRID_SIZE = 64;
    private int maxCurrentNumber = 128;
    private List<int> spawnNumbers = new List<int>() { 2, 4, 8, 16 };
    private List<int> randomNumbers = new List<int>() { 2, 4, 8, 16 };

    private void Awake()
    {
        if (instance == null) instance = this;
    }
    void Start()
    {
        StartFillGridGame();
    }
    private void OnEnable()
    {
        YG2.onGetSDKData += GetData;
    }

    // Отписываемся от ивента onGetSDKData
    private void OnDisable()
    {
        YG2.onGetSDKData -= GetData;
    }

    public void SetData()
    {
        List<int> numberOrder = new List<int>();
        foreach (Transform parent in targetParents)
        {
            Block block = parent.GetComponentInChildren<Block>();
            numberOrder.Add(block.GetNumber());
        }

        YG2.saves.blockNumberOrder = numberOrder.ToArray();
        YG2.SaveProgress();
    }
    public void OnApplicatioQuit()
    {
        SetData();
        Debug.Log("save");
    }

    public void GetData()
    {

    }

    public void Save()
    {
        YG2.SaveProgress();
    }

    public void StartFillGridGame()
    {
        for (int i = 0; i < targetParents.Length; i++)
        {
            int number = GetStartNumberAtIndex(i);
            GameObject newPrefab = Instantiate(prefab, targetParents[i]);
            Block block = newPrefab.GetComponent<Block>();
            block.Init();
            block.SetNumber(number);
        }
    }
    public int GetStartNumberAtIndex(int index)
    {
        Debug.Log($"Spawn pool обновлён: {string.Join(", ", YG2.saves.blockNumberOrder)}");
        if (YG2.saves.blockNumberOrder != null)
        {
            if (YG2.saves.blockNumberOrder.Length > index && YG2.saves.blockNumberOrder[index] != 0)
            {
                Debug.Log("save");
                return YG2.saves.blockNumberOrder[index];
            }

        }
        int rand = randomNumbers[Random.Range(0, randomNumbers.Count)];
        
        return rand;
    }

    public int GetRandomNumber()
    {
        foreach (var spawnNumber in spawnNumbers)
        {
            Debug.Log(spawnNumber);
        }
        return spawnNumbers[Random.Range(0, spawnNumbers.Count)];
    }
    public void UpdateSpawnPool(int newBlockNumber)
    {
        if (newBlockNumber < maxCurrentNumber)
        {
            return;
        }
        maxCurrentNumber *= 2;
        int minValue = FindMinNumberOnField();
        if (minValue == -1) return;

        RemoveBlocksWithNumber(minValue);

        if (spawnNumbers.Contains(minValue))
            spawnNumbers.Remove(minValue);

        if (!spawnNumbers.Contains(newBlockNumber / 4))
            spawnNumbers.Add(newBlockNumber / 4);

        //Debug.Log($"Spawn pool обновлён: {string.Join(", ", spawnNumbers)}");
    }

    private int FindMinNumberOnField()
    {
        Block[] blocks = FindObjectsOfType<Block>();
        int min = int.MaxValue;

        foreach (var block in blocks)
        {
            int val = block.GetNumber();
            if (val > 0 && val < min)
                min = val;
        }

        return min == int.MaxValue ? -1 : min;
    }

    private void RemoveBlocksWithNumber(int number)
    {
        Block[] blocks = FindObjectsOfType<Block>();

        foreach (var block in blocks)
        {
            if (block.GetNumber() == number)
            {
                Destroy(block.gameObject);
            }
        }
    }
}
