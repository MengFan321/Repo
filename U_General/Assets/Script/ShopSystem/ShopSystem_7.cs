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

    // �����豸��Ʒ
    public Item deviceItem = new Item
    {
        id = 999, // ����ID
        name = "�豸",
        cost = 300, // ��ʼ����۸�
        sellPrice = 0, // �豸�������������
        prefab = null // �豸����ҪPrefab
    };

    private int deviceLevel = 0; // �豸�ȼ������ڿ��ƹ���
    private const int devicePriceLevel1 = 300; // ��һ�ι���۸�
    private const int devicePriceLevel2 = 600; // �ڶ��ι���۸�
    private const int devicePriceLevel3 = 900; // �ڶ��ι���۸�

    public Button deviceButton; // �豸����ť
    public TextMeshProUGUI deviceText; // �豸��ť�ϵ��ı�

    public TextMeshProUGUI lowestProfitText; // ������ʾ���������Ʒ��TextMeshProUGUI

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

        // ��ʼ���豸��ť
        deviceButton.onClick.AddListener(BuyDevice);
        UpdateDeviceButton();
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

        UpdateDeviceButton();
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
        // �����豸���ܳɱ�
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

    private void BuyDevice()
    {
        if (playerMoney >= GetCurrentDevicePrice())
        {
            playerMoney -= GetCurrentDevicePrice();
            deviceLevel++;

            // �����ܳɱ������ݵ�ǰ�豸�ȼ���������Ľ��
            switch (deviceLevel)
            {
                case 1:
                    TotalSpent += devicePriceLevel1; // ��һ�ι��򣬼���300Ԫ
                    break;
                case 2:
                    TotalSpent += devicePriceLevel2; // �ڶ��ι��򣬼���600Ԫ
                    break;
                case 3:
                    TotalSpent += devicePriceLevel3; // �����ι��򣬼���900Ԫ
                    break;
            }

            // �����豸״̬
            UpdateDeviceButton();

            // ��ʾ���������Ʒ
            ShowLowestProfitItems();

            UpdateUI();
        }
        else
        {
            Debug.Log("���㣡");
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
                return 0; // ����豸�ȼ�����3������0�������ϲ��ᷢ����
        }
    }

    private void UpdateDeviceButton()
    {
        if (deviceLevel == 0)
        {
            deviceText.text = $"�豸\nCost: {devicePriceLevel1}";
        }
        else if (deviceLevel == 1)
        {
            deviceText.text = $"�豸\nCost: {devicePriceLevel2}";
        }
        else if (deviceLevel == 2)
        {
            deviceText.text = $"�豸\nCost: {devicePriceLevel3}";
        }
        else
        {
            deviceButton.gameObject.SetActive(false); // ���ذ�ť
            deviceText.text = "�豸�ѹ���";
        }
    }

    private void ShowLowestProfitItems()
    {
        // ����������Ʒ������
        List<Item> sortedItems = allItems.OrderBy(item => item.sellPrice - item.cost).ToList();

        // �����豸�ȼ���ʾ���������Ʒ
        int count = Mathf.Min(deviceLevel, 3); // ��һ�ι�����ʾ1�����ڶ��ι�����ʾ2���������ι�����ʾ3��
        string message = "���������Ʒ:\n";
        for (int i = 0; i < count; i++)
        {
            if (i < sortedItems.Count) // ȷ�����ᳬ����Ʒ�б�ķ�Χ
            {
                message += $"{sortedItems[i].name} (����: {sortedItems[i].sellPrice - sortedItems[i].cost})\n";
            }
        }

        // ����TextMeshProUGUI���ı�����
        lowestProfitText.text = message;
    }
}