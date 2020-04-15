using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerMove : MonoBehaviour
{
    enum eState
    {
        Stay,
        WalkLeft,
        WalkRight,
    };

    public enum Pers
    {
        rabbit,
        ball,
        gera,
        kuritsa,
        dog,
        bear,
        koza
    };

    public float RoadHeight;
    public float WalkSpeed;
    public float JumpHeight;
    public float JumpSpeed;
    public float FallSpeed;
    public float speed;
    public float gravity;
    public Transform myCamera;
    public float[] spawnPoints;
    public Transform cameraPos;
    public Animation anim;
    public AnimationClip[] animations;
    public AnimationClip[] animationsJump;
    public Pers pers;
    public SmoothFollow2D smoothFollow2D;
    public GameObject BottleOrEgg;

    [System.NonSerialized]
    public bool FPS;    //camera state
    [System.NonSerialized]
    public bool jetpack;
    [System.NonSerialized]
    public int StatePoint;
    [System.NonSerialized]
    public bool inTheForest;
    [System.NonSerialized]
    public bool jump;
    [System.NonSerialized]
    public int lives;

    private eState State;
    private eState rotState;
    private Transform player;
    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;
    private float CFallSpeed;
    private bool gameOver;
    private bool nonStopActived;
    private float[] fullSpawnPoints;
    private CarSpawner carSp;
    private AreaSpawner areaSp;
    private Vector3 nextRotation;
    private Transform pivot;
    private Transform sky;
    private ScoreListener sc;
    private bool Touched;
    private Vector2 delta;
    private int screenCenter;
    private DuckSpawner duckSpawner;

    //----------------------- Swim variables-------------------------
    public GameObject boatPrefab;
    public AnimationClip swimAnimation;
    public AnimationClip fromWaterToRoadAnim;
    public AnimationClip fromRoadToWaterAnim;

    private GameObject boat;
    private bool inTheWater;

    void MyAwake()
    {
        Touched         = false;
		jetpack         = false;
        gameOver        = false;
        jump            = false;
        nonStopActived  = false;
        inTheWater      = false;

        //Resources.UnloadUnusedAssets();

        carSp = (CarSpawner)GameObject.FindObjectOfType(typeof(CarSpawner));
        areaSp = (AreaSpawner)GameObject.FindObjectOfType(typeof(AreaSpawner));
        duckSpawner = (DuckSpawner)GameObject.FindObjectOfType(typeof(DuckSpawner));
        sc = ScoreListener.getInstance();
        controller = gameObject.GetComponent<CharacterController>();

        screenCenter = Screen.width / 2;
        ChangeCam(); ChangeCam();

        pivot = GameObject.Find("pivot").transform;
        sky = GameObject.Find("sky").transform;
        player = transform;
        State = eState.Stay;
        StatePoint = Random.Range(0, spawnPoints.Length);
        delta = Vector2.zero;
        CFallSpeed = 0;
        lives = 0;

        Vector3 playerpos = player.position;
        playerpos.x = spawnPoints[StatePoint];
        player.transform.position = playerpos;
		sky.position = playerpos;

        if (pers != Pers.rabbit) carSp.bonusPrefab[1] = BottleOrEgg;
        
        if(pers == Pers.rabbit || pers == Pers.dog)
        {
            carSp.bonusPrefab[6] = carSp.bonusPrefab[0];
        }
    }

    public void StartFromForest()
    {
        fullSpawnPoints = (float[])spawnPoints.Clone();

        spawnPoints = new float[fullSpawnPoints.Length - 1];
        for(int i = 0; i < fullSpawnPoints.Length - 1; i++)
        {
            spawnPoints[i] = fullSpawnPoints[i];
        }

        inTheForest = true;

        MyAwake();
    }

    public void StartFromRoute()
    {
        fullSpawnPoints = (float[])spawnPoints.Clone();

        inTheForest = false;

        MyAwake();
    }

    // Update is called once per frame		
    void Update()
    {
        if (!gameOver)
        {
            Vector3 newpos = sky.position;
            newpos.z = player.position.z;
            sky.position = newpos;

            moveDirection = new Vector3(0, 0, speed);

            switch (State)
            {
                case eState.WalkRight:
                    if (player.position.x < spawnPoints[StatePoint - 1])
                    {
                        moveDirection.x = WalkSpeed;
                        break;
                    }
                    else
                    {
                        State = eState.Stay;
                        StatePoint--;
                        break;
                    }
                case eState.WalkLeft:
                    if (player.position.x > spawnPoints[StatePoint + 1])
                    {
                        moveDirection.x = -WalkSpeed;
                        break;
                    }
                    else
                    {
                        State = eState.Stay;
                        StatePoint++;
                        break;
                    }
            }

            if (rotState == eState.WalkRight)
            {
                if (pivot.rotation.eulerAngles.y > 90)
                {
                    pivot.Rotate(new Vector3(0, -180 * Time.deltaTime, 0));//180гр/с. за 0.5fс
                    moveDirection = Vector3.zero;
                }
                else
                {
                    rotState = eState.Stay;
                    pivot.rotation = Quaternion.Euler(nextRotation);
                    carSp.canSpawn = true;
                    pivot.DetachChildren();
                }
            }
            else if (rotState == eState.WalkLeft)
            {
                if (pivot.rotation.eulerAngles.y < 270)
                {
                    pivot.Rotate(new Vector3(0, 180 * Time.deltaTime, 0));
                    moveDirection = Vector3.zero;
                }
                else
                {
                    rotState = eState.Stay;
                    pivot.rotation = Quaternion.Euler(nextRotation);
                    carSp.canSpawn = true;
                    pivot.DetachChildren();
                }
            }

            if (jump)
            {
                if (CFallSpeed > 0)
                {
                    CFallSpeed -= JumpSpeed * Time.deltaTime;
                    moveDirection.y = CFallSpeed;
                }
                else
                {
                    CFallSpeed = 0;
                    jump = false;
                }
            }

#if !UNITY_ANDROID
            if (jetpack)
            {
                if (Input.GetKey(KeyCode.LeftArrow))//<-
                {
                    if (player.position.x > -5.5)
                    {
                        moveDirection.x = -WalkSpeed;
                    }
                }
                else if (Input.GetKey(KeyCode.RightArrow))//->
                {
                    if (player.position.x < 5)
                    {
                        moveDirection.x = WalkSpeed;
                    }
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftArrow))//<-
                {
                    if (StatePoint < spawnPoints.Length - 1)
                    {
                        State = eState.WalkLeft;
                    }
                }
                if (Input.GetKey(KeyCode.RightArrow))//->
                {
                    if (StatePoint > 0)
                    {
                        State = eState.WalkRight;
                    }
                }
                if (Input.GetKey(KeyCode.UpArrow) && controller.isGrounded)//^
                {
                    CFallSpeed = JumpHeight;
                    int animIndex = Random.Range(0, animationsJump.Length);
                    anim.Play(animationsJump[animIndex].name, PlayMode.StopAll);
                    jump = true;
                }
                if (Input.GetKey(KeyCode.DownArrow))//^
                {
                    if (!controller.isGrounded)
                        CFallSpeed -= 2;

                    anim.Play(animations[4].name);
                }
            }

#else
            if (Input.touchCount > 0)
            {
                Touched = true;
                delta += Input.GetTouch(0).deltaPosition;

                if(jetpack)
                {
                    if (Input.GetTouch(0).position.x < screenCenter)
                    {
                        if (player.position.x > -5.5)
                        {
                            moveDirection.x = -WalkSpeed;
                        }
                    }
                    else
                    {
                        if (player.position.x < 5)
                        {
                            moveDirection.x = WalkSpeed;
                        }
                    }
                }
            }
            else
            {
                if (Touched)
                {
                    Touched = false;

                    if(!jetpack) getInput();

                    delta = Vector2.zero;
                }
            }

            if (Magnitude(delta) > 30)
            {
                if(!jetpack) getInput();

                delta = Vector2.zero;
            }
#endif
            newpos = carSp.transform.position;
            newpos.z = player.position.z + 120;
            carSp.transform.position = newpos;

            if (!jump && controller.isGrounded)
            {
                CFallSpeed = 0;
                if(!anim.isPlaying)
                    anim.PlayQueued(animations[0].name);
                //CFallSpeed = JumpHeight;
                //jump = true;
                //animation.CrossFadeQueued(animations[0].name, 1.0ff, QueueMode.PlayNow);
            }
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        if (!jump && !controller.isGrounded)
        {
            CFallSpeed -= gravity * Time.deltaTime;
            moveDirection.y += CFallSpeed;
        }

        controller.Move(moveDirection * Time.deltaTime);
    }

#if UNITY_ANDROID

    public static float Magnitude(Vector2 a)
    {
        return Mathf.Sqrt(a.x * a.x + a.y * a.y);
    }

    void getInput()
    {
        if (System.Math.Abs(delta.x) > System.Math.Abs(delta.y))
        {
            if (delta.x < 0)
            {//<-
                if (StatePoint < spawnPoints.Length - 1)
                {
                    State = eState.WalkLeft;
                }
            }
            if (delta.x > 0)
            {//<-
                if (StatePoint > 0)
                {
                    State = eState.WalkRight;
                }
            }
        }
        else
        {
            if (delta.y > 0 && controller.isGrounded)
            {//вверх
                CFallSpeed = JumpHeight;
                int animIndex = Random.Range(0, animationsJump.Length);
                anim.Play(animationsJump[animIndex].name, PlayMode.StopAll);
                jump = true;
            }
            if (delta.y < 0)
            {
                if (!controller.isGrounded)
                        CFallSpeed -= 2;

                anim.Play(animations[4].name);
            }
        }
    }
#endif

    public void Fly()
    {
        StatePoint = 1; State = eState.Stay;
        jetpack = true;
        jump = true;
        gravity = 0;
        anim.Play(animations[3].name);

        duckSpawner.changeBird(true);
        duckSpawner.BeginSpawningDucks();

        smoothFollow2D.smoothTime *= 3;

        sc.DisableFlyButton();
    }

    public void StopFlying()
    {
        jetpack = false;
        jump = false;
        gravity = 35;
        anim.Play(animations[0].name);

        duckSpawner.StopSpawningDucks();

        smoothFollow2D.smoothTime /= 3;

        int x = (int)player.position.x;
        if (x < -1) {
            if (inTheForest)
            {
                StatePoint = 3; State = eState.WalkRight;                           //2
            }
            else
            {
                StatePoint = 2; State = eState.WalkLeft;                            //3
            }
        }
        else if (x >= -1 && x < 0) { StatePoint = 1; State = eState.WalkLeft; }     //2
        else if (x >= 0 && x < 1) { StatePoint = 2; State = eState.WalkRight;}      //1
        else { StatePoint = 1; State = eState.WalkRight; }                          //0

        sc.EnableFlyButton();
    }

    public void NonStopDeactive()
    {
        sc.NonStopDeactive();
        nonStopActived = false;
    }

    public void ChangeCam()
    {
        if (FPS)
        {
            myCamera.parent = null;
        }
        else
        {
            myCamera.parent = player;
            myCamera.position = cameraPos.position;
        }
        myCamera.GetComponent<SmoothFollow2D>().enabled = FPS;
        FPS = !FPS;
    }

    public void DoExplosion()
    {
        player.particleSystem.Emit(100);
    }

    public void Death()
    {
        anim.Play(animations[1].name);
        gameOver = true;
        jump = false;

        gameObject.SendMessage("GameOver");
        
        GameObject.Destroy(areaSp);
        //GameObject.Destroy(carSp);
        carSp.GameOver();

        myCamera.SendMessage("GameOver");

        sc.GameOver();
    }

    IEnumerator swim(float z)
    {
        sc.DisableFlyButton();

        carSp.StopAllCoroutines();
        carSp.canSpawn = false;
        carSp.bonusPrefab[6] = carSp.bonusPrefab[0]; //убрать джетпак
        areaSp.Pause();

        StatePoint = 1; State = eState.Stay;
        jetpack     = true;
        jump        = true;
        gravity = 0;
        WalkSpeed /= 3;

        Vector3 newpos = new Vector3(spawnPoints[0], 0.1f, z - 5.9f);
        player.position = newpos;

        gameOver = true;
        smoothFollow2D.toFloatTarget();
        smoothFollow2D.smoothTime /= 3;

        if (fromRoadToWaterAnim != null)
        {
            anim.Play(fromRoadToWaterAnim.name, PlayMode.StopAll);
            yield return new WaitForSeconds(fromRoadToWaterAnim.length);
        }

        GameObject staticBoat = GameObject.Find("boat");
        GameObject.Destroy(staticBoat);

        gameOver = false;
        smoothFollow2D.toFixedTarget();
        smoothFollow2D.smoothTime *= 3;

        newpos = player.position;

        if (pers == Pers.gera)
            newpos.y = -7.06f;
        else
            newpos.y = -7.0f;

        player.position = newpos;

        duckSpawner.changeBird(false);
        duckSpawner.canSpawn = true;

        boat = NGUITools.AddChild(player.gameObject, boatPrefab);

        if(boat.animation)
            boat.animation.Play();

        anim.Play(swimAnimation.name);

        inTheWater = true;

        areaSp.Continue();
    }

    IEnumerator stopSwim(float z)
    {
        StatePoint = 0; State = eState.Stay;
        Vector3 newpos = new Vector3(spawnPoints[StatePoint], player.position.y, player.position.z - 20f);
        player.position = newpos;

        areaSp.RightFromWaterToRoute(z);
        areaSp.Pause();

        gameOver = true;
        smoothFollow2D.toFloatTarget();
        smoothFollow2D.smoothTime /= 3;

        if (fromWaterToRoadAnim != null)
        {
            if (boat.animation)
            {
                boat.animation.Play("fromWaterToRoad");
            }

            anim.Play(fromWaterToRoadAnim.name, PlayMode.StopAll);
            yield return new WaitForSeconds(fromWaterToRoadAnim.length);
        }
        else
        {
            BckgrndCar.DeleteAllBckgrndCars();
        }

        GameObject.Destroy(boat);

        gameOver = false;
        smoothFollow2D.toFixedTarget();
        smoothFollow2D.smoothTime *= 3;

        jetpack = false;
        jump = false;
        inTheWater = false;
        gravity = 35;
        WalkSpeed *= 3;

        duckSpawner.StopSpawningDucks();

        anim.Play(animations[0].name);

        newpos.y = 0.1f;
        newpos.z += 20f;
        player.position = newpos;

        duckSpawner.changeBird(true);

        areaSp.Continue();
        sc.EnableFlyButton();
    }

    void OnTriggerEnter(Collider car)
    {
        if (!gameOver)
        {
            if ((car.tag == "Car" || car.tag == "Moto") && rotState == eState.Stay)
            {
                car.collider.enabled = false;
                if (nonStopActived)
                {
                    DoExplosion();
                }
                else if (lives != 0)
                {
                    lives--;
                    DoExplosion();
                }
                else
                    Death();
            }
            else if (car.tag == "Sound")
            {
                car.audio.Play();
            }
            else if (car.tag == "Roll")
            {
                if (nonStopActived || anim.IsPlaying(animations[4].name) || anim.IsPlaying(animations[2].name))
                {
                    car.collider.enabled = false;
                    car.enabled = false;
                }
                else if (lives != 0)
                {
                    car.collider.enabled = false;
                    car.enabled = false;

                    lives--;
                    DoExplosion();
                }
                else
                    Death();
            }
            else if (car.tag == "Human")
            {
                anim.Play(animations[2].name);
                car.animation.Play("pedFall");
                car.audio.Play();
                sc.AddCarrot();
            }
            else if(car.tag == "Duck")
            {
                car.particleSystem.enableEmission = true;
                car.transform.GetChild(0).renderer.enabled = false;
                car.audio.Play();
                sc.AddDuck();
                sc.AddCarrot();
            }
            else if (car.tag == "Kuritsa")
            {
                car.particleSystem.enableEmission = true;
                car.transform.GetChild(0).renderer.enabled = false;
                car.audio.Play();
                anim.Play(animations[2].name);
                sc.AddDuck();
                sc.AddCarrot();
            }
            else if (car.tag == "RunScooter")
            {
                if (pers != Pers.rabbit && pers != Pers.kuritsa)
                {
                    car.animation.Play();
                    anim.Play(animations[2].name);
                    car.audio.Play();
                    car.transform.GetChild(0).gameObject.collider.enabled = false;
                    sc.AddScooter();
                    sc.AddCarrot();
                }
            }
            else if (car.tag == "Scooter")
            {
                car.animation.Play("fall(s)");
                anim.Play(animations[2].name);
                car.audio.Play();
                car.transform.GetChild(0).gameObject.collider.enabled = false;
                sc.AddCarrot();
                sc.AddScooter();
            }
            else if (car.tag == "Bonus")
            {
                if (car.name == "carrot")
                {
                    sc.AddCarrot();
                }
                else if (car.name == "X2(Clone)")
                {
                    sc.Multiple();
                    audio.Play();
                }
                else if (car.name == "nonStop(Clone)")
                {
                    sc.NonStopActive();
                    nonStopActived = true;
                    audio.Play();
                }
                else
                {
                    audio.Play();
                }
            }
            else if (car.tag == "GameController")
            {
                if (car.name == "Right" || car.name == "RightForest" || car.name == "RightWater" || car.name == "RightFromWaterToRoute")
                {
                    if (!inTheForest && StatePoint != 0 && !inTheWater) return;

                    StatePoint = 0;

                    if (!(car.name == "RightWater" || car.name == "RightFromWaterToRoute"))
                    {
                        restateSpawnPoints(true);

                        inTheForest = !inTheForest;
                    }
                    //else
                    //{
                        //if (pers != Pers.gera)
                            //return;
                    //}

                    Right(car.name, car.transform.position.z);
                }
                else if (car.name == "Left" || car.name == "LeftForest")
                {
                    if (inTheForest || StatePoint != 3) return;

                    StatePoint = 3;

                    restateSpawnPoints(false);

                    //если на лево, то полюбому в лес свернул.
                    inTheForest = true;

                    Left(car.name, car.transform.position.z);
                }

                car.enabled = false;
            }
        }
    }

    private void restateSpawnPoints(bool right)
    {
        if (inTheForest)
        {
            spawnPoints = (float[])fullSpawnPoints.Clone();
        }
        else if(!inTheWater)
        {
            if (right)
            {
                //удаление последнего элемента
                spawnPoints = new float[fullSpawnPoints.Length - 1];
                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    spawnPoints[i] = fullSpawnPoints[i];
                }
            }
            else
            {
                //удаление первого элемента
                spawnPoints = new float[fullSpawnPoints.Length - 1];
                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    spawnPoints[i] = fullSpawnPoints[i + 1];
                }
            }
        }
    }

    void prepareToRotate(float z)
    {
        pivot.rotation = Quaternion.Euler(0f, 180f, 0f);

        //перестановка игрока и pivot по Z, X
        State = eState.Stay;
        Vector3 newpos = new Vector3(fullSpawnPoints[StatePoint], player.position.y, z);
        newpos.z -= 5.04f;

        player.position = newpos;
        pivot.position = newpos;
		
		sky.transform.parent = pivot;

        string[] tagsToRotate = { "Car", "Area", "Scooter", "RunScooter", "Duck" };
        for (int k = 0; k < tagsToRotate.Length; k++)
        {
            GameObject[] go = GameObject.FindGameObjectsWithTag(tagsToRotate[k]);

            for (int i = 0; i < go.Length; i++)
            {
                go[i].transform.parent = pivot;
            }
        }
    }

    private void Right(string commandName, float Zpos)
    {
        prepareToRotate(Zpos);

        if (commandName == "RightWater")
        {
            StartCoroutine(swim(Zpos));

            areaSp.RightWater(Zpos);
            carSp.RightWater();
        }
        else if (commandName == "RightFromWaterToRoute")
        {
            StartCoroutine(stopSwim(Zpos));

            //BckgrndCars.canSpawn = false;
            BckgrndCar.DeleteAllBckgrndCarSpawners();

            //areaSp.RightFromWaterToRoute(Zpos);
            carSp.RightFromWaterToRoute();
        }
        else if(commandName == "RightForest")
        {
            areaSp.RightForest(Zpos);
            carSp.Right();
        }
        else
        {
            areaSp.Right(Zpos);
            carSp.Right();
        }

        nextRotation = pivot.rotation.eulerAngles;
        nextRotation.y -= 90;
        nextRotation = Quaternion.Euler(nextRotation).eulerAngles;

        rotState = eState.WalkRight;
    }

    private void Left(string commandName, float Zpos)
    {
        //StatePoint = 0;
        prepareToRotate(Zpos);

        if (commandName == "Left")
        {
            areaSp.Left(Zpos);
        }
        else
        {
            areaSp.LeftForest(Zpos);
        }

        carSp.Left();

        nextRotation = pivot.rotation.eulerAngles;
        nextRotation.y += 90;
        nextRotation = Quaternion.Euler(nextRotation).eulerAngles;

        rotState = eState.WalkLeft;
    }
}