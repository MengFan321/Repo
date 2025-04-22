using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSystem_3 : MonoBehaviour
{
    // ������UI����
    public TextMeshProUGUI moneyText; // ��ʾ����ʽ��TextMeshProUGUI
    public TextMeshProUGUI itemCountText; // ��ʾ�ѹ���Ʒ������TextMeshProUGUI
    public TextMeshProUGUI workerCountText; // ��ʾ�ѹ�����������TextMeshProUGUI
    public TextMeshProUGUI cartText; // ��ʾ���ﳵ���ݵ�TextMeshProUGUI

    public Button[] buyButtons; // ������Ʒ��ť����
    public Button buyWorkerButton; // �����˵İ�ť
    
    public Button[] removeButtons; // �Ƴ���Ʒ��ť����
    public Button removeWorkerButton; // �Ƴ����˰�ť
    
    

    public Button checkoutButton; // ���㰴ť

    public GameObject workerPrefab; // ����Prefab
    public Transform workerParent; // ���˸����󣨿�ѡ��������֯���˶���
    public BoxCollider2D areaCollider; // ���������BoxCollider2D


    // ˽�б���
    private int playerMoney = 1000; // ��ҳ�ʼ�ʽ�
    private int[] itemPrices = { 20, 40, 80, 100 }; // ÿ����Ʒ�ļ۸�
    private int[] itemCounts = { 0, 0, 0, 0 }; // ÿ����Ʒ���ѹ�����
    private int workerPrice = 70; // �����˵ļ۸�
    private int workerCount = 0; // �ѹ����˵�����

    

    private Dictionary<int, int> cartItems = new Dictionary<int, int>(); // ���ﳵ�е���Ʒ������������
    private int cartWorkerCount = 0; // ���ﳵ�еĹ�������

    void Start()
    {
        // ��ʼ��UI��ʾ
        UpdateUI();

        // Ϊÿ����Ʒ��ť��ӵ���¼�
        for (int i = 0; i < buyButtons.Length; i++)
        {
            int index = i; // ���浱ǰ��ť������
            buyButtons[i].onClick.AddListener(() => AddToCart(index));
        }

        // Ϊ�����˰�ť��ӵ���¼�
        buyWorkerButton.onClick.AddListener(AddWorkerToCart);

        // Ϊ���㰴ť��ӵ���¼�
        checkoutButton.onClick.AddListener(Checkout);

        // Ϊÿ���Ƴ���Ʒ��ť��ӵ���¼�
        for (int i = 0; i < removeButtons.Length; i++)
        {
            int index = i; // ���浱ǰ��ť������
            removeButtons[i].onClick.AddListener(() => RemoveFromCart(index));
        }

        // Ϊ�Ƴ����˰�ť��ӵ���¼�
        removeWorkerButton.onClick.AddListener(RemoveWorkerFromCart);
    }

    // ����UI��ʾ
    private void UpdateUI()
    {
        moneyText.text = "Money   " + playerMoney;
        itemCountText.text = "ItemCount";
        for (int i = 0; i < itemCounts.Length; i++)
        {
            itemCountText.text += "\nProduct" + (i + 1) + " x" + itemCounts[i];
        }
        workerCountText.text = "WorkerCount x" + workerCount; // ���¹���������ʾ
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

    // ���㹺�ﳵ�ܽ��
    private int CalculateTotalCost()
    {
        int totalCost = 0;
        foreach (var item in cartItems)
        {
            totalCost += itemPrices[item.Key] * item.Value;
        }
        // ֻ�е����ﳵ�еĹ�������Ϊ��ʱ���ż��㹤�˼۸�
        if (cartWorkerCount > 0)
        {
            totalCost += cartWorkerCount * workerPrice;
        }
        return totalCost;
    }

    // �����Ʒ�����ﳵ
    private void AddToCart(int index)
    {
        int currentCost = CalculateTotalCost();
        int newCost = currentCost + itemPrices[index];

        if (newCost > playerMoney)
        {
            Debug.Log("���㣡");
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

    // ��ӹ��˵����ﳵ
    private void AddWorkerToCart()
    {
        int currentCost = CalculateTotalCost();
        int newCost = currentCost + workerPrice;

        if (newCost > playerMoney)
        {
            Debug.Log("���㣡");
            return;
        }

        cartWorkerCount++;
        UpdateUI();
    }

    // �ӹ��ﳵ���Ƴ���Ʒ
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

    // �ӹ��ﳵ���Ƴ�����
    private void RemoveWorkerFromCart()
    {
        if (workerCount + cartWorkerCount > 0)
        {
            cartWorkerCount--;
            UpdateUI();
        }
        else
        {
            Debug.Log("û���㹻�Ĺ��˿����Ƴ���");
        }
    }

    // ���㹺�ﳵ
    private void Checkout()
    {
        int totalCost = CalculateTotalCost();

        if (playerMoney >= totalCost)
        {
            // �۳��ܼ۸�
            playerMoney -= totalCost;

            // �����ѹ���Ʒ�͹�������
            foreach (var item in cartItems)
            {
                itemCounts[item.Key] += item.Value;
            }
            workerCount += cartWorkerCount;

            // ��չ��ﳵ
            cartItems.Clear();
            cartWorkerCount = 0;

            // ���¾��������ڵĹ�������
            UpdateWorkersInArea();

            // ����UI��ʾ
            UpdateUI();
        }
        else
        {
            Debug.Log("���㣡");
        }
    }

    // ���¾��������ڵĹ�������
    private void UpdateWorkersInArea()
    {
        // ������еĹ���Prefab
        foreach (Transform child in workerParent)
        {
            Destroy(child.gameObject);
        }

        // �������ɹ���Prefab
        for (int i = 0; i < workerCount; i++)
        {
            Bounds bounds = areaCollider.bounds;
            Vector3 randomPosition = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x), // x���������Χ
                Random.Range(bounds.min.y, bounds.max.y) // y���������Χ
            );
            Instantiate(workerPrefab, randomPosition, Quaternion.identity, workerParent);
        }
    }
}
