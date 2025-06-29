using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Item
{
    public int id; // Ψһ��ʶ
    public string name; // ��Ʒ����
    public int cost; // ��Ʒ�ɱ�
    public int sellPrice; // ��Ʒ�ۼ�
    public GameObject prefab; // ��ƷPrefab
    public Sprite itemImage; // ��ƷͼƬ
}
public class ShopSystem_7 : MonoBehaviour
{
    public TextMeshProUGUI moneyText; // ��ʾ����ʽ��TextMeshProUGUI
    public TextMeshProUGUI cartText; // ��ʾ���ﳵ���ݵ�TextMeshProUGUI
    public TextMeshProUGUI workerCountText; // ��ʾ�ѹ�����������TextMeshProUGUI

    public Button[] buyButtons; // ������Ʒ��ť����
    public Button buyWorkerButton; // �����˵İ�ť
    public Button[] removeButtons; // �Ƴ���Ʒ��ť����
    public Button removeWorkerButton; // �Ƴ����˰�ť

    public GameObject workerPrefab; // ����Prefab
    public Transform workerParent; // ���˸�����������֯���˶���
    public BoxCollider2D workerAreaCollider; // ���������BoxCollider2D
    public Transform workerParent2; // �ڶ������˸�����
    public BoxCollider2D workerAreaCollider2; // �ڶ������������BoxCollider2D

    public Transform itemParent; // ��Ʒ������������֯��Ʒ����
    public BoxCollider2D itemAreaCollider; // ��Ʒ�����BoxCollider2D
    public BoxCollider2D itemAreaCollider2; // �ڶ�����Ʒ�����BoxCollider2D
    public Transform itemParent2; // �ڶ�����Ʒ������������֯��Ʒ����

    public TextMeshProUGUI[] itemTexts; // ������ʾ��Ʒ���ƺͼ۸���ı��������

    public int playerMoney = 1000; // ��ҳ�ʼ�ʽ�
    private int workerPrice = 70; // �����˵ļ۸�
    private int workerCount = 0; // �ѹ����˵�����

    private Dictionary<int, int> cartItems = new Dictionary<int, int>(); // ���ﳵ�е���ƷID��������
    private int cartWorkerCount = 0; // ���ﳵ�еĹ�������
    public int TotalSpent { get; private set; } // �ܻ��ѣ�����ֻ��

    // ����������Ʒ

    public List<Item> allItems = new List<Item>
    {
        new Item { id = 0, name = "����", cost = 10, sellPrice = 80, prefab = null },
        new Item { id = 1, name = "ƻ��", cost = 20, sellPrice = 80, prefab = null },
        new Item { id = 2, name = "��", cost = 10, sellPrice = 60, prefab = null },
        new Item { id = 3, name = "���ӱ�", cost = 10, sellPrice = 30, prefab = null },
        new Item { id = 4, name = "�㽶", cost = 40, sellPrice = 80, prefab = null },
        new Item { id = 5, name = "�ײ�", cost = 20, sellPrice = 60, prefab = null },
        new Item { id = 6, name = "����", cost = 40, sellPrice = 140, prefab = null },
        new Item { id = 7, name = "�۱�", cost = 5, sellPrice = 5, prefab = null },
        new Item { id = 8, name = "����ѧ�鱾", cost = 20, sellPrice = 140, prefab = null },
        new Item { id = 9, name = "�ֱ�", cost = 80, sellPrice = 140, prefab = null },
        new Item { id = 10, name = "���", cost = 80, sellPrice = 0, prefab = null },
        new Item { id = 11, name = "�㲨", cost = 40, sellPrice = 140, prefab = null },
        new Item { id = 12, name = "������", cost = 100, sellPrice = 30, prefab = null },
        new Item { id = 13, name = "ɳ��", cost = 80, sellPrice = 30, prefab = null },
        new Item { id = 14, name = "��˾", cost = 40, sellPrice = 80, prefab = null },
        new Item { id = 15, name = "�ɿ���", cost = 80, sellPrice = 140, prefab = null },
        new Item { id = 16, name = "����ʬ��", cost = 20, sellPrice = 140, prefab = null },
        new Item { id = 17, name = "����Ʒ��ʪˮ", cost = 100, sellPrice = 140, prefab = null },
        new Item { id = 18, name = "��������", cost = 100, sellPrice = 140, prefab = null },
        new Item { id = 19, name = "�人��������ģ��", cost = 20, sellPrice = 140, prefab = null },
        new Item { id = 20, name = "5G�ֻ�", cost = 40, sellPrice = 140, prefab = null },
        new Item { id = 21, name = "���", cost = 40, sellPrice = 80, prefab = null },
        new Item { id = 22, name = "�ɿ���", cost = 100, sellPrice = 0, prefab = null },
        new Item { id = 23, name = "�Ͼ�������", cost = 60, sellPrice = 140, prefab = null },

    };
    // ����ʱ���ѡ����Ʒ
    private Dictionary<string, List<Item>> itemsByTimePeriod;



    // ��ǰ��ʾ����Ʒ
    private List<Item> currentItems = new List<Item>();

    // ���ڼ�¼ÿ����ƷPrefab������
    private Dictionary<int, List<GameObject>> itemPrefabsInScene = new Dictionary<int, List<GameObject>>();

    
    // �����豸��ť���ı��������
    public Button[] deviceButtons; // �豸����ť����
    public TextMeshProUGUI[] deviceTexts; // �豸��ť�ϵ��ı�����

    // �����豸�۸�
    private const int devicePriceLevel1 = 300; // ��һ�ι���۸�
    private const int devicePriceLevel2 = 600; // �ڶ��ι���۸�
    private const int devicePriceLevel3 = 900; // �����ι���۸�

    // �豸�ȼ�
    private int deviceLevel = 0; // �豸�ȼ������ڿ��ƹ���


    // ���һ����־���������ڼ�¼�Ƿ񳬳����˿ɼӹ�����
    public bool IsCartExceedWorkerCapacity => cartItems.Values.Sum() > workerCount * 3;
    public List<string> discardedItemsInfo = new List<string>(); // ���ڴ洢����������Ʒ��Ϣ
    void Start()
    {
        // ��ʼ��ʱ�����Ʒ�ֵ�
        InitializeItemsByTimePeriod();

        // ���ݵ�ǰʱ��ѡ����Ʒ
        int currentYear = 1985; // �����ʼ����� 1985
        int currentMonth = 1;
        SelectItemsByTime(currentYear, currentMonth);

        UpdateUI();


        // Ϊÿ����Ʒ��ť��ӵ���¼�������ͼƬ
        for (int i = 0; i < buyButtons.Length; i++)
        {
            int index = i; // ���浱ǰ��ť������
            buyButtons[i].onClick.AddListener(() => AddToCart(index));

            // ���ð�ťͼƬ
            if (i < currentItems.Count && currentItems[i].itemImage != null)
            {
                buyButtons[i].GetComponent<Image>().sprite = currentItems[i].itemImage;
            }
            else
            {
                buyButtons[i].GetComponent<Image>().sprite = null; // ���û��ͼƬ�����
            }
        }

        // Ϊ�����˰�ť��ӵ���¼�
        buyWorkerButton.onClick.AddListener(AddWorker);

        // Ϊÿ���Ƴ���Ʒ��ť��ӵ���¼�
        for (int i = 0; i < removeButtons.Length; i++)
        {
            int index = i; // ���浱ǰ��ť������
            removeButtons[i].onClick.AddListener(() => RemoveFromCart(index));
        }

        // Ϊ�Ƴ����˰�ť��ӵ���¼�
        removeWorkerButton.onClick.AddListener(RemoveWorker);

        
        // ��ʼ���豸��ť
        for (int i = 0; i < deviceButtons.Length; i++)
        {
            int level = i + 1; // �豸�ȼ���1��ʼ
            deviceButtons[i].onClick.AddListener(() => BuyDevice(level));
            UpdateDeviceButton(i);
        }
    }

    private void UpdateUI()
    {
        moneyText.text = $"Money: {playerMoney}";
        workerCountText.text = $"WorkerCount: {workerCount}";

        // ���¹��ﳵ����
        cartText.text = "CartList:\n";
        foreach (var item in cartItems)
        {
            Item itemInAllItems = allItems.Find(i => i.id == item.Key);
            if (itemInAllItems != null)
            {
                cartText.text += $"{itemInAllItems.name}: {item.Value}\n";
            }
        }
        if (cartWorkerCount != 0)
        {
            cartText.text += $"Worker: {cartWorkerCount}\n";
        }

        //UpdateDeviceButton();
        // �����豸��ť
        for (int i = 0; i < deviceButtons.Length; i++)
        {
            UpdateDeviceButton(i);
        }

        //// ��ʾ���������Ʒ��ʾ
        //for (int i = 0; i < deviceLevel; i++)
        //{
        //    ShowLowestProfitItems(i + 1);
        //}
    }

    private void AddToCart(int index)
    {
        if (index >= currentItems.Count)
        {
            Debug.LogError("��Ʒ����������Χ��");
            return;
        }

        Item item = currentItems[index];
        if (playerMoney >= item.cost)
        {
            playerMoney -= item.cost; // �۳���Ǯ
            TotalSpent += item.cost;

            if (cartItems.ContainsKey(item.id))
            {
                cartItems[item.id]++;
            }
            else
            {
                cartItems[item.id] = 1;
            }

            // ����������ƷPrefab
            GameObject instantiatedPrefab = SpawnItemPrefab(item.prefab);

            // ��¼Prefab����
            if (!itemPrefabsInScene.ContainsKey(item.id))
            {
                itemPrefabsInScene[item.id] = new List<GameObject>();
            }
            itemPrefabsInScene[item.id].Add(instantiatedPrefab);

            UpdateUI();

            CheckCartCapacity();
        }
        else
        {
            Debug.Log("���㣡");
        }
    }

    private void RemoveFromCart(int index)
    {
        if (index >= currentItems.Count)
        {
            Debug.LogError("��Ʒ����������Χ��");
            return;
        }

        Item item = currentItems[index];
        if (cartItems.ContainsKey(item.id) && cartItems[item.id] > 0)
        {
            playerMoney += item.cost; // �˻���Ǯ
            TotalSpent -= item.cost;

            cartItems[item.id]--;
            if (cartItems[item.id] == 0)
            {
                cartItems.Remove(item.id);
            }

            // ��������һ����ƷPrefab
            if (itemPrefabsInScene.ContainsKey(item.id) && itemPrefabsInScene[item.id].Count > 0)
            {
                Destroy(itemPrefabsInScene[item.id][0]);
                itemPrefabsInScene[item.id].RemoveAt(0);
            }

            UpdateUI();
            // ��鹺�ﳵ״̬
            CheckCartCapacity();
        }
    }

    private void AddWorker()
    {
        if (playerMoney >= workerPrice)
        {
            playerMoney -= workerPrice; // �۳���Ǯ
            workerCount++;
            cartWorkerCount++;
            TotalSpent += workerPrice;

            // �������ɹ���Prefab
            SpawnWorkerPrefab();

            UpdateUI();
            // ��鹺�ﳵ״̬
            CheckCartCapacity();
        }
        else
        {
            Debug.Log("���㣡");
        }
    }

    private void RemoveWorker()
    {
        if (workerCount > 0)
        {
            playerMoney += workerPrice; // �˻���Ǯ
            workerCount--;
            cartWorkerCount--;
            TotalSpent -= workerPrice;

            // ��������һ������Prefab
            DestroyWorkerPrefab();

            UpdateUI();
            // ��鹺�ﳵ״̬
            CheckCartCapacity();
        }
        else
        {
            Debug.Log("û���㹻�Ĺ��˿����Ƴ���");
        }
    }



    //private GameObject SpawnItemPrefab(GameObject prefab)
    //{
    //    Bounds bounds = itemAreaCollider.bounds;
    //    Vector3 randomPosition = new Vector3(
    //        Random.Range(bounds.min.x, bounds.max.x), // x���������Χ
    //        Random.Range(bounds.min.y, bounds.max.y) // y���������Χ
    //    );
    //    return Instantiate(prefab, randomPosition, Quaternion.identity, itemParent);
    //}
    private GameObject SpawnItemPrefab(GameObject prefab)
    {
        // ���ѡ��һ����Ʒ��������
        int areaIndex = Random.Range(0, 2); // 0 ��ʾ��һ������1 ��ʾ�ڶ�������

        BoxCollider2D selectedCollider;
        Transform selectedParent;

        if (areaIndex == 0)
        {
            selectedCollider = itemAreaCollider;
            selectedParent = itemParent;
        }
        else
        {
            selectedCollider = itemAreaCollider2;
            selectedParent = itemParent2;
        }

        // ��ѡ�������������������Ʒ
        Bounds bounds = selectedCollider.bounds;
        Vector3 randomPosition = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x), // x���������Χ
            Random.Range(bounds.min.y, bounds.max.y) // y���������Χ
        );

        return Instantiate(prefab, randomPosition, Quaternion.identity, selectedParent);
    }


    private bool IsPositionOccupied(Vector3 position, Transform parent, float radius = 0.5f)
    {
        // ���Ŀ��λ���Ƿ���ĳ������Prefab����ײ�巶Χ��
        foreach (Transform child in parent)
        {
            if (Vector3.Distance(position, child.position) < radius)
            {
                return true; // λ�ñ�ռ��
            }
        }
        return false; // λ��δ��ռ��
    }

    private void SpawnWorkerPrefab()
    {
        // ���ѡ��һ��������������
        int areaIndex = Random.Range(0, 2); // 0 ��ʾ��һ������1 ��ʾ�ڶ�������

        BoxCollider2D selectedCollider;
        Transform selectedParent;

        if (areaIndex == 0)
        {
            selectedCollider = workerAreaCollider;
            selectedParent = workerParent;
        }
        else
        {
            selectedCollider = workerAreaCollider2;
            selectedParent = workerParent2;
        }

        // ��ѡ����������������ɹ���
        Bounds bounds = selectedCollider.bounds;
        Vector3 randomPosition = Vector3.zero; // ��ʼ��Ϊ��������ȷ�������Ѹ�ֵ

        // �����ҵ�һ��δ��ռ�õ�λ��
        int maxAttempts = 100; // ����Դ�������������ѭ��
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            randomPosition = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x), // x���������Χ
                Random.Range(bounds.min.y, bounds.max.y) // y���������Χ
            );

            if (!IsPositionOccupied(randomPosition, selectedParent))
            {
                // �ҵ�һ��δ��ռ�õ�λ�ã����ɹ���
                Instantiate(workerPrefab, randomPosition, Quaternion.identity, selectedParent);
                return;
            }
        }

        // �����������������δ�ҵ����ʵ�λ�ã��������һ��λ�����ɹ���
        Instantiate(workerPrefab, randomPosition, Quaternion.identity, selectedParent);
    }

    private void DestroyWorkerPrefab()
    {
        // �������ٵ�һ������Ĺ���
        if (workerParent.childCount > 0)
        {
            Destroy(workerParent.GetChild(0).gameObject);
            return;
        }

        // �����һ������û�й��ˣ������ٵڶ�������Ĺ���
        if (workerParent2.childCount > 0)
        {
            Destroy(workerParent2.GetChild(0).gameObject);
        }
    }

    public void AutoCheckout()
    {
        // ����������Ʒ
        HandleExcessItems();
        int totalCost = CalculateTotalCost();
        int totalSellPrice = CalculateTotalSellPrice();
        playerMoney += totalSellPrice; // ����ʱ�����ʽ�

        // ��չ��ﳵ
        cartItems.Clear();
        cartWorkerCount = 0;
        TotalSpent = 0;

        // ���������ƷPrefab
        foreach (var list in itemPrefabsInScene.Values)
        {
            foreach (var prefab in list)
            {
                Destroy(prefab);
            }
        }
        itemPrefabsInScene.Clear();

        UpdateUI();

    }

    private int CalculateTotalCost()
    {
        int totalCost = 0;
        foreach (var item in cartItems)
        {
            Item itemInAllItems = allItems.Find(i => i.id == item.Key);
            if (itemInAllItems != null)
            {
                totalCost += itemInAllItems.cost * item.Value;
            }
        }
        if (cartWorkerCount > 0)
        {
            totalCost += cartWorkerCount * workerPrice;
        }
        // �����豸���ܳɱ�
        totalCost += TotalSpent;
        return totalCost;
    }

    public int CalculateTotalSellPrice()
    {
        // ����������Ʒ
        HandleExcessItems();
        int totalSellPrice = 0;
        foreach (var item in cartItems)
        {
            Item itemInAllItems = allItems.Find(i => i.id == item.Key);
            if (itemInAllItems != null)
            {
                totalSellPrice += itemInAllItems.sellPrice * item.Value;
            }
        }

        return totalSellPrice;
    }


    
    private int GetCurrentDevicePrice()
    {
        switch (deviceLevel)
        {
            case 0:
                return devicePriceLevel1;
            case 1:
                return devicePriceLevel2;
            case 2:
                return devicePriceLevel3;
            default:
                return 0; // ����豸�ȼ�����3������0�������ϲ��ᷢ����
        }
    }

    
    // ���һ���������������ڼ�鹺�ﳵ״̬
    public void CheckCartCapacity()
    {
        if (IsCartExceedWorkerCapacity)
        {
            // ���������֪ͨ TimeSystem_7
            TimeSystem_7 timeSystem = FindObjectOfType<TimeSystem_7>();
            if (timeSystem != null)
            {
                timeSystem.SetCartExceedWorkerCapacity(true);
                Debug.Log("���ﳵ�������˿ɼӹ���������֪ͨ TimeSystem_7��");
            }

        }
        else
        {
            // ���δ������֪ͨ TimeSystem_7
            TimeSystem_7 timeSystem = FindObjectOfType<TimeSystem_7>();
            if (timeSystem != null)
            {
                timeSystem.SetCartExceedWorkerCapacity(false);
                Debug.Log("���ﳵδ�������˿ɼӹ���������֪ͨ TimeSystem_7��");
            }
        }
    }

    // ���ڼ�¼δ�۳�����Ʒ
    public List<Item> excessItems = new List<Item>();

    private void HandleExcessItems()
    {
        int totalItemsInCart = cartItems.Values.Sum();
        int maxProcessableItems = workerCount * 3;
        int excessItemCount = totalItemsInCart - maxProcessableItems;

        if (excessItemCount > 0)
        {
            excessItems.Clear(); // ���֮ǰ�ļ�¼

            List<int> itemIds = new List<int>(cartItems.Keys);
            while (excessItemCount > 0 && itemIds.Count > 0)
            {
                int randomIndex = Random.Range(0, itemIds.Count);
                int itemId = itemIds[randomIndex];

                Item item = allItems.Find(i => i.id == itemId);
                if (item != null)
                {
                    int itemCount = cartItems[itemId];
                    int itemsToRemove = Mathf.Min(itemCount, excessItemCount);

                    cartItems[itemId] -= itemsToRemove;
                    if (cartItems[itemId] == 0)
                    {
                        cartItems.Remove(itemId);
                    }

                    excessItemCount -= itemsToRemove;
                    for (int i = 0; i < itemsToRemove; i++)
                    {
                        excessItems.Add(item); // ��¼δ�۳�����Ʒ
                    }
                }

                itemIds.RemoveAt(randomIndex);
            }

            // ������Ϣ
            foreach (var item in excessItems)
            {
                Debug.Log($"δ�ӹ�����Ʒ: {item.name}");
            }
        }
    }

    private void InitializeItemsByTimePeriod()
    {
        itemsByTimePeriod = new Dictionary<string, List<Item>>
        {
            { "1985-1-1987-5", new List<Item> { allItems[0], allItems[1], allItems[2], allItems[3], allItems[4], allItems[5], allItems[6], allItems[7] } },
            { "1987-7-1987-7", new List<Item> { allItems[10], allItems[1], allItems[2], allItems[8], allItems[4], allItems[11] } },
            { "1987-9-1997-5", new List<Item> { allItems[21], allItems[1], allItems[2], allItems[3], allItems[9], allItems[11], allItems[12], allItems[13] } },
            { "1997-7-1997-7", new List<Item> { allItems[23], allItems[9], allItems[15], allItems[11], allItems[10], allItems[12] } },
            { "1997-9-2007-9", new List<Item> { allItems[14], allItems[13], allItems[15], allItems[16], allItems[9], allItems[11], allItems[17], allItems[18] } },
            { "2007-11-2007-11", new List<Item> { allItems[19], allItems[14], allItems[9], allItems[11], allItems[17], allItems[18] } },
            { "2008-1-2008-3", new List<Item> { allItems[14], allItems[13], allItems[15], allItems[16], allItems[9], allItems[11], allItems[17], allItems[18] } },
            { "2008-5-2008-5", new List<Item> { allItems[10], allItems[22], allItems[13], allItems[11], allItems[17], allItems[18] } },
            { "2008-7-2009-11", new List<Item> { allItems[14], allItems[13], allItems[15], allItems[16], allItems[9], allItems[11], allItems[17], allItems[18] } },
            { "2010-1-2010-1", new List<Item> { allItems[20], allItems[14], allItems[13], allItems[15], allItems[16], allItems[9] } }
            // ���Ը�����Ҫ������Ӹ���ʱ���
            };
    }
    public void SelectItemsByTime(int year, int month)
    {
        currentItems.Clear();
        string timePeriod = GetTimePeriod(year, month);

        if (itemsByTimePeriod.ContainsKey(timePeriod))
        {
            List<Item> itemsForTimePeriod = itemsByTimePeriod[timePeriod];
            // ��ʱ�����Ʒ�б������ѡ��6����Ʒ
            List<Item> selectedItems = new List<Item>();
            while (selectedItems.Count < 6)
            {
                int randomIndex = Random.Range(0, itemsForTimePeriod.Count);
                Item selectedItem = itemsForTimePeriod[randomIndex];
                if (!selectedItems.Contains(selectedItem))
                {
                    selectedItems.Add(selectedItem);
                }
            }
            currentItems.AddRange(selectedItems);
        }
        else
        {
            Debug.LogError($"No items defined for time period: {timePeriod}");
        }


        // ������Ʒ��ť����ʾ
        for (int i = 0; i < buyButtons.Length; i++)
        {
            if (i < currentItems.Count)
            {
                buyButtons[i].gameObject.SetActive(true);
                itemTexts[i].text = $"{currentItems[i].name}  {currentItems[i].cost}RMB";

                // ���ð�ťͼƬ
                if (currentItems[i].itemImage != null)
                {
                    buyButtons[i].GetComponent<Image>().sprite = currentItems[i].itemImage;
                }
                else
                {
                    buyButtons[i].GetComponent<Image>().sprite = null; // ���û��ͼƬ�����
                }
            }
            else
            {
                buyButtons[i].gameObject.SetActive(false);
                itemTexts[i].text = ""; // ����ı�����
            }
        }
    }

    private string GetTimePeriod(int year, int month)
    {
        // ����ʱ���
        var timePeriods = new List<(string, string)>
    {
        ("1985-1", "1987-5"),
        ("1987-7", "1987-7"),
        ("1987-9", "1997-5"),
        ("1997-7", "1997-7"),
        ("1997-9", "2007-9"),
        ("2007-11", "2007-11"),
        ("2008-1", "2008-3"),
        ("2008-5", "2008-5"),
        ("2008-7", "2009-11"),
        ("2010-1", "2010-1")
    };

        foreach (var (start, end) in timePeriods)
        {
            if (IsWithinPeriod(year, month, start, end))
            {
                return $"{start}-{end}";
            }
        }

        return "Unknown";
    }

    private bool IsWithinPeriod(int year, int month, string startPeriod, string endPeriod)
    {
        var start = ParsePeriod(startPeriod);
        var end = ParsePeriod(endPeriod);
        var current = (year, month);

        // �Ƚϵ�ǰʱ���Ƿ�����ʼʱ��ͽ���ʱ��֮��
        bool isAfterStart = (current.Item1 > start.Item1) || (current.Item1 == start.Item1 && current.Item2 >= start.Item2);
        bool isBeforeEnd = (current.Item1 < end.Item1) || (current.Item1 == end.Item1 && current.Item2 <= end.Item2);

        return isAfterStart && isBeforeEnd;
    }

    private (int, int) ParsePeriod(string period)
    {
        var parts = period.Split('-');
        return (int.Parse(parts[0]), int.Parse(parts[1]));
    }

    private void UpdateDeviceButton(int index)
    {
        int level = index + 1; // �豸�ȼ���1��ʼ
        if (deviceLevel >= level)
        {
            deviceButtons[index].gameObject.SetActive(false); // ���ذ�ť
            ShowLowestProfitItems(level); // ��ʾ���������Ʒ��ʾ
        }
        else
        {
            deviceButtons[index].gameObject.SetActive(true); // ��ʾ��ť
            deviceTexts[index].text = ""; // ����ı�����
        }
    }
    private void BuyDevice(int level)
    {
        if (playerMoney >= GetDevicePrice(level))
        {
            playerMoney -= GetDevicePrice(level);
            deviceLevel = level;

            // �����ܳɱ������ݵ�ǰ�豸�ȼ���������Ľ��
            TotalSpent += GetDevicePrice(level);

            UpdateUI();
        }
        else
        {
            Debug.Log("���㣡");
        }
    }

    private int GetDevicePrice(int level)
    {
        switch (level)
        {
            case 1:
                return devicePriceLevel1;
            case 2:
                return devicePriceLevel2;
            case 3:
                return devicePriceLevel3;
            default:
                return 0; // ����豸�ȼ�����3������0�������ϲ��ᷢ����
        }
    }

    private void ShowLowestProfitItems(int level)
    {
        // ����������Ʒ������
        List<Item> sortedItems = allItems.OrderBy(item => item.sellPrice - item.cost).ToList();

        // �����豸�ȼ���ʾ���������Ʒ
        int count = Mathf.Min(level, 3); // ��һ�ι�����ʾ1�����ڶ��ι�����ʾ2���������ι�����ʾ3��
        string message = "���������Ʒ:\n";
        for (int i = 0; i < count; i++)
        {
            if (i < sortedItems.Count) // ȷ�����ᳬ����Ʒ�б�ķ�Χ
            {
                message += $"{sortedItems[i].name} (����: {sortedItems[i].sellPrice - sortedItems[i].cost})\n";
            }
        }

        // ���¶�Ӧ��TextMeshProUGUI���ı�����
        deviceTexts[level - 1].text = message;
    }
}