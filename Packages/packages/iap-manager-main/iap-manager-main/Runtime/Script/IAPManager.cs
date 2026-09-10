using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NabaGame.Core.Runtime.EventManager;
using NabaGame.Core.Runtime.Singleton;
using NabaGame.Tracking;
using UnityEngine;
using UnityEngine.Purchasing;

[Serializable]
public class IAPInfo
{
    public string productId;
    public float amout;
    public string priceLocal;
}

[Singleton("IAPManager", true)]
public class IAPManager : Singleton<IAPManager>
{
    public List<IAPInfo> PackageID = new List<IAPInfo>();
    private IAPPurchaseSuccessEvent _purchaseSuccessEvent = new IAPPurchaseSuccessEvent(0);
    private IAPInfo _iapInfo;
    private Action<bool> m_Completed;
    StoreController m_StoreController;
    bool isInited = false;

    public override void Init()
    {
        InitializeIAP();
    }

    async void InitializeIAP()
    {
        m_StoreController = UnityIAPServices.StoreController();
        m_StoreController.OnPurchasePending += OnPurchasePending;
        m_StoreController.OnPurchaseConfirmed += OnPurchaseConfirmed;
        m_StoreController.OnProductsFetched += OnProductFetched;
        await m_StoreController.Connect();
        isInited = true;
    }

    public void InitializePurchasing(List<IAPInfo> PackageID)
    {
        this.PackageID = PackageID;
        StartCoroutine(Waite());

        IEnumerator Waite()
        {
            yield return new WaitUntil(() => isInited);
            var initialProductsToFetch = new List<ProductDefinition>();
            for (int i = 0; i < PackageID.Count; i++)
            {
                initialProductsToFetch.Add(new ProductDefinition(PackageID[i].productId, ProductType.Consumable));
            }

            m_StoreController.FetchProducts(initialProductsToFetch);
        }
    }

    private void OnProductFetched(List<Product> obj)
    {
        for (int i = 0; i < PackageID.Count; i++)
        {
            var product = obj.Find(x => x.definition.id == PackageID[i].productId);
            PackageID[i].priceLocal = product.metadata.localizedPriceString;
        }
    }

    public string GetPriceLocal(string productId)
    {
        var product = PackageID.Find(x => x.productId == productId);
        if (product == null)
            return "0";
        return product.priceLocal;
    }

    public void InitiatePurchase(string productId, Action<bool> callback)
    {
        this.m_Completed = callback;
        _iapInfo = PackageID.Find(x => x.productId == productId);
        _purchaseSuccessEvent.amout = _iapInfo.amout;
#if UNITY_EDITOR
        callback?.Invoke(true);
        return;
#endif
        var product = m_StoreController?.GetProducts().FirstOrDefault(product => product.definition.id == productId);
        if (product != null)
        {
            m_StoreController?.PurchaseProduct(product);
        }
        else
        {
            callback?.Invoke(false);
        }
    }

    void OnPurchasePending(PendingOrder order)
    {
        m_StoreController.ConfirmPurchase(order);
    }

    void OnPurchaseConfirmed(Order order)
    {
        switch (order)
        {
            case FailedOrder:
                m_Completed?.Invoke(false);
                break;
            case ConfirmedOrder:
                m_Completed?.Invoke(true);
                EventManager.Instance.Raise(_purchaseSuccessEvent);
                TrackingManager.TrackEvent("Purchased", "ID", _iapInfo.productId, "amout", _iapInfo.amout.ToString());
                break;
        }
    }
}