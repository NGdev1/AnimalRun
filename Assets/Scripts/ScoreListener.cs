using UnityEngine;
using System.Collections;
using System.Xml;
using System.IO;

using TapjoyUnity;

using Assets.Scripts.Common;

public class ScoreListener : MonoBehaviour
{
    [System.NonSerialized]
    public int score;
    [System.NonSerialized]
    public int maxScore;
    [System.NonSerialized]
    public int gliders;    

    public CarSpawner carSpawner;
    public AreaSpawner areaSpawner;
    public UILabel trindecLabel;
    public UILabel glidersLabel;
    public UILabel[] ScoreLabels;
    public UILabel[] MaxScoreLabels;
    public UILabel[] CarrotsLabels;
    public UILabel[] CarrotsTextLabels;
    public UILabel[] DuckLabels;
    public UILabel[] ScooterLabels;
    public UILabel MultipleLabel;
    public Animation menuAnimation1;
    public Animation GoAnimation2;
    public Animation pauseAnimation3;
    public Transform GameMenu;
    public Transform PauseMenu;
    public GameObject flyButton;

    public string persName;
    public GameObject[] pers;

    public bool night;
    public Light directionalLight;
    public Material skyMaterial;
    public Texture nightSkyTexture;
    public Texture daySkyTexture;

    public gTapjoy tapjoy;

    public string[] trindecPhrases;

    private string carrots;
    private string key;
    private string gameCarrots;
    private string key2;

    private bool playing;
    //private bool soundEnabled;
    private bool musicEnabled;
    private int multiple;
    private float TimeScale;
    private GameObject player;
    private bool inTheForest;
    private int Ducks;
    private int Scooters;
    private int Level;
    private int Attempts;
    private int gameDucks;
    private int gameScooters;
    private PlayerMove pm;
    private bool recordBroken;

    private string[] clothData;

    private static ScoreListener instance;

    public static ScoreListener getInstance()
    {
        if(instance == null)
        {
            instance = (ScoreListener)GameObject.FindObjectOfType(typeof(ScoreListener));
        }

        return instance;
    }

    void Awake()
    {
        clothData = new string[4];
        for (int i = 0; i < clothData.Length; i++)
        {
            clothData[i] = "none";
        }
        score = 0;
        maxScore = 0;
        playing = true;
        //soundEnabled = true;
        musicEnabled = true;
        recordBroken = false;
        TimeScale = 1;
        multiple = 1;
        Time.timeScale = TimeScale;

        if (PlayerPrefs.HasKey("GSAGSAGSA"))
        {
            readXML();
        }
        else
        {
            createXML();
        }

        SpawnPers();
        readXMLCloth();
        readXMLBonusesDuration();
        WearPers();

        pm = player.GetComponent<PlayerMove>();

        if (night)
        {
            Color nightColor = new Color(0.12f, 0.12f, 0.12f);
            //skyMaterial.color = nightColor;
            //directionalLight.color = nightColor;
            directionalLight.intensity = 0.1f;
            skyMaterial.mainTexture = nightSkyTexture;
            RenderSettings.fogColor = nightColor;
            Camera.main.backgroundColor = nightColor;
        }

        if (inTheForest)
        {
            pm.StartFromForest();
            areaSpawner.StartFromForest();
            carSpawner.StartFromForest();
        }
        else
        {
            areaSpawner.StartFromRoute();
            carSpawner.StartFromRoute();
            pm.StartFromRoute();
        }

        key2 = B64X.GetNewKey();
        gameCarrots = B64X.Encrypt("0", key2);

        //AudioListener.pause = !soundEnabled;

        if (!musicEnabled)
        {
            pm.myCamera.audio.enabled = false;
        }

        for (int i = 0; i < clothData.Length; i++)
            if (clothData[i] == "sparta" || clothData[i] == "arbuz" || clothData[i] == "Lopata" || clothData[i] == "Shlem") pm.lives++;
        if (persName == "Bear") pm.lives += 3;

#if UNITY_ANDROID
        if (Tapjoy.IsConnected) tapjoy.enabled = true;
        Tapjoy.OnConnectSuccess += HandleConnectSuccess;
#endif
    }

#if UNITY_ANDROID
    public void HandleConnectSuccess()
    {
        tapjoy.enabled = true;
    }
#endif


    void Update()
    {
        if (playing)
        {
            //score += (int)(Time.deltaTime * 1000);

            score = (int)Time.timeSinceLevelLoad;

                ScoreLabels[0].text = score.ToString();
                ScoreLabels[1].text = score.ToString();
                ScoreLabels[2].text = score.ToString();
                if (score > maxScore)
                {
                    recordBroken = true;
                    MaxScoreLabels[0].text = score.ToString();
                    MaxScoreLabels[1].text = score.ToString();
                    MaxScoreLabels[2].text = score.ToString();
                }
        }
        if (Input.GetKey(KeyCode.Escape) && playing)
        {
            Pause();
        }

        if (Time.timeScale < 1.7f && playing)
            Time.timeScale += 0.00002f;
    }

    void Pause()
    {
        if (playing)
        {
            TimeScale = Time.timeScale;
            Time.timeScale = 0;
            playing = false;

            GameMenu.position = Vector3.zero;
            PauseMenu.position = PauseMenu.parent.position;

#if UNITY_ANDROID
            if (Random.Range(0, 10) == 5 && tapjoy.enabled && Attempts > 5)
                tapjoy.ShowOnPausePlacement();
#endif
        }
    }

    void OnTimeScaleChange(float val)
    {
        TimeScale = val + 1;     
    }

    void MainMenu()
    {
        Time.timeScale = 1;
        Application.LoadLevel(0);
    }

    void Again()
    {
        Time.timeScale = 1;
        Application.LoadLevel(Application.loadedLevel);
    }

    void GliderActive()
    {
        if (gliders > 0)
        {
            gliders--;
            glidersLabel.text = gliders.ToString();
            player.SendMessage("gliderActive");                
        }
        else
        {
            DisableFlyButton();
        }
    }

    void Continue()
    {
        //menuAnimation1.Play();
        ActiveAnimation.Play(menuAnimation1, AnimationOrTween.Direction.Reverse);
        pauseAnimation3.Play();

        playing = true;
        Time.timeScale = TimeScale;
    }

    public void GameOver()
    {
        int trindecIndex = UnityEngine.Random.Range(0, trindecPhrases.Length);
        trindecLabel.text = trindecPhrases[trindecIndex];
        ScoreLabels[1].text = score.ToString();

        ActiveAnimation.Play(menuAnimation1, AnimationOrTween.Direction.Forward);
        GoAnimation2.Play();

        if (playing)
        {
            playing = false;
        }

#if UNITY_ANDROID
        if (tapjoy.enabled)
        {
            if (recordBroken)
                tapjoy.ShowOnRecordPlacement();
            else if (Random.Range(0, 15) == 5) // && Attempts < 30)
                tapjoy.ShowGameOverPlacement();
        }
#endif
    }

    private void readXML()
    {
        XmlTextReader reader = new XmlTextReader(B64X.Decrypt(PlayerPrefs.GetString("GSAGSAGSA"), "ZeFuTo!"), XmlNodeType.Document, null);
        string NodeName = "";
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Text && NodeName == "MaxScore")
            {
                maxScore = int.Parse(reader.Value);

                for (int i = 0; i < MaxScoreLabels.Length; i++)
                {
                    MaxScoreLabels[i].text = maxScore.ToString();
                }
                //break; //можно прервать цикл (нужно прочитать только одно значение)
            }
            else if (reader.NodeType == XmlNodeType.Text && NodeName == "Pers")
            {
                persName = reader.Value;
            }
            else if (reader.NodeType == XmlNodeType.Text && NodeName == "Night")
            {
                night = bool.Parse(reader.Value);
            }
            else if (reader.NodeType == XmlNodeType.Text && NodeName == "StartRoute")
            {
                inTheForest = !bool.Parse(reader.Value);
            }
            //else if (reader.NodeType == XmlNodeType.Text && NodeName == "SoundEnabled")
            //{
                //soundEnabled = bool.Parse(reader.Value);
            //}
            else if (reader.NodeType == XmlNodeType.Text && NodeName == "MusicEnabled")
            {
                musicEnabled = bool.Parse(reader.Value);
            }
            else if (reader.NodeType == XmlNodeType.Text && NodeName == "Money")
            {
                key = B64X.GetNewKey();
                carrots = B64X.Encrypt(reader.Value, key);
            }
            else if (reader.NodeType == XmlNodeType.Text && NodeName == "Attempts")
            {
                Attempts = int.Parse(reader.Value);
            }
            else if (reader.NodeType == XmlNodeType.Text && NodeName == "Gliders")
            {
                gliders = int.Parse(reader.Value);

                if (persName == "gera" || persName == "ball" || persName == "kuritsa" || persName == "Bear" || persName == "Koza")
                {
                    if (gliders != 0)
                    {
                        EnableFlyButton();
                    }
                }
                string pickupName = "Монеты:";
                if (persName == "rabbit")
                {
                    pickupName = "Морковь:";
                }
                else if(persName == "kuritsa")
                {
                    pickupName = "Яйца:";
                }
                else if(persName == "dog")
                {
                    pickupName = "Косточки:";
                }
                else if (persName == "Bear")
                {
                    pickupName = "Малина:";
                }

                CarrotsTextLabels[0].text = pickupName;
                CarrotsTextLabels[1].text = pickupName;
            }
            else if (reader.NodeType == XmlNodeType.Text && NodeName == "Scooters")
            {
                Scooters = int.Parse(reader.Value);
            }
            else if (reader.NodeType == XmlNodeType.Text && NodeName == "Level")
            {
                Level = int.Parse(reader.Value);
            }
            else if (reader.NodeType == XmlNodeType.Text && NodeName == "Ducks")
            {
                Ducks = int.Parse(reader.Value);
            }
            else if (reader.NodeType == XmlNodeType.Element)
            {
                NodeName = reader.Name;
            }
        }
        reader.Close();
    }

    public void DisableFlyButton()
    {
        flyButton.SetActive(false);
    }

    public void EnableFlyButton()
    {
        flyButton.SetActive(true);
        glidersLabel.text = gliders.ToString();
    }

    public void WearPers()
    {
        ClothLoader cloth = player.GetComponentInChildren<ClothLoader>();

        for (int i = 0; i < clothData.Length; i++)
        {
            cloth.Wear(clothData[i]);
        }
    }

    private void readXMLCloth()
    {
        XmlTextReader reader = new XmlTextReader(B64X.Decrypt(PlayerPrefs.GetString("GSAGSAGSA"), "ZeFuTo!"), XmlNodeType.Document, null);

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name == persName)
            {
                for (int i = 0; reader.MoveToNextAttribute(); i++)
                {
                    if (reader.Name != "opened")
                    {
                        clothData[i - 1] = reader.Value;
                    }
                }
                break;
            }
        }
        reader.Close();
    }

    private void readXMLBonusesDuration()
    {
        XmlTextReader reader = new XmlTextReader(B64X.Decrypt(PlayerPrefs.GetString("GSAGSAGSA"), "ZeFuTo!"), XmlNodeType.Document, null);

        while (reader.Read())
        {
            if (reader.Name == "BonusesDuration")
            {
                PlayerBonuses pb = player.GetComponent<PlayerBonuses>();
                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    pb.bonuses[i].duration = float.Parse(reader.GetAttribute(i));
                }
            }
        }
        reader.Close();
    }

    private void createXML()
    {
        TextAsset xmlAsset = Resources.Load("274") as TextAsset;
        PlayerPrefs.SetString("GSAGSAGSA", B64X.Encrypt(xmlAsset.text, "ZeFuTo!"));
        Debug.Log(PlayerPrefs.GetString("GSAGSAGSA"));

        key = B64X.GetNewKey();
        carrots = B64X.Encrypt("0", key);
    }

    public void AddCarrot()
    {
        gameCarrots = B64X.Decrypt(gameCarrots, key2);

        int money = int.Parse(gameCarrots);
        money += multiple;

        gameCarrots = money.ToString(); money = -127;

        CarrotsLabels[0].text = gameCarrots.ToString();
        CarrotsLabels[1].text = gameCarrots.ToString();
        CarrotsLabels[2].text = gameCarrots.ToString();

        key2 = B64X.GetNewKey();
        gameCarrots = B64X.Encrypt(gameCarrots, key2);
    }

    public void AddScooter()
    {
        Scooters++;
        gameScooters++;
        ScooterLabels[0].text = gameScooters.ToString();
        ScooterLabels[1].text = gameScooters.ToString();
    }

    public void AddDuck()
    {
        Ducks++;
        gameDucks++;
        DuckLabels[0].text = gameDucks.ToString();
        DuckLabels[1].text = gameDucks.ToString();
    }

    public void Multiple()
    {
        multiple++;
        MultipleLabel.text = "X" + multiple.ToString();
    }

    public void NonStopActive()
    {
        Color nightColor = new Color(0.52f, 0.32f, 0.32f); //Color.red was too bright;
        skyMaterial.color = nightColor;
        directionalLight.color = nightColor;
        directionalLight.intensity = 0.5f;
        RenderSettings.fogColor = nightColor;
        Camera.main.backgroundColor = nightColor;
    }

    public void NonStopDeactive()
    {
        if (night)
        {
            Color nightColor = new Color(0.12f, 0.12f, 0.12f);
            directionalLight.intensity = 0.1f;
            RenderSettings.fogColor = nightColor;
            Camera.main.backgroundColor = nightColor;
        }
        else
        {
            RenderSettings.fogColor = new Color(0.9f, 0.98f, 0.99f);
            Camera.main.backgroundColor = new Color(0.87f, 0.96f, 0.97f);
        }
        skyMaterial.color = new Color(0.57f, 0.57f, 0.57f);
        directionalLight.color = new Color(0.8f, 0.8f, 0.8f);
    }

    void SpawnPers()
    {
        switch (persName)
        {
            case "rabbit":
                player = GameObject.Instantiate(pers[0]) as GameObject;
                return;
            case "ball":
                player = GameObject.Instantiate(pers[1]) as GameObject;
                return;
            case "gera":
                player = GameObject.Instantiate(pers[2]) as GameObject;
                return;
            case "kuritsa":
                player = GameObject.Instantiate(pers[3]) as GameObject;
                return;
            case "dog":
                player = GameObject.Instantiate(pers[4]) as GameObject;
                return;
            case "Bear":
                player = GameObject.Instantiate(pers[5]) as GameObject;
                return;
            case "Koza":
                player = GameObject.Instantiate(pers[6]) as GameObject;
                return;
            default:
                player = GameObject.Instantiate(pers[0]) as GameObject;
                print("debil");
                return;
        }
    }

    void ChangeCam()
    {
        pm.ChangeCam();
    }

    void rewriteXML()
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.InnerXml = B64X.Decrypt(PlayerPrefs.GetString("GSAGSAGSA"), "ZeFuTo!");

        if (score > maxScore)
        {
            xmlDoc.SelectSingleNode("Information/MaxScore").InnerText = score.ToString();
            maxScore = score;
        }

        xmlDoc.SelectSingleNode("Information/Gliders").InnerText = gliders.ToString();
        xmlDoc.SelectSingleNode("Information/Attempts").InnerText = (++Attempts).ToString();
        xmlDoc.SelectSingleNode("Information/Scooters").InnerText = Scooters.ToString();
        xmlDoc.SelectSingleNode("Information/Ducks").InnerText = Ducks.ToString();
        xmlDoc.SelectSingleNode("Information/Level").InnerText = Level.ToString();

        //Carrots
        carrots = B64X.Decrypt(carrots, key);
        gameCarrots = B64X.Decrypt(gameCarrots, key2);

        int money = int.Parse(carrots); carrots = "";
        int newMoney = int.Parse(gameCarrots); gameCarrots = "";

        money += newMoney;

        xmlDoc.SelectSingleNode("Information/Money").InnerText = money.ToString(); money = -127;

        PlayerPrefs.SetString("GSAGSAGSA", B64X.Encrypt(xmlDoc.InnerXml, "ZeFuTo!"));
    }

    void OnDestroy()
    {
        skyMaterial.mainTexture = daySkyTexture;
        skyMaterial.color = new Color(0.57f, 0.57f, 0.57f);
        rewriteXML();
        PlayerPrefs.Save();
    }
}