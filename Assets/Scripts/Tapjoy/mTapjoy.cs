using UnityEngine;
using System.Collections;
using TapjoyUnity;

public class mTapjoy : MonoBehaviour {

    public MenuManager menuManager;

    public TJPlacement video;
    public TJPlacement gameLaunch;
    public TJPlacement onPause;
    public TJPlacement offerwall;

    public Transform loadingSprite;
    public UISprite loadingSpriteUI;
    public UILabel uiVideoLabel;

    void OnEnable()
    {
        // Offers Delegates
        Tapjoy.OnOffersResponse += HandleShowOffers;

        // Placement Delegates
        TJPlacement.OnRequestSuccess += HandlePlacementRequestSuccess;
        TJPlacement.OnRequestFailure += HandlePlacementRequestFailure;
        TJPlacement.OnContentReady += HandlePlacementContentReady;
        TJPlacement.OnContentShow += HandlePlacementContentShow;
        TJPlacement.OnContentDismiss += HandlePlacementContentDismiss;
        //TJPlacement.OnPurchaseRequest += HandleOnPurchaseRequest;
        //TJPlacement.OnRewardRequest += HandleOnRewardRequest;

        // Tapjoy Video Delegates
        //Tapjoy.OnVideoStart += HandleVideoStart;
        Tapjoy.OnVideoError += HandleVideoError;

        // Preload direct play event
        video = TJPlacement.CreatePlacement("Video");
        gameLaunch = TJPlacement.CreatePlacement("GameLaunch");
        onPause = TJPlacement.CreatePlacement("OnPause");
        offerwall = TJPlacement.CreatePlacement("Offerwall");

        TJPlacement[] placements = {video, gameLaunch, onPause, offerwall};
        for (int i = 0; i < placements.Length; i++)
        {
            if (placements[i] != null)
            {
                placements[i].RequestContent();
            }
        }

        Tapjoy.GetCurrencyBalance();
        //Tapjoy.
        //ShowGameLaunchPlacement();
    }

    public void Update()
    {
        loadingSprite.Rotate(new Vector3(0, 0, -Time.deltaTime * 200));
    }

    public void HandleShowOffers()
    {

    }

#region Placement Delegate Handlers
    public void HandlePlacementRequestSuccess(TJPlacement placement)
    {
        if (placement.GetName() == "Video")
        {
            if (placement.IsContentAvailable())
            {
                uiVideoLabel.text = "Смотреть видео\n+200 руб";
            }
            else
            {
                uiVideoLabel.text = "Нет контента\n(видео)";
            }
            loadingSpriteUI.enabled = false;
        }
    }

    public void HandlePlacementRequestFailure(TJPlacement placement, string error)
    {
        if (placement.GetName() == "Video")
        {
            uiVideoLabel.text = "Ошибка запроса\n(видео)";
            loadingSpriteUI.enabled = false;
        }
    }

    public void HandlePlacementContentReady(TJPlacement placement)
    {
        if (placement.GetName() == "Video")
        {
            if (placement.IsContentAvailable())
            {
                uiVideoLabel.text = "Видео(загружено)\n+200 руб";
            }
            else
            {
                uiVideoLabel.text = "Нет контента\n(видео)";
            }
            loadingSpriteUI.enabled = false;
        }
    }

    public void HandlePlacementContentShow(TJPlacement placement)
    {
        if (placement.GetName() == "Video")
        {
            loadingSpriteUI.enabled = false;
        }
    }

    public void HandlePlacementContentDismiss(TJPlacement placement)
    {
        if (placement.GetName() == "Video")
        {
            uiVideoLabel.text = "Контент не загружен\n(видео)";
            loadingSpriteUI.enabled = false;
        }
    }

    //void HandleOnPurchaseRequest(TJPlacement placement, TJActionRequest request, string productId)
    //{
    //    Debug.Log("C#: HandleOnPurchaseRequest");
    //    request.Completed();
    //}

    void HandleOnRewardRequest(TJPlacement placement, TJActionRequest request, string itemId, int quantity)
    {
    //    Debug.Log("C#: HandleOnRewardRequest");
        menuManager.changeMoney(quantity, false);
        request.Completed();
    }

#endregion

#region Video Delegate Handlers
    //public void HandleVideoStart()
    //{
        
    //}

    public void HandleVideoError(string status)
    {
        uiVideoLabel.text = "Ошибка";
    }
#endregion
#region Placement Showing
    public void ShowVideo()
    {
        if (video.IsContentReady())
        {
            video.ShowContent();
            video.RequestContent();
            loadingSpriteUI.enabled = true;
        }
    }

    public void ShowOffers()
    {
        if (offerwall.IsContentReady())
            offerwall.ShowContent();
    }

    public void ShowGameLaunchPlacement()
    {
        if (gameLaunch.IsContentReady())
        {
            gameLaunch.ShowContent();
            if (Random.Range(0, 5) == 1)
                gameLaunch.RequestContent();
        }
    }

    public void ShowGameExitAds()
    {
        if (gameLaunch.IsContentReady())
        {
            gameLaunch.ShowContent();
        }
        else
        {
            Application.Quit();
        }
    }

    public void ShowOnPausePlacement()
    {
        if (onPause.IsContentReady())
        {
            onPause.ShowContent();
            onPause.RequestContent();
        }
    }
#endregion
}
