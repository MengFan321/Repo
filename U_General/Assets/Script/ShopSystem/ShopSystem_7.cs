using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Item
{
    public int id; // 唯一标识
    public string name; // 商品名称
    public int cost; // 商品成本
    public int sellPrice; // 商品售价
    public GameObject prefab; // 商品Prefab
    public Sprite itemImage; // 商品图片
}
public class ShopSystem_7 : MonoBehaviour
{
    public TextMeshProUGUI moneyText; // 显示玩家资金的TextMeshProUGUI
    public TextMeshProUGUI cartText; // 显示购物车内容的TextMeshProUGUI
    public TextMeshProUGUI workerCountText; // 显示已购工人数量的TextMeshProUGUI

    public Button[] buyButtons; // 购买商品按钮数组
    public Button buyWorkerButton; // 购买工人的按钮
    public Button[] removeButtons; // 移除商品按钮数组
    public Button removeWorkerButton; // 移除工人按钮

    public GameObject workerPrefab; // 工人Prefab
    public Transform workerParent; // 工人父对象（用于组织工人对象）
    public BoxCollider2D workerAreaCollider; // 工人区域的BoxCollider2D
    public Transform workerParent2; // 第二个工人父对象
    public BoxCollider2D workerAreaCollider2; // 第二个工人区域的BoxCollider2D

    public Transform itemParent; // 商品父对象（用于组织商品对象）
    public BoxCollider2D itemAreaCollider; // 商品区域的BoxCollider2D
    public BoxCollider2D itemAreaCollider2; // 第二个商品区域的BoxCollider2D
    public Transform itemParent2; // 第二个商品父对象（用于组织商品对象）

    public TextMeshProUGUI[] itemTexts; // 用于显示商品名称和价格的文本组件数组

    public int playerMoney = 1000; // 玩家初始资金
    private int workerPrice = 70; // 购买工人的价格
    private int workerCount = 0; // 已购工人的数量

    private Dictionary<int, int> cartItems = new Dictionary<int, int>(); // 购物车中的商品ID及其数量
    private int cartWorkerCount = 0; // 购物车中的工人数量
    public int TotalSpent { get; private set; } // 总花费，公开只读

    // 定义所有商品

    public List<Item> allItems = new List<Item>
    {
        new Item { id = 0, name = "大米", cost = 10, sellPrice = 80, prefab = null },
        new Item { id = 1, name = "苹果", cost = 20, sellPrice = 80, prefab = null },
        new Item { id = 2, name = "盐", cost = 10, sellPrice = 60, prefab = null },
        new Item { id = 3, name = "田子本", cost = 10, sellPrice = 30, prefab = null },
        new Item { id = 4, name = "香蕉", cost = 40, sellPrice = 80, prefab = null },
        new Item { id = 5, name = "白菜", cost = 20, sellPrice = 60, prefab = null },
        new Item { id = 6, name = "肥皂", cost = 40, sellPrice = 140, prefab = null },
        new Item { id = 7, name = "粉笔", cost = 5, sellPrice = 5, prefab = null },
        new Item { id = 8, name = "经济学书本", cost = 20, sellPrice = 140, prefab = null },
        new Item { id = 9, name = "钢笔", cost = 80, sellPrice = 140, prefab = null },
        new Item { id = 10, name = "面包", cost = 80, sellPrice = 0, prefab = null },
        new Item { id = 11, name = "香波", cost = 40, sellPrice = 140, prefab = null },
        new Item { id = 12, name = "收音机", cost = 100, sellPrice = 30, prefab = null },
        new Item { id = 13, name = "沙拉", cost = 80, sellPrice = 30, prefab = null },
        new Item { id = 14, name = "寿司", cost = 40, sellPrice = 80, prefab = null },
        new Item { id = 15, name = "巧克力", cost = 80, sellPrice = 140, prefab = null },
        new Item { id = 16, name = "奶龙尸块", cost = 20, sellPrice = 140, prefab = null },
        new Item { id = 17, name = "护肤品保湿水", cost = 100, sellPrice = 140, prefab = null },
        new Item { id = 18, name = "蓝牙耳机", cost = 100, sellPrice = 140, prefab = null },
        new Item { id = 19, name = "武汉长江大桥模型", cost = 20, sellPrice = 140, prefab = null },
        new Item { id = 20, name = "5G手机", cost = 40, sellPrice = 140, prefab = null },
        new Item { id = 21, name = "面包", cost = 40, sellPrice = 80, prefab = null },
        new Item { id = 22, name = "巧克力", cost = 100, sellPrice = 0, prefab = null },
        new Item { id = 23, name = "紫荆花发卡", cost = 60, sellPrice = 140, prefab = null },

    };
    // 根据时间段选择商品
    private Dictionary<string, List<Item>> itemsByTimePeriod;



    // 当前显示的商品
    private List<Item> currentItems = new List<Item>();

    // 用于记录每个商品Prefab的引用
    private Dictionary<int, List<GameObject>> itemPrefabsInScene = new Dictionary<int, List<GameObject>>();

    
    // 定义设备按钮和文本组件数组
    public Button[] deviceButtons; // 设备购买按钮数组
    public TextMeshProUGUI[] deviceTexts; // 设备按钮上的文本数组

    // 定义设备价格
    private const int devicePriceLevel1 = 300; // 第一次购买价格
    private const int devicePriceLevel2 = 600; // 第二次购买价格
    private const int devicePriceLevel3 = 900; // 第三次购买价格

    // 设备等级
    private int deviceLevel = 0; // 设备等级，用于控制功能


    // 添加一个标志变量，用于记录是否超出工人可加工数量
    public bool IsCartExceedWorkerCapacity => cartItems.Values.Sum() > workerCount * 3;
    public List<string> discardedItemsInfo = new List<string>(); // 用于存储被丢弃的商品信息
    void Start()
    {
        // 初始化时间段商品字典
        InitializeItemsByTimePeriod();

        // 根据当前时间选择商品
        int currentYear = 1985; // 假设初始年份是 1985
        int currentMonth = 1;
        SelectItemsByTime(currentYear, currentMonth);

        UpdateUI();


        // 为每个商品按钮添加点击事件并设置图片
        for (int i = 0; i < buyButtons.Length; i++)
        {
            int index = i; // 保存当前按钮的索引
            buyButtons[i].onClick.AddListener(() => AddToCart(index));

            // 设置按钮图片
            if (i < currentItems.Count && currentItems[i].itemImage != null)
            {
                buyButtons[i].GetComponent<Image>().sprite = currentItems[i].itemImage;
            }
            else
            {
                buyButtons[i].GetComponent<Image>().sprite = null; // 如果没有图片，清空
            }
        }

        // 为购买工人按钮添加点击事件
        buyWorkerButton.onClick.AddListener(AddWorker);

        // 为每个移除商品按钮添加点击事件
        for (int i = 0; i < removeButtons.Length; i++)
        {
            int index = i; // 保存当前按钮的索引
            removeButtons[i].onClick.AddListener(() => RemoveFromCart(index));
        }

        // 为移除工人按钮添加点击事件
        removeWorkerButton.onClick.AddListener(RemoveWorker);

        
        // 初始化设备按钮
        for (int i = 0; i < deviceButtons.Length; i++)
        {
            int level = i + 1; // 设备等级从1开始
            deviceButtons[i].onClick.AddListener(() => BuyDevice(level));
            UpdateDeviceButton(i);
        }
    }

    private void UpdateUI()
    {
        moneyText.text = $"Money: {playerMoney}";
        workerCountText.text = $"WorkerCount: {workerCount}";

        // 更新购物车内容
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
        // 更新设备按钮
        for (int i = 0; i < deviceButtons.Length; i++)
        {
            UpdateDeviceButton(i);
        }

        //// 显示最低利润商品提示
        //for (int i = 0; i < deviceLevel; i++)
        //{
        //    ShowLowestProfitItems(i + 1);
        //}
    }

    private void AddToCart(int index)
    {
        if (index >= currentItems.Count)
        {
            Debug.LogError("商品索引超出范围！");
            return;
        }

        Item item = currentItems[index];
        if (playerMoney >= item.cost)
        {
            playerMoney -= item.cost; // 扣除金钱
            TotalSpent += item.cost;

            if (cartItems.ContainsKey(item.id))
            {
                cartItems[item.id]++;
            }
            else
            {
                cartItems[item.id] = 1;
            }

            // 立刻生成商品Prefab
            GameObject instantiatedPrefab = SpawnItemPrefab(item.prefab);

            // 记录Prefab引用
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
            Debug.Log("金额不足！");
        }
    }

    private void RemoveFromCart(int index)
    {
        if (index >= currentItems.Count)
        {
            Debug.LogError("商品索引超出范围！");
            return;
        }

        Item item = currentItems[index];
        if (cartItems.ContainsKey(item.id) && cartItems[item.id] > 0)
        {
            playerMoney += item.cost; // 退还金钱
            TotalSpent -= item.cost;

            cartItems[item.id]--;
            if (cartItems[item.id] == 0)
            {
                cartItems.Remove(item.id);
            }

            // 立刻销毁一个商品Prefab
            if (itemPrefabsInScene.ContainsKey(item.id) && itemPrefabsInScene[item.id].Count > 0)
            {
                Destroy(itemPrefabsInScene[item.id][0]);
                itemPrefabsInScene[item.id].RemoveAt(0);
            }

            UpdateUI();
            // 检查购物车状态
            CheckCartCapacity();
        }
    }

    private void AddWorker()
    {
        if (playerMoney >= workerPrice)
        {
            playerMoney -= workerPrice; // 扣除金钱
            workerCount++;
            cartWorkerCount++;
            TotalSpent += workerPrice;

            // 立刻生成工人Prefab
            SpawnWorkerPrefab();

            UpdateUI();
            // 检查购物车状态
            CheckCartCapacity();
        }
        else
        {
            Debug.Log("金额不足！");
        }
    }

    private void RemoveWorker()
    {
        if (workerCount > 0)
        {
            playerMoney += workerPrice; // 退还金钱
            workerCount--;
            cartWorkerCount--;
            TotalSpent -= workerPrice;

            // 立刻销毁一个工人Prefab
            DestroyWorkerPrefab();

            UpdateUI();
            // 检查购物车状态
            CheckCartCapacity();
        }
        else
        {
            Debug.Log("没有足够的工人可以移除！");
        }
    }



    //private GameObject SpawnItemPrefab(GameObject prefab)
    //{
    //    Bounds bounds = itemAreaCollider.bounds;
    //    Vector3 randomPosition = new Vector3(
    //        Random.Range(bounds.min.x, bounds.max.x), // x方向随机范围
    //        Random.Range(bounds.min.y, bounds.max.y) // y方向随机范围
    //    );
    //    return Instantiate(prefab, randomPosition, Quaternion.identity, itemParent);
    //}
    private GameObject SpawnItemPrefab(GameObject prefab)
    {
        // 随机选择一个商品生成区域
        int areaIndex = Random.Range(0, 2); // 0 表示第一个区域，1 表示第二个区域

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

        // 在选定的区域中随机生成商品
        Bounds bounds = selectedCollider.bounds;
        Vector3 randomPosition = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x), // x方向随机范围
            Random.Range(bounds.min.y, bounds.max.y) // y方向随机范围
        );

        return Instantiate(prefab, randomPosition, Quaternion.identity, selectedParent);
    }


    private bool IsPositionOccupied(Vector3 position, Transform parent, float radius = 0.5f)
    {
        // 检查目标位置是否在某个工人Prefab的碰撞体范围内
        foreach (Transform child in parent)
        {
            if (Vector3.Distance(position, child.position) < radius)
            {
                return true; // 位置被占用
            }
        }
        return false; // 位置未被占用
    }

    private void SpawnWorkerPrefab()
    {
        // 随机选择一个工人生成区域
        int areaIndex = Random.Range(0, 2); // 0 表示第一个区域，1 表示第二个区域

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

        // 在选定的区域中随机生成工人
        Bounds bounds = selectedCollider.bounds;
        Vector3 randomPosition = Vector3.zero; // 初始化为零向量，确保变量已赋值

        // 尝试找到一个未被占用的位置
        int maxAttempts = 100; // 最大尝试次数，避免无限循环
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            randomPosition = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x), // x方向随机范围
                Random.Range(bounds.min.y, bounds.max.y) // y方向随机范围
            );

            if (!IsPositionOccupied(randomPosition, selectedParent))
            {
                // 找到一个未被占用的位置，生成工人
                Instantiate(workerPrefab, randomPosition, Quaternion.identity, selectedParent);
                return;
            }
        }

        // 如果尝试了最大次数仍未找到合适的位置，则在最后一个位置生成工人
        Instantiate(workerPrefab, randomPosition, Quaternion.identity, selectedParent);
    }

    private void DestroyWorkerPrefab()
    {
        // 优先销毁第一个区域的工人
        if (workerParent.childCount > 0)
        {
            Destroy(workerParent.GetChild(0).gameObject);
            return;
        }

        // 如果第一个区域没有工人，则销毁第二个区域的工人
        if (workerParent2.childCount > 0)
        {
            Destroy(workerParent2.GetChild(0).gameObject);
        }
    }

    public void AutoCheckout()
    {
        // 处理超出的商品
        HandleExcessItems();
        int totalCost = CalculateTotalCost();
        int totalSellPrice = CalculateTotalSellPrice();
        playerMoney += totalSellPrice; // 结算时增加资金

        // 清空购物车
        cartItems.Clear();
        cartWorkerCount = 0;
        TotalSpent = 0;

        // 清空所有商品Prefab
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
        // 加上设备的总成本
        totalCost += TotalSpent;
        return totalCost;
    }

    public int CalculateTotalSellPrice()
    {
        // 处理超出的商品
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
                return 0; // 如果设备等级超过3，返回0（理论上不会发生）
        }
    }

    
    // 添加一个公共方法，用于检查购物车状态
    public void CheckCartCapacity()
    {
        if (IsCartExceedWorkerCapacity)
        {
            // 如果超出，通知 TimeSystem_7
            TimeSystem_7 timeSystem = FindObjectOfType<TimeSystem_7>();
            if (timeSystem != null)
            {
                timeSystem.SetCartExceedWorkerCapacity(true);
                Debug.Log("购物车超出工人可加工数量，已通知 TimeSystem_7。");
            }

        }
        else
        {
            // 如果未超出，通知 TimeSystem_7
            TimeSystem_7 timeSystem = FindObjectOfType<TimeSystem_7>();
            if (timeSystem != null)
            {
                timeSystem.SetCartExceedWorkerCapacity(false);
                Debug.Log("购物车未超出工人可加工数量，已通知 TimeSystem_7。");
            }
        }
    }

    // 用于记录未售出的商品
    public List<Item> excessItems = new List<Item>();

    private void HandleExcessItems()
    {
        int totalItemsInCart = cartItems.Values.Sum();
        int maxProcessableItems = workerCount * 3;
        int excessItemCount = totalItemsInCart - maxProcessableItems;

        if (excessItemCount > 0)
        {
            excessItems.Clear(); // 清空之前的记录

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
                        excessItems.Add(item); // 记录未售出的商品
                    }
                }

                itemIds.RemoveAt(randomIndex);
            }

            // 调试信息
            foreach (var item in excessItems)
            {
                Debug.Log($"未加工的商品: {item.name}");
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
            // 可以根据需要继续添加更多时间段
            };
    }
    public void SelectItemsByTime(int year, int month)
    {
        currentItems.Clear();
        string timePeriod = GetTimePeriod(year, month);

        if (itemsByTimePeriod.ContainsKey(timePeriod))
        {
            List<Item> itemsForTimePeriod = itemsByTimePeriod[timePeriod];
            // 从时间段商品列表中随机选择6种商品
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


        // 更新商品按钮的显示
        for (int i = 0; i < buyButtons.Length; i++)
        {
            if (i < currentItems.Count)
            {
                buyButtons[i].gameObject.SetActive(true);
                itemTexts[i].text = $"{currentItems[i].name}  {currentItems[i].cost}RMB";

                // 设置按钮图片
                if (currentItems[i].itemImage != null)
                {
                    buyButtons[i].GetComponent<Image>().sprite = currentItems[i].itemImage;
                }
                else
                {
                    buyButtons[i].GetComponent<Image>().sprite = null; // 如果没有图片，清空
                }
            }
            else
            {
                buyButtons[i].gameObject.SetActive(false);
                itemTexts[i].text = ""; // 清空文本内容
            }
        }
    }

    private string GetTimePeriod(int year, int month)
    {
        // 定义时间段
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

        // 比较当前时间是否在起始时间和结束时间之间
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
        int level = index + 1; // 设备等级从1开始
        if (deviceLevel >= level)
        {
            deviceButtons[index].gameObject.SetActive(false); // 隐藏按钮
            ShowLowestProfitItems(level); // 显示最低利润商品提示
        }
        else
        {
            deviceButtons[index].gameObject.SetActive(true); // 显示按钮
            deviceTexts[index].text = ""; // 清空文本内容
        }
    }
    private void BuyDevice(int level)
    {
        if (playerMoney >= GetDevicePrice(level))
        {
            playerMoney -= GetDevicePrice(level);
            deviceLevel = level;

            // 更新总成本，根据当前设备等级决定加入的金额
            TotalSpent += GetDevicePrice(level);

            UpdateUI();
        }
        else
        {
            Debug.Log("金额不足！");
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
                return 0; // 如果设备等级超过3，返回0（理论上不会发生）
        }
    }

    private void ShowLowestProfitItems(int level)
    {
        // 计算所有商品的利润
        List<Item> sortedItems = allItems.OrderBy(item => item.sellPrice - item.cost).ToList();

        // 根据设备等级显示最低利润商品
        int count = Mathf.Min(level, 3); // 第一次购买显示1个，第二次购买显示2个，第三次购买显示3个
        string message = "最低利润商品:\n";
        for (int i = 0; i < count; i++)
        {
            if (i < sortedItems.Count) // 确保不会超出商品列表的范围
            {
                message += $"{sortedItems[i].name} (利润: {sortedItems[i].sellPrice - sortedItems[i].cost})\n";
            }
        }

        // 更新对应的TextMeshProUGUI的文本内容
        deviceTexts[level - 1].text = message;
    }
}