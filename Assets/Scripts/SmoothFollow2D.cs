using UnityEngine;
using System.Collections;

public class SmoothFollow2D : MonoBehaviour
{
    public Transform fixedTarget;
    public Transform floatTarget;
    public float smoothTime = 0.3f;

    private bool gameOver;
    private Transform thisTransform;
    private Vector3 velocity;
    private Transform lowTarget;
    private PlayerMove move;
    private Vector3 moveDir;
    private Transform target;

    public void toFloatTarget() //clip Playing
    {
        target = floatTarget;
    }

    public void toFixedTarget()
    {
        target = fixedTarget;
    }

    void Start()
    {
        target = fixedTarget;
        lowTarget = GameObject.Find("lowCamPos").transform;
        thisTransform = transform;

        move = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
    }

    void Update()
    {
        if (!gameOver)
        {
            moveDir = Vector3.zero;

            if (move.pers == PlayerMove.Pers.gera)
                setHighPos();
            else
                setLowPos();

            thisTransform.position = moveDir;
        }
        else
        {
            moveDir.y = Mathf.SmoothDamp(thisTransform.position.y,
            lowTarget.position.y + 2f, ref velocity.y, smoothTime);

            thisTransform.position = moveDir;
        }
    }

    void setHighPos()//gera
    {
        if (move.StatePoint == 0)
        {
            moveDir.x = Mathf.SmoothDamp(thisTransform.position.x,
                target.position.x - 1, ref velocity.x, smoothTime);
        }
        else if (move.StatePoint == 3 || (move.StatePoint == 2 && move.inTheForest))
        {
            moveDir.x = Mathf.SmoothDamp(thisTransform.position.x,
                target.position.x + 1, ref velocity.x, smoothTime);
        }
        else
        {
            moveDir.x = Mathf.SmoothDamp(thisTransform.position.x,
                target.position.x, ref velocity.x, smoothTime);
        }

        if (target.position.y > 3f && target.position.y < 15)//высокий прыжок
        {
            moveDir.y = Mathf.SmoothDamp(thisTransform.position.y,
                target.position.y + 2, ref velocity.y, smoothTime * 2);
        }
        else if(target.position.y > 15)//полет
        {
            moveDir.y = Mathf.SmoothDamp(thisTransform.position.y,
                target.position.y + 3, ref velocity.y, smoothTime * 2);
        }
        else if (target.position.y < 0)//плывет
        {
            moveDir.y = Mathf.SmoothDamp(thisTransform.position.y,
                target.position.y + 2, ref velocity.y, smoothTime * 2);
        }
        else
        {
            moveDir.y = Mathf.SmoothDamp(thisTransform.position.y,
                lowTarget.position.y + 4.5f, ref velocity.y, smoothTime);
        }
        moveDir.z = Mathf.SmoothDamp(thisTransform.position.z,
            target.position.z - 3, ref velocity.z, smoothTime);
    }

    void setLowPos()//ball, rabbit
    {
        if (move.StatePoint == 0)
        {
            moveDir.x = Mathf.SmoothDamp(thisTransform.position.x,
                target.position.x - 1, ref velocity.x, smoothTime);
        }
        else if (move.StatePoint == 3 || (move.StatePoint == 2 && move.inTheForest))
        {
            moveDir.x = Mathf.SmoothDamp(thisTransform.position.x,
                target.position.x + 1, ref velocity.x, smoothTime);
        }
        else
        {
            moveDir.x = Mathf.SmoothDamp(thisTransform.position.x,
                target.position.x, ref velocity.x, smoothTime);
        }

        if (target.position.y > 3f && target.position.y < 15) //высокий прыжок
        {
            moveDir.y = Mathf.SmoothDamp(thisTransform.position.y,
                target.position.y + 2, ref velocity.y, smoothTime * 2);
        }
        else if (target.position.y > 15)//полет
        {
            moveDir.y = Mathf.SmoothDamp(thisTransform.position.y,
                target.position.y + 3, ref velocity.y, smoothTime * 2);
        }
        else if (target.position.y < 0)//плывет
        {
            moveDir.y = Mathf.SmoothDamp(thisTransform.position.y,
                target.position.y + 2f, ref velocity.y, smoothTime * 2);
        }
        else
        {
            moveDir.y = Mathf.SmoothDamp(thisTransform.position.y,
                lowTarget.position.y + 4, ref velocity.y, smoothTime);
        }
        moveDir.z = Mathf.SmoothDamp(thisTransform.position.z,
            target.position.z - 2, ref velocity.z, smoothTime);
    }

    void GameOver()
    {
        //this.transform.Translate(Vector3(0,-3,0));
        gameOver = true;
    }
}