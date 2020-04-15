using UnityEngine;
using System.Collections;

#if UNITY_ANDROID
using TapjoyUnity;
#endif

public class gTapjoy : MonoBehaviour
{
#if UNITY_ANDROID
    [System.NonSerialized]
    public TJPlacement onRecord;
    [System.NonSerialized]
    public TJPlacement gameOver;
    [System.NonSerialized]
    public TJPlacement onPause;

    // Use this for initialization
    void Start()
    {
        // Preload direct play event
        gameOver = TJPlacement.CreatePlacement("GameOver");
        onPause = TJPlacement.CreatePlacement("OnPause");
        onRecord = TJPlacement.CreatePlacement("OnRecord");

        TJPlacement[] placements = { gameOver, onRecord, onPause };
        for (int i = 0; i < placements.Length; i++)
        {
            if (placements[i] != null)
            {
                placements[i].RequestContent();
            }
        }
    }

    public void ShowOnRecordPlacement()
    {
        if (onRecord.IsContentReady())
            onRecord.ShowContent();
    }

    public void ShowGameOverPlacement()
    {
        if (gameOver.IsContentReady())
        {
            gameOver.ShowContent();
            gameOver.RequestContent();
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
#endif
}
