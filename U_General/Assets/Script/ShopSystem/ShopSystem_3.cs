using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSystem_3 : MonoBehaviour
{
    // 公开的UI引用
    public TextMeshProUGUI moneyText; // 显示玩家资金的TextMeshProUGUI
    public TextMeshProUGUI itemCountText; // 显示已购商品数量的TextMeshProUGUI
    public TextMeshProUGUI workerCountText; // 显示已购工人数量的TextMeshProUGUI
    public TextMeshProUGUI cartText; // 显示购物车内容的TextMeshProUGUI

    public Button[] buyButtons; // 购买商品按钮数组
    public Button buyWorkerButton; // 购买工人的按钮
    
    public Button[] removeButtons; // 移除商品按钮数组
    public Button removeWorkerButton; // 移除工人按钮
    
    

    public Button checkoutButton; // 结算按钮

    public GameObject workerPrefab; // 工人Prefab
    public Transform workerParent; // 工人父对象（可选，用于组织工人对象）
    public BoxCollider2D areaCollider; // 矩形区域的BoxCollider2D


    // 私有变量
    private int playerMoney = 1000; // 玩家初始资金
    private int[] itemPrices = { 20, 40, 80, 100 }; // 每个商品的价格
    private int[] itemCounts = { 0, 0, 0, 0 }; // 每个商品的已购数量
    private int workerPrice = 70; // 购买工人的价格
    private int workerCount = 0; // 已购工人的数量

    

    private Dictionary<int, int> cartItems = new Dictionary<int, int>(); // 购物车中的商品索引及其数量
    private int cartWorkerCount = 0; // 购物车中的工人数量

    void Start()
    {
        // 初始化UI显示
        UpdateUI();

        // 为每个商品按钮添加点击事件
        for (int i = 0; i < buyButtons.Length; i++)
        {
            int index = i; // 保存当前按钮的索引
            buyButtons[i].onClick.AddListener(() => AddToCart(index));
        }

        // 为购买工人按钮添加点击事件
        buyWorkerButton.onClick.AddListener(AddWorkerToCart);

        // 为结算按钮添加点击事件
        checkoutButton.onClick.AddListener(Checkout);

        // 为每个移除商品按钮添加点击事件
        for (int i = 0; i < removeButtons.Length; i++)
        {
            int index = i; // 保存当前按钮的索引
            removeButtons[i].onClick.AddListener(() => RemoveFromCart(index));
        }

        // 为移除工人按钮添加点击事件
        removeWorkerButton.onClick.AddListener(RemoveWorkerFromCart);
    }

    // 更新UI显示
    private void UpdateUI()
    {
        moneyText.text = "Money   " + playerMoney;
        itemCountText.text = "ItemCount";
        for (int i = 0; i < itemCounts.Length; i++)
        {
            itemCountText.text += "\nProduct" + (i + 1) + " x" + itemCounts[i];
        }
        workerCountText.text = "WorkerCount x" + workerCount; // 更新工人数量显示
        cartText.text = "CartList";
        foreach (var item in cartItems)
        {
            cartText.text += "\nProduct" + (item.Key + 1) + " x " + item.Value;
        }
        if (cartWorkerCount != 0)
        {
            cartText.text += "\nWorker x " + cartWorkerCount;
        }
    }

    // 计算购物车总金额
    private int CalculateTotalCost()
    {
        int totalCost = 0;
        foreach (var item in cartItems)
        {
            totalCost += itemPrices[item.Key] * item.Value;
        }
        // 只有当购物车中的工人数量为正时，才计算工人价格
        if (cartWorkerCount > 0)
        {
            totalCost += cartWorkerCount * workerPrice;
        }
        return totalCost;
    }

    // 添加商品到购物车
    private void AddToCart(int index)
    {
        int currentCost = CalculateTotalCost();
        int newCost = currentCost + itemPrices[index];

        if (newCost > playerMoney)
        {
            Debug.Log("金额不足！");
            return;
        }

        if (cartItems.ContainsKey(index))
        {
            cartItems[index]++;
        }
        else
        {
            cartItems[index] = 1;
        }
        UpdateUI();
    }

    // 添加工人到购物车
    private void AddWorkerToCart()
    {
        int currentCost = CalculateTotalCost();
        int newCost = currentCost + workerPrice;

        if (newCost > playerMoney)
        {
            Debug.Log("金额不足！");
            return;
        }

        cartWorkerCount++;
        UpdateUI();
    }

    // 从购物车中移除商品
    private void RemoveFromCart(int index)
    {
        if (cartItems.ContainsKey(index) && cartItems[index] > 0)
        {
            cartItems[index]--;
            if (cartItems[index] == 0)
            {
                cartItems.Remove(index);
            }
            UpdateUI();
        }
    }

    // 从购物车中移除工人
    private void RemoveWorkerFromCart()
    {
        if (workerCount + cartWorkerCount > 0)
        {
            cartWorkerCount--;
            UpdateUI();
        }
        else
        {
            Debug.Log("没有足够的工人可以移除！");
        }
    }

    // 结算购物车
    private void Checkout()
    {
        int totalCost = CalculateTotalCost();

        if (playerMoney >= totalCost)
        {
            // 扣除总价格
            playerMoney -= totalCost;

            // 更新已购商品和工人数量
            foreach (var item in cartItems)
            {
                itemCounts[item.Key] += item.Value;
            }
            workerCount += cartWorkerCount;

            // 清空购物车
            cartItems.Clear();
            cartWorkerCount = 0;

            // 更新矩形区域内的工人数量
            UpdateWorkersInArea();

            // 更新UI显示
            UpdateUI();
        }
        else
        {
            Debug.Log("金额不足！");
        }
    }

    // 更新矩形区域内的工人数量
    private void UpdateWorkersInArea()
    {
        // 清除现有的工人Prefab
        foreach (Transform child in workerParent)
        {
            Destroy(child.gameObject);
        }

        // 重新生成工人Prefab
        for (int i = 0; i < workerCount; i++)
        {
            Bounds bounds = areaCollider.bounds;
            Vector3 randomPosition = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x), // x方向随机范围
                Random.Range(bounds.min.y, bounds.max.y) // y方向随机范围
            );
            Instantiate(workerPrefab, randomPosition, Quaternion.identity, workerParent);
        }
    }
}
