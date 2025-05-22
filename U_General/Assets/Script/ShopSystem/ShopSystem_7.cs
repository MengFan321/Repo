using System.Collections;
using System.Collections.Generic;
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

    public Transform itemParent; // ��Ʒ������������֯��Ʒ����
    public BoxCollider2D itemAreaCollider; // ��Ʒ�����BoxCollider2D

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
        new Item { id = 7, name = "�۱�", cost = 5, sellPrice = 5, prefab = null }
    };

    // ��ǰ��ʾ����Ʒ
    private List<Item> currentItems = new List<Item>();

    // ���ڼ�¼ÿ����ƷPrefab������
    private Dictionary<int, List<GameObject>> itemPrefabsInScene = new Dictionary<int, List<GameObject>>();

    void Start()
    {
        UpdateUI();

        // ���ѡ��6����Ʒ
        SelectRandomItems();
        // Ϊÿ����Ʒ��ť��ӵ���¼�
        for (int i = 0; i < buyButtons.Length; i++)
        {
            int index = i; // ���浱ǰ��ť������
            buyButtons[i].onClick.AddListener(() => AddToCart(index));
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
    }

    private void UpdateUI()
    {
        moneyText.text = $"���: {playerMoney}";
        workerCountText.text = $"Ա��: {workerCount}";

        // ���¹��ﳵ����
        cartText.text = "���ﳵ:\n";
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
            cartText.text += $"Ա��: {cartWorkerCount}\n";
        }
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
        }
        else
        {
            Debug.Log("û���㹻�Ĺ��˿����Ƴ���");
        }
    }

    private GameObject SpawnItemPrefab(GameObject prefab)
    {
        Bounds bounds = itemAreaCollider.bounds;
        Vector3 randomPosition = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x), // x���������Χ
            Random.Range(bounds.min.y, bounds.max.y) // y���������Χ
        );
        return Instantiate(prefab, randomPosition, Quaternion.identity, itemParent);
    }

    private void SpawnWorkerPrefab()
    {
        Bounds bounds = workerAreaCollider.bounds;
        Vector3 randomPosition = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x), // x���������Χ
            Random.Range(bounds.min.y, bounds.max.y) // y���������Χ
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
    //            // �����ѹ���Ʒ����
    //            Item itemInAllItems = allItems.Find(i => i.id == item.Key);
    //            if (itemInAllItems != null)
    //            {
    //                itemInAllItems.cost = item.Value; // ���������Ҫ�޸��߼�
    //            }
    //        }

    //        // ��չ��ﳵ
    //        cartItems.Clear();
    //        cartWorkerCount = 0;
    //        TotalSpent = 0;

    //        // ���������ƷPrefab
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
    //        Debug.Log("���㣡");
    //    }
    //}

    public void AutoCheckout()
    {
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
            currentItems.Add(tempItems[randomIndex]); // ֱ���������
            tempItems.RemoveAt(randomIndex);
        }

        // ������Ʒ��ť����ʾ
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
                itemTexts[i].text = ""; // ����ı�����
            }
        }
    }
}