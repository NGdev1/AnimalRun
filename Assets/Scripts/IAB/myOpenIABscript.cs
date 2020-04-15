/*******************************************************************************
 * Copyright 2012-2014 One Platform Foundation
 *
 *       Licensed under the Apache License, Version 2.0 (the "License");
 *       you may not use this file except in compliance with the License.
 *       You may obtain a copy of the License at
 *
 *           http://www.apache.org/licenses/LICENSE-2.0
 *
 *       Unless required by applicable law or agreed to in writing, software
 *       distributed under the License is distributed on an "AS IS" BASIS,
 *       WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *       See the License for the specific language governing permissions and
 *       limitations under the License.
 ******************************************************************************/
#if UNITY_ANDROID

using UnityEngine;
using OnePF;
using System.Collections.Generic;
using Assets.Scripts.Common;

public class myOpenIABscript : MonoBehaviour
{
    const string SKU = "money_5000";
    const string SKU2 = "no_ads";

#pragma warning disable 0414
#pragma warning restore 0414

    bool _isInitialized = false;
    Inventory _inventory = null;

    public MainTapjoy mainTapjoy;
    public MenuManager menuManager;

    private string payload;

    private void OnEnable()
    {
        // Listen to all events for illustration purposes
        OpenIABEventManager.billingSupportedEvent += billingSupportedEvent;
        OpenIABEventManager.billingNotSupportedEvent += billingNotSupportedEvent;
        OpenIABEventManager.queryInventorySucceededEvent += queryInventorySucceededEvent;
        OpenIABEventManager.queryInventoryFailedEvent += queryInventoryFailedEvent;
        OpenIABEventManager.purchaseSucceededEvent += purchaseSucceededEvent;
        OpenIABEventManager.purchaseFailedEvent += purchaseFailedEvent;
        OpenIABEventManager.consumePurchaseSucceededEvent += consumePurchaseSucceededEvent;
        OpenIABEventManager.consumePurchaseFailedEvent += consumePurchaseFailedEvent;
    }
    private void OnDisable()
    {
        // Remove all event handlers
        OpenIABEventManager.billingSupportedEvent -= billingSupportedEvent;
        OpenIABEventManager.billingNotSupportedEvent -= billingNotSupportedEvent;
        OpenIABEventManager.queryInventorySucceededEvent -= queryInventorySucceededEvent;
        OpenIABEventManager.queryInventoryFailedEvent -= queryInventoryFailedEvent;
        OpenIABEventManager.purchaseSucceededEvent -= purchaseSucceededEvent;
        OpenIABEventManager.purchaseFailedEvent -= purchaseFailedEvent;
        OpenIABEventManager.consumePurchaseSucceededEvent -= consumePurchaseSucceededEvent;
        OpenIABEventManager.consumePurchaseFailedEvent -= consumePurchaseFailedEvent;
    }

    void init()
    {
        // Application public key
        var googlePublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAjN/yjT//IzEY3Jl8hClmpsCst+gDXR1GSu1fTJ132Gusw9YJ4h2pSYHkF9HYRlg2MZ9wZCmZ0iedUfw08XSeOdcjVklLexlgJW2FU64y+pHmp9gIxs6nQvvdm4xkC+TT2eTCYG1/rYSwTdM834UgwxGMAcdVK+HykRJrTpR1i0t2w9iiYWYcYnLscw2bGv1BgtFVjLJdrBqelejvqjDq/xRQGx0K5zXhZltEF+fik71dzAZ7GvxT2ckNu3z6BYrNMUznNepW33Hlo4Hk+ZAPeh3o3NG/TZr6lZIii9ZO18emwhYOvwYCMWS1F+8ECMiXVma2R7/0eH1P5ZETOsLcjQIDAQAB";

        var options = new Options();
        //options.checkInventoryTimeoutMs = Options.INVENTORY_CHECK_TIMEOUT_MS * 2;
        //options.discoveryTimeoutMs = Options.DISCOVER_TIMEOUT_MS * 2;
        //options.checkInventory = false;
        //options.verifyMode = OptionsVerifyMode.VERIFY_SKIP;
        options.storeKeys = new Dictionary<string, string> { { OpenIAB_Android.STORE_GOOGLE, googlePublicKey } };
        //options.storeSearchStrategy = SearchStrategy.INSTALLER_THEN_BEST_FIT;
        options.availableStoreNames = new string[] { OpenIAB_Android.STORE_GOOGLE };
        options.prefferedStoreNames = new string[] { OpenIAB_Android.STORE_GOOGLE };
        options.storeSearchStrategy = SearchStrategy.INSTALLER_THEN_BEST_FIT;
        // Transmit options and start the service
        OpenIAB.init(options);

        //query inventory
        //OpenIAB.queryInventory(new string[] { SKU });
    }

    private void Start()
    {
        OpenIAB.mapSku(SKU, OpenIAB_Android.STORE_GOOGLE, SKU);

        init();

        //Consume Product
        //if (_inventory != null && _inventory.HasPurchase(SKU))
            //OpenIAB.consumeProduct(_inventory.GetPurchase(SKU));

        //Test Consume
        //if (_inventory != null && _inventory.HasPurchase("android.test.purchased"))
            //OpenIAB.consumeProduct(_inventory.GetPurchase("android.test.purchased"));

        //Test Item Unavailable
        //OpenIAB.purchaseProduct("android.test.item_unavailable");

        //Test Purchase Canceled
        //OpenIAB.purchaseProduct("android.test.canceled");
    }

    public bool Donate()
    {
        if (_isInitialized)
        {
            string randomString = B64X.GetNewKey() + B64X.GetNewKey();

            //real Purchase
            OpenIAB.purchaseProduct(SKU, randomString);
            //ToDo: save payload on server

            return true;
        }
        else return false;
    }
    
    private void billingSupportedEvent()
    {
        _isInitialized = true;
        mainTapjoy.BillingInitalized();

        OpenIAB.queryInventory(new string[] { SKU });
    }

    private void billingNotSupportedEvent(string error)
    {
        mainTapjoy.HandleBillingNotSupportedEvent(error);
    }

    private void queryInventorySucceededEvent(Inventory inventory)
    {
        if (inventory != null)
        {
            _inventory = inventory;
            Purchase mPurchase = inventory.GetPurchase(SKU);

            string rsaXml = B64X.Decrypt((Resources.Load("RSA") as TextAsset).text, "myRSA!!!xml");
            bool check = GooglePlayPurchaseGuard.Verify(mPurchase.OriginalJson, mPurchase.Signature, rsaXml);

            if (!check) print("incorrect");

            bool payed = (mPurchase != null && VerifyDeveloperPayload(mPurchase.DeveloperPayload) && check && mPurchase.PurchaseState == 0);

            string key = B64X.GetNewKey();
            menuManager.payed = B64X.Encrypt(payed.ToString(), key);
            menuManager.key2 = key;
        }
        else
            print("Debil!");
    }

    private bool VerifyDeveloperPayload(string _payload)
    {
        //ToDo: проверка payload на стороннем сервере
        return true;
    }
    
    private void queryInventoryFailedEvent(string error)
    {

    }

    private void purchaseSucceededEvent(Purchase purchase)
    {
        mainTapjoy.HandleDonateSuccess();
    }

    private void purchaseFailedEvent(int errorCode, string errorMessage)
    {

    }

    private void consumePurchaseSucceededEvent(Purchase purchase)
    {
        mainTapjoy.HandleDonateSuccess();
    }

    private void consumePurchaseFailedEvent(string error)
    {

    }
}

#endif