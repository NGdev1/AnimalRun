using UnityEngine;
using System.Collections;

public class bonuses : MonoBehaviour
{
    public enum Bonus
    {
        botinok,
        X2,
        carrot,
        nonStop,
        moneta,
        galosha,
        glider
    }

    public Bonus bonusType;

    private Transform myTransform;

    void Awake()
    {
        myTransform = transform;
    }

    void Update()
    {
        myTransform.Rotate(new Vector3(0, 0, Time.deltaTime * 200));
    }

    void OnTriggerEnter(Collider player)
    {
        if (player.tag == "Player")
        {
            if (bonusType == Bonus.botinok)
            {
                player.SendMessage("jumpBonusActive");
            }
            else if(bonusType == Bonus.galosha)
            {
                player.SendMessage("galoshaActive");
            }
            else if(bonusType == Bonus.glider)
            {
                player.SendMessage("gliderActive");
            }
            else if (bonusType == Bonus.nonStop)
            {
                player.SendMessage("NonStopActive");
            }

            Destroy(gameObject);
        }
    }
}