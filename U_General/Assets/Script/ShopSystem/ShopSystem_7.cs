using System.Collections;
using System.Collections.Generic;
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
    }

    private void UpdateUI()
    {
        moneyText.text = $"金额: {playerMoney}";
        workerCountText.text = $"员工: {workerCount}";

        // 更新购物车内容
        cartText.text = "购物车:\n";
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
            cartText.text += $"员工: {cartWorkerCount}\n";
        }
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

    private void SpawnWorkerPrefab()
    {
        Bounds bounds = workerAreaCollider.bounds;
        Vector3 randomPosition = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x), // x方向随机范围
            Random.Range(bounds.min.y, bounds.max.y) // y方向随机范围
        );
        Instantiate(workerPrefab, randomPosition, Quaternion.identity, workerParent);
    }

    private void DestroyWorkerPrefab()
    {
        if (workerParent.childCount > 0)
        {
            Destroy(workerParent.GetChild(0).gameObject);
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
}