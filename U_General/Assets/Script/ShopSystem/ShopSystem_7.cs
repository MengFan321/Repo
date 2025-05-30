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
        new Item { id = 7, name = "粉笔", cost = 5, sellPrice = 5, prefab = null }
    };

    // 当前显示的商品
    private List<Item> currentItems = new List<Item>();

    // 用于记录每个商品Prefab的引用
    private Dictionary<int, List<GameObject>> itemPrefabsInScene = new Dictionary<int, List<GameObject>>();

    // 定义设备商品
    public Item deviceItem = new Item
    {
        id = 999, // 特殊ID
        name = "设备",
        cost = 300, // 初始购买价格
        sellPrice = 0, // 设备不参与利润计算
        prefab = null // 设备不需要Prefab
    };

    private int deviceLevel = 0; // 设备等级，用于控制功能
    private const int devicePriceLevel1 = 300; // 第一次购买价格
    private const int devicePriceLevel2 = 600; // 第二次购买价格
    private const int devicePriceLevel3 = 900; // 第二次购买价格

    public Button deviceButton; // 设备购买按钮
    public TextMeshProUGUI deviceText; // 设备按钮上的文本

    public TextMeshProUGUI lowestProfitText; // 用于显示利润最低商品的TextMeshProUGUI

    void Start()
    {
        UpdateUI();

        // 随机选择6种商品
        SelectRandomItems();
        // 为每个商品按钮添加点击事件
        for (int i = 0; i < buyButtons.Length; i++)
        {
            int index = i; // 保存当前按钮的索引
            buyButtons[i].onClick.AddListener(() => AddToCart(index));
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
        deviceButton.onClick.AddListener(BuyDevice);
        UpdateDeviceButton();
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

        UpdateDeviceButton();
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
        }
        else
        {
            Debug.Log("没有足够的工人可以移除！");
        }
    }



    private GameObject SpawnItemPrefab(GameObject prefab)
    {
        Bounds bounds = itemAreaCollider.bounds;
        Vector3 randomPosition = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x), // x方向随机范围
            Random.Range(bounds.min.y, bounds.max.y) // y方向随机范围
        );
        return Instantiate(prefab, randomPosition, Quaternion.identity, itemParent);
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

    //public void AutoCheckout()
    //{
    //    int totalCost = CalculateTotalCost();


    //    if (playerMoney >= totalCost)
    //    {
    //        playerMoney -= totalCost;

    //        foreach (var item in cartItems)
    //        {
    //            // 更新已购商品数量
    //            Item itemInAllItems = allItems.Find(i => i.id == item.Key);
    //            if (itemInAllItems != null)
    //            {
    //                itemInAllItems.cost = item.Value; // 这里可能需要修改逻辑
    //            }
    //        }

    //        // 清空购物车
    //        cartItems.Clear();
    //        cartWorkerCount = 0;
    //        TotalSpent = 0;

    //        // 清空所有商品Prefab
    //        foreach (var list in itemPrefabsInScene.Values)
    //        {
    //            foreach (var prefab in list)
    //            {
    //                Destroy(prefab);
    //            }
    //        }
    //        itemPrefabsInScene.Clear();

    //        UpdateUI();
    //    }
    //    else
    //    {
    //        Debug.Log("金额不足！");
    //    }
    //}

    public void AutoCheckout()
    {
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

    private void SelectRandomItems()
    {
        currentItems.Clear();
        List<Item> tempItems = new List<Item>(allItems);
        while (currentItems.Count < 6 && tempItems.Count > 0)
        {
            int randomIndex = Random.Range(0, tempItems.Count);
            currentItems.Add(tempItems[randomIndex]); // 直接添加引用
            tempItems.RemoveAt(randomIndex);
        }

        // 更新商品按钮的显示
        for (int i = 0; i < buyButtons.Length; i++)
        {
            if (i < currentItems.Count)
            {
                buyButtons[i].gameObject.SetActive(true);
                itemTexts[i].text = $"{currentItems[i].name}\nCost: {currentItems[i].cost}";
            }
            else
            {
                buyButtons[i].gameObject.SetActive(false);
                itemTexts[i].text = ""; // 清空文本内容
            }
        }
    }

    private void BuyDevice()
    {
        if (playerMoney >= GetCurrentDevicePrice())
        {
            playerMoney -= GetCurrentDevicePrice();
            deviceLevel++;

            // 更新总成本，根据当前设备等级决定加入的金额
            switch (deviceLevel)
            {
                case 1:
                    TotalSpent += devicePriceLevel1; // 第一次购买，加上300元
                    break;
                case 2:
                    TotalSpent += devicePriceLevel2; // 第二次购买，加上600元
                    break;
                case 3:
                    TotalSpent += devicePriceLevel3; // 第三次购买，加上900元
                    break;
            }

            // 更新设备状态
            UpdateDeviceButton();

            // 显示最低利润商品
            ShowLowestProfitItems();

            UpdateUI();
        }
        else
        {
            Debug.Log("金额不足！");
        }
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

    private void UpdateDeviceButton()
    {
        if (deviceLevel == 0)
        {
            deviceText.text = $"设备\nCost: {devicePriceLevel1}";
        }
        else if (deviceLevel == 1)
        {
            deviceText.text = $"设备\nCost: {devicePriceLevel2}";
        }
        else if (deviceLevel == 2)
        {
            deviceText.text = $"设备\nCost: {devicePriceLevel3}";
        }
        else
        {
            deviceButton.gameObject.SetActive(false); // 隐藏按钮
            deviceText.text = "设备已购买";
        }
    }

    private void ShowLowestProfitItems()
    {
        // 计算所有商品的利润
        List<Item> sortedItems = allItems.OrderBy(item => item.sellPrice - item.cost).ToList();

        // 根据设备等级显示最低利润商品
        int count = Mathf.Min(deviceLevel, 3); // 第一次购买显示1个，第二次购买显示2个，第三次购买显示3个
        string message = "最低利润商品:\n";
        for (int i = 0; i < count; i++)
        {
            if (i < sortedItems.Count) // 确保不会超出商品列表的范围
            {
                message += $"{sortedItems[i].name} (利润: {sortedItems[i].sellPrice - sortedItems[i].cost})\n";
            }
        }

        // 更新TextMeshProUGUI的文本内容
        lowestProfitText.text = message;
    }
}