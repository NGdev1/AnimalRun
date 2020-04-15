using UnityEngine;
using System.Collections;

public class PlayerBonuses : MonoBehaviour
{
    public struct Bonus
    {
        public float mTime;
        public float duration;
        public bool actived;
    }
    //0 - botinok
    //1 - galosha
    //2 - nonStop
    //3 - glider
    public GameObject gliderPrefab;
    public string gliderAttachmentPath;

    [System.NonSerialized]
    public Bonus[] bonuses;
    private UISprite bonusSprite;
    private UISlider shkala;
    private float progress;
    private float lastTimeScale;
    private int lastId;
    private GameObject glider;
    private PlayerMove playerMove;

    void Awake()
    {
        bonusSprite = GameObject.Find("UI Root (2D)/Camera/GamePanel/AnchorBottomL/BonusSprite").GetComponent<UISprite>();
        shkala = GameObject.Find("UI Root (2D)/Camera/GamePanel/AnchorBottomL/BonusSlider").GetComponent<UISlider>();
        playerMove = gameObject.GetComponent<PlayerMove>();
        bonusSprite.alpha = 0;
        for (int i = 0; i < 2; i++)
        {
            shkala.transform.GetChild(i).GetComponent<UISprite>().alpha = 0;
        }

        bonuses = new Bonus[4];
        for (int i = 0; i < bonuses.Length; i++)
        {
            bonuses[i] = new Bonus();
            bonuses[i].actived = false;
            bonuses[i].mTime = 0;
        }

        progress = 0; lastId = 0;
    }

    void Update()
    {
        if (bonuses[0].actived)
        {
            if (bonuses[0].mTime < bonuses[0].duration)
            {
                bonuses[0].mTime += Time.deltaTime;
            }
            else
            {
                jumpBunusDeactive();
                CalculateLastId();
            }
        }
        else if (bonuses[1].actived)
        {
            if (bonuses[1].mTime < bonuses[1].duration)
            {
                bonuses[1].mTime += Time.deltaTime * 2;
            }
            else
            {
                galoshaDeactive();
                CalculateLastId();
            }
        }
        else if (bonuses[2].actived)
        {
            if (bonuses[2].mTime < bonuses[2].duration)
            {
                bonuses[2].mTime += Time.deltaTime;
            }
            else
            {
                NonStopDeactive();
                CalculateLastId();
            }
        }
        else if (bonuses[3].actived)
        {
            if (bonuses[3].mTime < bonuses[3].duration)
            {
                bonuses[3].mTime += Time.deltaTime;
            }
            else
            {
                gliderDeactive();
                CalculateLastId();
            }
        }

        progress = 1 - bonuses[lastId].mTime / bonuses[lastId].duration;
        shkala.sliderValue = progress;
    }

    void CalculateLastId()
    {
        float maxMTime = bonuses[lastId].mTime;
        for(int i = 0; i < bonuses.Length; i++)
        {
            if (bonuses[i].mTime > maxMTime)
            {
                lastId = i;
                maxMTime = bonuses[i].mTime;
            }
        }

        switch (lastId)
        {
            case 0:
                bonusSprite.spriteName = "botinok";
                break;
            case 1:
                bonusSprite.spriteName = "galosha";
                break;
            case 2:
                bonusSprite.spriteName = "nonStop";
                break;
            case 3:
                bonusSprite.spriteName = "glider";
                break;
        }
    }

    void NonStopActive()
    {
        lastId = 2;
        if (!bonuses[lastId].actived)
        {
            if(!bonuses[1].actived)
                lastTimeScale = Time.timeScale;
            Time.timeScale = 2.0f;
            bonuses[lastId].actived = true;

            bonusSprite.alpha = 1;
            bonusSprite.spriteName = "nonStop";
            for (int i = 0; i < 2; i++)
            {
                shkala.transform.GetChild(i).GetComponent<UISprite>().alpha = 1;
            }
        }
        else bonuses[lastId].mTime = 0;
    }

    void NonStopDeactive()
    {
        playerMove.NonStopDeactive(); //PlayerBonuses->PlayerMove->scoreListener
        if (!bonuses[1].actived)
            Time.timeScale = lastTimeScale;

        bonuses[2].mTime = 0;
        bonuses[2].actived = false;

        bonusSprite.alpha = 0;
        for (int i = 0; i < 2; i++)
        {
            shkala.transform.GetChild(i).GetComponent<UISprite>().alpha = 0;
        }
    }

    void galoshaActive()
    {
        lastId = 1;
        if (!bonuses[lastId].actived)
        {
            if (!bonuses[2].actived)
                lastTimeScale = Time.timeScale;
            Time.timeScale = 0.5f;
            bonuses[lastId].actived = true;

            bonusSprite.alpha = 1;
            bonusSprite.spriteName = "galosha";
            for (int i = 0; i < 2; i++)
            {
                shkala.transform.GetChild(i).GetComponent<UISprite>().alpha = 1;
            }
        }
        else bonuses[lastId].mTime = 0;
    }

    void gliderActive()
    {
        lastId = 3;
        if (!bonuses[lastId].actived)
        {
            Vector3 Pos = transform.position;
            Pos.y = 25;
            transform.position = Pos;

            playerMove.Fly();
            glider = NGUITools.AddChild(gameObject.transform.Find(gliderAttachmentPath).gameObject, gliderPrefab);
            bonuses[lastId].actived = true;

            bonusSprite.alpha = 1;
            bonusSprite.spriteName = "glider";
            for (int i = 0; i < 2; i++)
            {
                shkala.transform.GetChild(i).GetComponent<UISprite>().alpha = 1;
            }
        }
        else bonuses[lastId].mTime = 0;
    }

    public void gliderDeactive()
    {
        bonuses[3].mTime = 0;
        playerMove.StopFlying();
        GameObject.Destroy(glider);
        bonuses[3].actived = false;

        bonusSprite.alpha = 0;
        for (int i = 0; i < 2; i++)
        {
            shkala.transform.GetChild(i).GetComponent<UISprite>().alpha = 0;
        }
    }

    void jumpBonusActive()
    {
        lastId = 0;
        if (!bonuses[lastId].actived)
        {
            playerMove.JumpHeight *= 1.4f;
            bonuses[lastId].actived = true;

            bonusSprite.alpha = 1;
            bonusSprite.spriteName = "botinok";
            for (int i = 0; i < 2; i++)
            {
                shkala.transform.GetChild(i).GetComponent<UISprite>().alpha = 1;
            }
        }
        else bonuses[lastId].mTime = 0;
    }

    void jumpBunusDeactive()
    {
        bonuses[0].mTime = 0;
        playerMove.JumpHeight /= 1.4f;
        bonuses[0].actived = false;

        bonusSprite.alpha = 0;
        for (int i = 0; i < 2; i++)
        {
            shkala.transform.GetChild(i).GetComponent<UISprite>().alpha = 0;
        }
    }

    void galoshaDeactive()
    {
        bonuses[1].mTime = 0;
        bonuses[1].actived = false;
        if (!bonuses[2].actived)
            Time.timeScale = lastTimeScale;

        bonusSprite.alpha = 0;
        for (int i = 0; i < 2; i++)
        {
            shkala.transform.GetChild(i).GetComponent<UISprite>().alpha = 0;
        }
    }

    void GameOver()
    {
        for (int i = 0; i < bonuses.Length; i++)
        {
            bonuses[i].actived = false;
        }
    }
}