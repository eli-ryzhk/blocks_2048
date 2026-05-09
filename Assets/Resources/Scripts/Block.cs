using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class Block : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int columns = 8;
    public GameObject highlightFrame;
    public Image mainIcon;
    public TextMeshProUGUI numberText;
    private int counter;
    private int number = 2;
    private bool isMouseClickDown;
    private Vector3 lastMousePosition;
    private List<Block> highlightedBlocks = new List<Block>();
    private Block currentHoveredBlock; 
    private Block pressedBlock;


    void Awake()
    {
        if (highlightFrame != null)
            highlightFrame.SetActive(false);
    }
    public void Init()
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isMouseClickDown = true;
        pressedBlock = this;
        lastMousePosition = Input.mousePosition;

        Transform gridParent = GetGridParent();
        if (gridParent == null) return;

        List<Transform> allCells = new List<Transform>();
        foreach (Transform child in gridParent) allCells.Add(child);

        int startIndex = allCells.IndexOf(transform.parent ? transform.parent : transform);
        if (startIndex == -1) startIndex = allCells.IndexOf(transform);
        if (startIndex == -1) return;

        int number = GetNumber();
        bool hasNeighbor = HasSameNeighbor(startIndex, allCells, number);

        DisableAllHighlights();
        if (hasNeighbor)
        {
            HashSet<int> visited = new HashSet<int>();
            HighlightWithNeighbors(startIndex, allCells, visited, number);
            currentHoveredBlock = this;
        }
        else
        {
            currentHoveredBlock = null;
        }
    }

    void Update()
    {
        if (!isMouseClickDown) return;

        Vector3 deltaPos = lastMousePosition - Input.mousePosition;
        if (deltaPos.magnitude > 0.5f)
            MouseMove();

        lastMousePosition = Input.mousePosition;
    }

    private void MouseMove()
    {
        if (!isMouseClickDown) return;

        Block hoveredBlock = GetBlockUnderMouse();
        if (hoveredBlock == null)
        {
            DisableAllHighlights();
            currentHoveredBlock = null;
            return;
        }

        Transform gridParent = hoveredBlock.GetGridParent();
        if (gridParent == null) return;

        List<Transform> allCells = new List<Transform>();
        foreach (Transform child in gridParent) allCells.Add(child);

        int hoveredIndex = allCells.IndexOf(hoveredBlock.transform.parent ? hoveredBlock.transform.parent : hoveredBlock.transform);
        if (hoveredIndex == -1) return;

        int number = hoveredBlock.GetNumber();
        bool hasNeighbor = HasSameNeighbor(hoveredIndex, allCells, number);

        if (hoveredBlock != currentHoveredBlock)
        {
            DisableAllHighlights();
        }

        if (!hasNeighbor)
        {
            currentHoveredBlock = null;
            return;
        }

        DisableAllHighlights();
        HashSet<int> visited = new HashSet<int>();
        HighlightWithNeighbors(hoveredIndex, allCells, visited, number);
        currentHoveredBlock = hoveredBlock;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isMouseClickDown = false;

        Block target = currentHoveredBlock != null ? currentHoveredBlock : pressedBlock;
        pressedBlock = null;
        currentHoveredBlock = null;

        DisableAllHighlights();

        if (target == null) return;

        Transform gridParent = target.GetGridParent();
        if (gridParent == null) return;

        List<Transform> allCells = new List<Transform>();
        foreach (Transform child in gridParent) allCells.Add(child);

        int startIndex = allCells.IndexOf(target.transform.parent ? target.transform.parent : target.transform);
        if (startIndex == -1) startIndex = allCells.IndexOf(target.transform);
        if (startIndex == -1) return;

        int startNumber = target.GetNumber();

        HashSet<int> visited = new HashSet<int>();
        counter = 0;
        DestroyWithNeighbors(startIndex, allCells, visited, startNumber);

        if (counter != 1)
        {
            startNumber *= 2;
            ScoreManager.instance.AddScore(startNumber);
        }

        CreateNumberAtIndex(startIndex, allCells, startNumber);

        PrefabSpawner.instance.UpdateSpawnPool(startNumber);

        StartCoroutine(ApplyGravity(allCells));
    }
    private Block GetBlockUnderMouse()
    {
        if (EventSystem.current == null) return null;

        var ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        foreach (var r in results)
        {
            var b = r.gameObject.GetComponentInParent<Block>();
            if (b != null) return b;
        }
        return null;
    }

    private void HighlightWithNeighbors(int index, List<Transform> allCells, HashSet<int> visited, int targetNumber)
    {
        if (index < 0 || index >= allCells.Count || visited.Contains(index)) return;

        Transform cell = allCells[index];
        Block script = cell.GetComponentInChildren<Block>();
        if (script == null || script.GetNumber() != targetNumber) return;

        visited.Add(index);

        if (script.highlightFrame != null)
            script.highlightFrame.SetActive(true);

        highlightedBlocks.Add(script);

        int[] offsets = { -columns - 1, -columns, -columns + 1, -1, 1, columns - 1, columns, columns + 1 };
        foreach (int offset in offsets)
        {
            int neighborIndex = index + offset;
            if (neighborIndex >= 0 && neighborIndex < allCells.Count &&
                IsNeighborValid(index, neighborIndex, columns))
            {
                HighlightWithNeighbors(neighborIndex, allCells, visited, targetNumber);
            }
        }
    }

    private void DisableAllHighlights()
    {
        for (int i = 0; i < highlightedBlocks.Count; i++)
        {
            var block = highlightedBlocks[i];
            if (block != null && block.highlightFrame != null)
                block.highlightFrame.SetActive(false);
        }
        highlightedBlocks.Clear();
    }

    private void DestroyWithNeighbors(int index, List<Transform> allCells, HashSet<int> visited, int targetNumber)
    {
        if (index < 0 || index >= allCells.Count || visited.Contains(index)) return;

        Transform cell = allCells[index];
        Block script = cell.GetComponentInChildren<Block>();
        if (script == null || script.GetNumber() != targetNumber) return;

        visited.Add(index);

        var children = new List<Transform>();
        foreach (Transform ch in cell) children.Add(ch);

        foreach (Transform child in children)
        {
            child.SetParent(null);
            counter++;
            StartCoroutine(AnimateDestroy(child.gameObject));
        }

        int[] offsets = { -columns - 1, -columns, -columns + 1, -1, 1, columns - 1, columns, columns + 1 };
        foreach (int offset in offsets)
        {
            int neighborIndex = index + offset;
            if (neighborIndex >= 0 && neighborIndex < allCells.Count &&
                IsNeighborValid(index, neighborIndex, columns))
            {
                DestroyWithNeighbors(neighborIndex, allCells, visited, targetNumber);
            }
        }
    }

    private IEnumerator ApplyGravity(List<Transform> allCells)
    {
        yield return new WaitForEndOfFrame();

        for (int col = 0; col < columns; col++)
        {
            List<Transform> columnCells = new List<Transform>();
            for (int row = 0; row < allCells.Count / columns; row++)
                columnCells.Add(allCells[row * columns + col]);

            for (int row = columnCells.Count - 1; row >= 0; row--)
            {
                if (columnCells[row].childCount == 0)
                {
                    for (int above = row - 1; above >= 0; above--)
                    {
                        if (columnCells[above].childCount > 0)
                        {
                            Transform block = columnCells[above].GetChild(0);
                            block.SetParent(columnCells[row]);
                            StartCoroutine(AnimateFall(block.gameObject, columnCells[row].position));
                            break;
                        }
                    }
                }
            }

            for (int row = 0; row < columnCells.Count; row++)
            {
                if (columnCells[row].childCount == 0)
                {
                    GameObject newObj = Instantiate(PrefabSpawner.instance.prefab, columnCells[row]);
                    newObj.transform.localScale = Vector3.one;
                    int num = PrefabSpawner.instance.GetRandomNumber();

                    Block block = newObj.GetComponent<Block>();
                    block.Init();
                    block.SetNumber(num);
                    StartCoroutine(AnimateFall(newObj, columnCells[row].position));
                }
            }
        }

        yield return new WaitForSeconds(0.2f);

        GameOverChecker checker = FindObjectOfType<GameOverChecker>();
        if (checker != null)
            checker.CheckGameOver();
    }

    private void CreateNumberAtIndex(int index, List<Transform> allCells, int number)
    {
        Transform cell = allCells[index];
        GameObject newObj = Instantiate(PrefabSpawner.instance.prefab, cell);
        Block block = newObj.GetComponent<Block>();
        newObj.transform.localScale = Vector3.one;
        block.SetNumber(number);

        newObj.GetComponent<Block>().Init();
        StartCoroutine(AnimateSpawn(newObj));
    }
    public int GetNumber() => number;
    public void SetNumber(int number)
    {
        this.number = number;
        numberText.text = ScoreManager.ShortenNumber(number);
        mainIcon.color = ColorManager.instance.GetColorAtIndex(GetNumber());
    }

    private bool HasSameNeighbor(int index, List<Transform> allCells, int targetNumber)
    {
        if (index < 0 || index >= allCells.Count) return false;

        int[] offsets = { -columns - 1, -columns, -columns + 1, -1, 1, columns - 1, columns, columns + 1 };
        foreach (int offset in offsets)
        {
            int neighborIndex = index + offset;
            if (neighborIndex < 0 || neighborIndex >= allCells.Count) continue;
            if (!IsNeighborValid(index, neighborIndex, columns)) continue;

            Transform neighborCell = allCells[neighborIndex];
            if (neighborCell == null) continue;

            Block neighborBlock = neighborCell.GetComponentInChildren<Block>();
            if (neighborBlock != null)
            {
                if (neighborBlock.GetNumber() == targetNumber)
                    return true;
            }
            else
            {
                int val = neighborCell.GetComponent<Block>().GetNumber();
                if (val == targetNumber)
                    return true;
            }
        }
        return false;
    }

    private bool IsNeighborValid(int myIndex, int neighborIndex, int columns)
    {
        int myRow = myIndex / columns;
        int neighborRow = neighborIndex / columns;
        int myCol = myIndex % columns;
        int neighborCol = neighborIndex % columns;
        return Mathf.Abs(myRow - neighborRow) <= 1 && Mathf.Abs(myCol - neighborCol) <= 1;
    }

    private Transform GetGridParent()
    {
        Transform t = transform;
        while (t != null)
        {
            if (t.GetComponent<GridLayoutGroup>() != null)
                return t;
            t = t.parent;
        }
        return null;
    }

    private IEnumerator AnimateSpawn(GameObject obj)
    {
        AudioManager.instance.PlaySpawnSound();
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect == null) yield break;

        Vector3 originalScale = Vector3.one;
        rect.localScale = Vector3.zero;
        float time = 0.15f;
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            rect.localScale = Vector3.Lerp(Vector3.zero, originalScale, t / time);
            yield return null;
        }
        rect.localScale = originalScale;
    }

    private IEnumerator AnimateDestroy(GameObject obj)
    {
        AudioManager.instance.PlayDestroySound();
        Vector3 originalScale = obj.transform.localScale;
        float duration = 0.2f;
        Vector3 targetScale = Vector3.zero;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            obj.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }
        Destroy(obj);
    }

    private IEnumerator AnimateFall(GameObject obj, Vector3 targetPos)
    {
        Vector3 startPos = obj.transform.position;
        float duration = 0.2f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            obj.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }
        obj.transform.position = targetPos;
    }
}
