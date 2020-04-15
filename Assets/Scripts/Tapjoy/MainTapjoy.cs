using UnityEngine;
using System.Collections;
using TapjoyUnity;

public class MainTapjoy : MonoBehaviour
{
#if UNITY_ANDROID
    public UILabel[] tapjoyLabels;

    public mTapjoy tapjoy;
    public myOpenIABscript IAB;
    public MenuManager menuManager;

	void Start ()
    {
        if (Tapjoy.IsConnected) tapjoy.enabled = true;

        // Connect Delegates
        Tapjoy.OnConnectSuccess += HandleConnectSuccess;
        Tapjoy.OnConnectFailure += HandleConnectFailure;

        // Offers Delegates
        Tapjoy.OnOffersResponseFailure += HandleShowOffersFailure;

        // Tapjoy Video Delegates
        Tapjoy.OnVideoComplete += HandleVideoComplete;

        // Currency Delegates
        Tapjoy.OnAwardCurrencyResponse += HandleAwardCurrencyResponse;
        Tapjoy.OnAwardCurrencyResponseFailure += HandleAwardCurrencyResponseFailure;
        Tapjoy.OnSpendCurrencyResponse += HandleSpendCurrencyResponse;
        Tapjoy.OnSpendCurrencyResponseFailure += HandleSpendCurrencyResponseFailure;
        Tapjoy.OnGetCurrencyBalanceResponse += HandleGetCurrencyBalanceResponse;
        Tapjoy.OnGetCurrencyBalanceResponseFailure += HandleGetCurrencyBalanceResponseFailure;
        Tapjoy.OnEarnedCurrency += HandleEarnedCurrency;
    }

    public void HandleConnectSuccess()
    {
        tapjoy.enabled = true;

        tapjoyLabels[0].text = "Предложения\n(offers)";
        tapjoyLabels[1].text = "Смотреть видео\n+200 руб";
    }

    public void HandleConnectFailure()
    {
        tapjoy.enabled = false;

        tapjoyLabels[0].text = "Ошибка\n(offers)";
        tapjoyLabels[0].text = "Ошибка\n";
    }

    public void HandleShowOffersFailure(string error)
    {
        tapjoyLabels[0].text = "Ошибка\n(offers)";
    }

    #region Currency Delegate Handlers
    public void HandleAwardCurrencyResponse(string currencyName, int balance)
    {
        //Debug.Log("C#: HandleAwardCurrencySucceeded: currencyName: " + currencyName + ", balance: " + balance);
    }

    public void HandleAwardCurrencyResponseFailure(string error)
    {
        //Debug.Log("C#: HandleAwardCurrencyResponseFailure: " + error);
    }

    public void HandleGetCurrencyBalanceResponse(string currencyName, int balance)
    {
        int money = menuManager.getMoney();
        if (balance > money && money == 0)
            menuManager.changeMoney(balance, false);
        //Debug.Log("C#: HandleGetCurrencyBalanceResponse: currencyName: " + currencyName + ", balance: " + balance);
    }

    public void HandleGetCurrencyBalanceResponseFailure(string error)
    {
        //Debug.Log("C#: HandleGetCurrencyBalanceResponseFailure: " + error);
    }

    public void HandleSpendCurrencyResponse(string currencyName, int balance)
    {
        //Debug.Log("C#: HandleSpendCurrencyResponse: currencyName: " + currencyName + ", balance: " + balance);
    }

    public void HandleSpendCurrencyResponseFailure(string error)
    {
        //Debug.Log("C#: HandleSpendCurrencyResponseFailure: " + error);
    }

    public void HandleEarnedCurrency(string currencyName, int amount)
    {
        //Debug.Log("C#: HandleEarnedCurrency: currencyName: " + currencyName + ", amount: " + amount);
        menuManager.changeMoney(amount, false);
        Tapjoy.ShowDefaultEarnedCurrencyAlert();
    }
    #endregion

    public void ShowOffers()
    {
        if (Tapjoy.IsConnected)
            Tapjoy.ShowOffers();
        else
            tapjoyLabels[0].text = "Пока недоступно\n или нет интернета";
    }

    public void ShowVideo()
    {
        if (Tapjoy.IsConnected)
            tapjoy.ShowVideo();
        else
            tapjoyLabels[1].text = "Пока недоступно\n или нет интернета";
    }

    public void Donate()
    {
        if (!IAB.Donate())
            tapjoyLabels[2].text = "Пока недоступно\n или нет интернета";
    }

    public void BillingInitalized()
    {
        tapjoyLabels[2].text = "Купить за 1$\n+30 000 руб";
    }

    public void HandleBillingNotSupportedEvent(string error)
    {
        tapjoyLabels[2].transform.localScale /= 1.7f;
        tapjoyLabels[2].text = error;//"Покупки\n не поддерживаются";
    }

    public void HandleDonateSuccess()//G.S.A.!
    {
        menuManager.changeMoney(30000, true);
        //Tapjoy.AwardCurrency(5000);
    }

    public void HandleVideoComplete()
    {
        menuManager.changeMoney(100, true);
    }

    public void ChangeMoney(int val)
    {
        if (Tapjoy.IsConnected)
        {
            if (val > 0)
                Tapjoy.AwardCurrency(val);
            else if (val != 0)
                Tapjoy.SpendCurrency(val);
        }
    }
#endif
}
