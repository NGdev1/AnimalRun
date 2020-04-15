using UnityEngine;
using System.Xml;
using Assets.Scripts.Common;

public class MenuManager : MonoBehaviour
{
    public AudioClip[] ButtonClickSound;
    public AudioClip villageSound;
    public UIPanel scoreAndMoney;
    public Transform ball;
    public Transform rabbit;
    public Transform gera;
    public Transform kuritsa;
    public Transform dog;
    public Transform bear;
    public Transform koza;
    public GameObject[] gridPrefabs;
    public UILabel AttemptsLabel;
    public UILabel MaxScoreLabel;
    public UILabel CarrotsLabel;
    public UILabel CarrotsLabel1;
    public UILabel PriceLabel;
    public UILabel PriceLabel1;
    public UILabel PlayLabel;
    public UILabel BuyLabel;
    public UILabel[] priceShop2Labels;
    public GameObject ClothShopButton;

    public UILabel glidersLabel;
    public UILabel gliderTimeLabel;
    public UILabel nonStopTimeLabel;
    public UILabel jumpBTimeLabel;
    public UILabel galoshaTimeLabel;

    public UICenterOnChild gridSelecion;
    public UIDraggablePanel mDrag;
    public UIGrid grid;
    //public UICheckbox UIsoundEnanled;
    public UICheckbox UImusicEnanled;
    public UICheckbox UInight;
    public UICheckbox UIForest;
    public GameObject[] characters;

    public int[] prices;
    public GameObject gridNonePrefab;

    public MainTapjoy mainTapjoy;
    public mTapjoy tapjoy;

    //зашифрованные переменные
    private string carrots; //int
    private string key;

#if UNITY_ANDROID
    [System.NonSerialized]
    public string payed;   //bool
    [System.NonSerialized]
    public string key2;
#endif

    private int StatePoint;
    //private bool soundEnabled;
    private bool musicEnabled;
    private bool night;
    private bool inTheForest;
    private Vector3 velocity;
    private int maxScore;
    private string persName;
    private int gliders;
    private bool[] openedPers;
    private bool[][] openedCloth;
    private string clothName = "";
    private GameObject[] gridElements;

    private string[][] clothData;
    private float[] BonusesDuration;

    //0 - botinok
    //1 - galosha
    //2 - nonStop
    //3 - glider

    // Use this for initialization
    const int MaxClothCount = 8;
    const int PersCount = 7;

    void Start()
    {
        //PlayerPrefs.DeleteKey("GSAGSAGSA");
        musicEnabled = true;

        clothData = new string[characters.Length][];
        openedCloth = new bool[characters.Length][];
        BonusesDuration = new float[4];

        for (int i = 0; i < characters.Length; i++)
        {
            clothData[i] = new string[MaxClothCount];
            openedCloth[i] = new bool[MaxClothCount];
            for (int j = 0; j < MaxClothCount; j++)
            {
                clothData[i][j] = "none";
                openedCloth[i][j] = false;
            }
        }

        PriceLabel.alpha = 0;
        PriceLabel1.alpha = 0;

        openedPers = new bool[PersCount];
        openedPers[0] = true;
        for (int i = 1; i < PersCount; i++)
            openedPers[i] = false;
        openedPers[2] = true;

        StatePoint = 1;

        if (PlayerPrefs.HasKey("GSAGSAGSA"))
        {
            try
            {
                readXML();
            }
            catch
            {
                Debug.LogError("Error!");
                PlayerPrefs.DeleteKey("GSAGSAGSA");
                createXML();
            }
        }
        else
        {
            createXML();
        }

        readXMLCloth();
        FillGrid();
        WearPers();

        gridSelecion.CenterOn(gridElements[gridElements.Length - 1].transform);

        if (openedPers[StatePoint - 1])
        {
            PriceLabel.alpha = 0;
            PriceLabel1.alpha = 0;
            PlayLabel.text = "Играть";
        }
        else
        {
            PlayLabel.text = "Купить";
            PriceLabel.alpha = 1;
            PriceLabel.text = prices[StatePoint - 1].ToString();
            PriceLabel1.alpha = 1;
        }

        if (StatePoint == 4 || StatePoint == 6 || StatePoint == 7) ClothShopButton.SetActive(false);
        else ClothShopButton.SetActive(true);

#if UNITY_ANDROID
        key2 = B64X.GetNewKey();
        payed = B64X.Encrypt("false", key2);
#endif
    }

    public void FillGrid()
    {
        string[] gridNames = characters[StatePoint - 1].GetComponentInChildren<ClothLoader>().getGridNames();
        int pos = 0;
        int i;

        foreach (Transform child in grid.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        if (gridNames.Length == 0) { Debug.Log("debil"); return; }

        gridElements = new GameObject[gridNames.Length + 1];

        GameObject go = NGUITools.AddChild(grid.gameObject, gridNonePrefab);
        go.name = "none";

        gridElements[gridNames.Length] = go;

        for (i = 0; i < gridNames.Length; i++)
        {
            while (gridPrefabs[pos].name != gridNames[i])
                pos++;

            go = NGUITools.AddChild(grid.gameObject, gridPrefabs[pos]);
            go.name = gridPrefabs[pos].name;

            gridElements[i] = go;
        }
        grid.Reposition();

        if (clothData[StatePoint - 1][0] == "none")
            BuyLabel.text = "Применено";
        else
            BuyLabel.text = "Применить";
    }

    public void changeMoney(int val, bool changeInTapjoy)
    {
        carrots = B64X.Decrypt(carrots, key);

        int money = int.Parse(carrots);
        money += val;

        carrots = money.ToString(); money = -127;
        CarrotsLabel.text = carrots.ToString();
        CarrotsLabel1.text = carrots.ToString();

        key = B64X.GetNewKey();
        carrots = B64X.Encrypt(carrots, key);

#if UNITY_ANDROID
        if(changeInTapjoy)
            mainTapjoy.ChangeMoney(val);
#endif
    }

    public int getMoney()
    {
        carrots = B64X.Decrypt(carrots, key);

        int money = int.Parse(carrots);

        key = B64X.GetNewKey();
        carrots = B64X.Encrypt(carrots, key);
        return money;
    }

#if UNITY_ANDROID
    public bool Payed()
    {
        payed = B64X.Decrypt(payed, key2);

        bool val = bool.Parse(payed);

        key2 = B64X.GetNewKey();
        payed = B64X.Encrypt(payed, key2);
        return val;
    }
#endif

    /*
    void OnSoundChange(bool val)
    {
        soundEnabled = val;
        if (soundEnabled)
            audio.volume = 1;
        else
            audio.volume = 0;
    }
    */

    void OnMusicChange(bool val)
    {
        musicEnabled = val;
    }

    void OnNightChange(bool val)
    {
        night = val;
    }
    
    void OnForestChange(bool val)
    {
        inTheForest = val;
    }

    void HideScoreAndMoney()
    {
        scoreAndMoney.alpha = 0;
    }

    void ShowScoreAndMoney()
    {
        scoreAndMoney.alpha = 1;
    }

    #region secondShop
    //0 - botinok
    //1 - galosha
    //2 - nonStop
    //3 - glider
    void BuyGlider()
    {
        if (getMoney() >= 150)
        {
            gliders++;
            changeMoney(-150, true);
            glidersLabel.text = "Джетпаки: [990000]" + gliders.ToString() + "[000000] шт";
        }
        else
        {
            priceShop2Labels[0].color = Color.red;
        }
    }

    void BuyGliderTime()
    {
        if (getMoney() >= 50)
        {
            BonusesDuration[3]++;
            changeMoney(-50, true);
            gliderTimeLabel.text = "Время бонуса: [990000]" + BonusesDuration[3].ToString() + "[000000] сек.";
        }
        else
        {
            priceShop2Labels[1].color = Color.red;
        }
    }

    void BuyNonStopTime()
    {
        if (getMoney() >= 50)
        {
            BonusesDuration[2]++;
            changeMoney(-50, true);
            nonStopTimeLabel.text = "Время бонуса: [990000]" + BonusesDuration[2].ToString() + "[000000] сек.";
        }
        else
        {
            priceShop2Labels[2].color = Color.red;
        }
    }

    void BuyJumpBTime()
    {
        if (getMoney() >= 50)
        {
            BonusesDuration[0]++;
            changeMoney(-50, true);
            jumpBTimeLabel.text = "Время бонуса: [990000]" + BonusesDuration[0].ToString() + "[000000] сек.";
        }
        else
        {
            priceShop2Labels[3].color = Color.red;
        }
    }

    void BuyGaloshaTime()
    {
        if (getMoney() >= 50)
        {
            BonusesDuration[1]++;
            changeMoney(-50, true);
            galoshaTimeLabel.text = "Время бонуса: [990000]" + BonusesDuration[1].ToString() + "[000000] сек.";
        }
        else
        {
            priceShop2Labels[4].color = Color.red;
        }
    }

    #endregion

    public void WearPers()
    {
        ClothLoader cloth = characters[StatePoint - 1].GetComponentInChildren<ClothLoader>();
        cloth.TakeOff();
        for (int i = 0; i < clothData[StatePoint - 1].Length; i++)
        {
            cloth.Wear(clothData[StatePoint - 1][i]);
        }
        PriceLabel.alpha = 0;
        PriceLabel1.alpha = 0;
    }

    private void readXML()
    {
        XmlTextReader reader = new XmlTextReader(B64X.Decrypt(PlayerPrefs.GetString("GSAGSAGSA"), "ZeFuTo!"), XmlNodeType.Document, null);
        string NodeName = "";
        while (reader.Read())
        {
            /*if (reader.NodeType == XmlNodeType.Text && NodeName == "SoundEnabled")
            {
                soundEnabled = bool.Parse(reader.Value);
                UImusicEnanled.isChecked = soundEnabled;
                OnSoundChange(soundEnabled);
            }
            */
            if (reader.NodeType == XmlNodeType.Text && NodeName == "MusicEnabled")
            {
                musicEnabled = bool.Parse(reader.Value);
                UImusicEnanled.isChecked = musicEnabled;
            }
            else if (reader.NodeType == XmlNodeType.Text && NodeName == "Night")
            {
                night = bool.Parse(reader.Value);
                UInight.isChecked = night;
            }
            else if (reader.NodeType == XmlNodeType.Text && NodeName == "StartRoute")
            {
                inTheForest = bool.Parse(reader.Value);
                UIForest.isChecked = inTheForest;
            }
            else if (reader.NodeType == XmlNodeType.Text && NodeName == "Gliders")
            {
                gliders = int.Parse(reader.Value);
                glidersLabel.text = "Джетпаки: [990000]" + gliders.ToString() + "[000000] шт";
            }
            else if (reader.NodeType == XmlNodeType.Text && NodeName == "MaxScore")
            {
                maxScore = int.Parse(reader.Value);
                MaxScoreLabel.text = maxScore.ToString();
                //break; //можно прервать цикл (нужно прочитать только одно значение)
            }
            else if (reader.NodeType == XmlNodeType.Text && NodeName == "Pers")
            {
                persName = reader.Value;
                SwitchStatePoint();
            }
            else if (reader.NodeType == XmlNodeType.Text && NodeName == "OpenedPers")
            {
                string line = reader.Value;
                for (int pos = 0; pos < line.Length; pos++)
                {
                    if (line[pos] == '1')
                        openedPers[pos] = true;
                    else
                        openedPers[pos] = false;
                }
            }
            else if (reader.NodeType == XmlNodeType.Text && NodeName == "Money")
            {
                key = B64X.GetNewKey();

                //if(reader.Value == "0")
                    //carrots = B64X.Encrypt("70000", key);
                //else
                    carrots = B64X.Encrypt(reader.Value, key);

                changeMoney(0, false);
            }
            else if (reader.NodeType == XmlNodeType.Text && NodeName == "Attempts")
            {
                int Attempts = int.Parse(reader.Value);
                AttemptsLabel.text = "Попыток: " + Attempts.ToString();
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "BonusesDuration")
            {
                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    BonusesDuration[i] = float.Parse(reader.GetAttribute(i));
                }

                jumpBTimeLabel.text = "Время бонуса: [990000]" + BonusesDuration[0].ToString() + "[000000] сек.";
                galoshaTimeLabel.text = "Время бонуса: [990000]" + BonusesDuration[1].ToString() + "[000000] сек.";
                nonStopTimeLabel.text = "Время бонуса: [990000]" + BonusesDuration[2].ToString() + "[000000] сек.";
                gliderTimeLabel.text = "Время бонуса: [990000]" + BonusesDuration[3].ToString() + "[000000] сек.";
            }
            else if (reader.NodeType == XmlNodeType.Element)
            {
                NodeName = reader.Name;
            }
        }
        reader.Close();
    }

    private void readXMLCloth()
    {
        XmlTextReader reader = new XmlTextReader(B64X.Decrypt(PlayerPrefs.GetString("GSAGSAGSA"), "ZeFuTo!"), XmlNodeType.Document, null);

        for (int j = 1; j <= PersCount; j++)
        {
            persName = parsePersName(j);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == persName)
                {
                    for (int i = 0; reader.MoveToNextAttribute(); i++)
                    {
                        if (reader.Name != "opened")
                        {
                            clothData[j - 1][i - 1] = reader.Value;
                        }
                        else
                        {
                            string line = reader.Value;
                            for (int pos = 0; pos < line.Length; pos++)
                            {
                                if (line[pos] == '1')
                                    openedCloth[j - 1][pos] = true;
                                else
                                    openedCloth[j - 1][pos] = false;
                            }
                        }
                    }
                    break;
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
		
		for(int i = 0; i < 4; i++)
			BonusesDuration[i] = 10;
			
		jumpBTimeLabel.text = "Время бонуса: [990000]" + BonusesDuration[0].ToString() + "[000000] сек.";
        galoshaTimeLabel.text = "Время бонуса: [990000]" + BonusesDuration[1].ToString() + "[000000] сек.";
        nonStopTimeLabel.text = "Время бонуса: [990000]" + BonusesDuration[2].ToString() + "[000000] сек.";
        gliderTimeLabel.text = "Время бонуса: [990000]" + BonusesDuration[3].ToString() + "[000000] сек.";
		
		gliders = 0;
		glidersLabel.text = "Джетпаки: " + gliders.ToString() + " шт.";

        key = B64X.GetNewKey();
        carrots = B64X.Encrypt("0", key);
    }

    void WantToPlay()
    {
        if (openedPers[StatePoint - 1])
        {
            Application.LoadLevel(1);
        }
        else
        {
            if (getMoney() >= prices[StatePoint - 1])
            {
                changeMoney(-prices[StatePoint - 1], true);

                openedPers[StatePoint - 1] = true;
                PlayLabel.text = "Играть";
                PriceLabel.alpha = 0;
                PriceLabel1.alpha = 0;
            }
            else
            {
                PlayLabel.text = "Не хватает денег!!!";
            }
        }
    }

    void Right()
    {
        StatePoint++;

        if (StatePoint == PersCount + 1)
            StatePoint = 1;

        if (StatePoint == 4 || StatePoint == 6 || StatePoint == 7) ClothShopButton.SetActive(false);
        else ClothShopButton.SetActive(true);

        FillGrid();
        gridSelecion.CenterOn(gridElements[gridElements.Length - 1].transform);

        if (openedPers[StatePoint - 1])
        {
            PriceLabel.alpha = 0;
            PriceLabel1.alpha = 0;
            PlayLabel.text = "Играть";
        }
        else
        {
            PlayLabel.text = prices[StatePoint - 1].ToString() + " руб\nКупить";
        }
        playSound();
        WearPers();

#if UNITY_ANDROID
        if (!Payed())
            if (tapjoy.enabled && Random.Range(0, 10) == 5)
                tapjoy.ShowOnPausePlacement();
#endif
    }

    void Left()
    {
        StatePoint--;

        if (StatePoint == 0)
            StatePoint = PersCount;

        if (StatePoint == 4 || StatePoint == 6) ClothShopButton.SetActive(false);
        else ClothShopButton.SetActive(true);

        FillGrid();
        gridSelecion.CenterOn(gridElements[gridElements.Length - 1].transform);

        if (openedPers[StatePoint - 1])
        {
            PriceLabel.alpha = 0;
            PriceLabel1.alpha = 0;
            PlayLabel.text = "Играть";
        }
        else
        {
            PlayLabel.text = prices[StatePoint - 1].ToString() + " руб\nКупить";
        }
        playSound();
        WearPers();

#if UNITY_ANDROID
        if (!Payed())
            if (tapjoy.enabled && Random.Range(0, 10) == 5)
                tapjoy.ShowOnPausePlacement();
#endif
    }

    void Update()
    {
        if (!audio.isPlaying)
        {
            audio.clip = villageSound;
            audio.Play();
        }

        if (StatePoint == 2)
        {
            transform.position = Vector3.SmoothDamp(transform.position, ball.position, ref velocity, 0.5f);

            if (transform.rotation.eulerAngles.y > 310)
                transform.Rotate(new Vector3(0, -20 * Time.deltaTime, 0));
        }
        else if(StatePoint == 1)
        {
            transform.position = Vector3.SmoothDamp(transform.position, rabbit.position, ref velocity, 0.5f);

            if (transform.rotation.eulerAngles.y > 310)
                transform.Rotate(new Vector3(0, -20 * Time.deltaTime, 0));
        }
        else if(StatePoint == 3)
        {
            transform.position = Vector3.SmoothDamp(transform.position, gera.position, ref velocity, 0.5f);

            if (transform.rotation.eulerAngles.y < 359)
                transform.Rotate(new Vector3(0, 20 * Time.deltaTime, 0));
        }
        else if (StatePoint == 4)
        {
            transform.position = Vector3.SmoothDamp(transform.position, kuritsa.position, ref velocity, 0.5f);

            if (transform.rotation.eulerAngles.y < 359)
                transform.Rotate(new Vector3(0, 20 * Time.deltaTime, 0));
        }
        else if (StatePoint == 5)
        {
            transform.position = Vector3.SmoothDamp(transform.position, dog.position, ref velocity, 0.5f);

            if (transform.rotation.eulerAngles.y > 310)
                transform.Rotate(new Vector3(0, -20 * Time.deltaTime, 0));
        }
        else if (StatePoint == 6)
        {
            transform.position = Vector3.SmoothDamp(transform.position, bear.position, ref velocity, 0.5f);

            if (transform.rotation.eulerAngles.y > 310)
                transform.Rotate(new Vector3(0, -20 * Time.deltaTime, 0));
        }
        else if (StatePoint == 7)
        {
            transform.position = Vector3.SmoothDamp(transform.position, koza.position, ref velocity, 0.5f);

            if (transform.rotation.eulerAngles.y > 310)
                transform.Rotate(new Vector3(0, -20 * Time.deltaTime, 0));
        }

        if (gridSelecion.centeredObject)
        {
            if (gridSelecion.centeredObject.name != clothName)
            {
                clothName = gridSelecion.centeredObject.name;
                ClothLoader cloth = characters[StatePoint - 1].GetComponentInChildren<ClothLoader>();

                //clothData[StatePoint - 1][cloth.DetermineType(clothName)] = clothName;
                if (openedCloth[StatePoint - 1][cloth.getIndex(clothName)] || clothName == "none")
                {
                    if (clothData[StatePoint - 1][cloth.DetermineTypeIndex(clothName)] == clothName)
                        BuyLabel.text = "Применено";
                    else
                        BuyLabel.text = "Применить";

                    PriceLabel.alpha = 0;
                    PriceLabel1.alpha = 0;
                }
                else
                {
                    BuyLabel.text = "Купить";

                    PriceLabel.text = cloth.getPrice(clothName).ToString();
                    PriceLabel.alpha = 1;
                    PriceLabel1.alpha = 1;
                }

                if (clothName == "none")
                {
                    WearPers();
                }
                else
                    cloth.Wear(clothName);
            }
        }

        if (Input.GetKey(KeyCode.Escape)) {
#if UNITY_ANDROID
            if (!Payed())
                if (tapjoy.enabled)
                    tapjoy.ShowOnPausePlacement();
#endif
            Application.Quit();
        }
    }

    void Buy()
    {
        ClothLoader cloth = characters[StatePoint - 1].GetComponentInChildren<ClothLoader>();

        if (openedCloth[StatePoint - 1][cloth.getIndex(clothName)] || clothName == "none")
        {
            BuyLabel.text = "Применено";
            clothData[StatePoint - 1][cloth.DetermineTypeIndex(clothName)] = clothName;

            if (clothName == "none")
            {
                cloth.TakeOff();
                for (int i = 0; i < 4; i++)
                    clothData[StatePoint - 1][i] = "none";
            }
            else
                cloth.Wear(clothName);
        }
        else
        {
            int price = cloth.getPrice(clothName);
            if(getMoney() >= price)
            {
                changeMoney(-price, true);

                clothData[StatePoint - 1][cloth.DetermineTypeIndex(clothName)] = clothName;

                BuyLabel.text = "Применено";
                openedCloth[StatePoint - 1][cloth.getIndex(clothName)] = true;
            }
            else
            {
                BuyLabel.text = "Не хватает бабосов!!!";
            }
        }
    }

    void playSound()
    {
        if (Random.Range(0, 3) == 0)
        {
            AudioClip clip = ButtonClickSound[Random.Range(0, ButtonClickSound.Length)];
            if (audio.clip != clip)
            {
                audio.clip = clip;
                audio.Play();
            }
        }
    }

    void rewriteXML()
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.InnerXml = B64X.Decrypt(PlayerPrefs.GetString("GSAGSAGSA"), "ZeFuTo!");

        xmlDoc.SelectSingleNode("Information/Pers").InnerText = persName;
        xmlDoc.SelectSingleNode("Information/MusicEnabled").InnerText = musicEnabled.ToString();
        xmlDoc.SelectSingleNode("Information/Night").InnerText = night.ToString();
        xmlDoc.SelectSingleNode("Information/StartRoute").InnerText = inTheForest.ToString();
        xmlDoc.SelectSingleNode("Information/Gliders").InnerText = gliders.ToString();

        string line = "";
        for (int i = 0; i < openedPers.Length; i++)
        {
            if (openedPers[i])
                line += '1';
            else
                line += '0';
        }
        xmlDoc.SelectSingleNode("Information/OpenedPers").InnerText = line;

        XmlNode node;

        for (int i = 0; i < PersCount; i++)
        {
            node = xmlDoc.SelectSingleNode("Information/Cloth/" + parsePersName(i + 1));

            line = "";
            for (int j = 0; j < openedCloth[i].Length; j++)
            {
                if (openedCloth[i][j])
                    line += '1';
                else
                    line += '0';
            }

            node.Attributes.Item(0).Value = line;

            for (int j = 0; j < 4; j++)
            {
                node.Attributes.Item(j + 1).Value = clothData[i][j];
            }
        }

        node = xmlDoc.SelectSingleNode("Information/BonusesDuration");
        for (int i = 0; i < 4; i++)
        {
            node.Attributes.Item(i).Value = BonusesDuration[i].ToString();
        }

        //Carrots
        xmlDoc.SelectSingleNode("Information/Money").InnerText = getMoney().ToString();

        //xmlDoc.Save("C:\\274.xml");

        PlayerPrefs.SetString("GSAGSAGSA", B64X.Encrypt(xmlDoc.InnerXml, "ZeFuTo!"));
    }

    string parsePersName(int index)
    {
        switch(index)
        {
            case 1:
                return "rabbit";
            case 2:
                return "ball";
            case 3:
                return "gera";
            case 4:
                return "kuritsa";
            case 5:
                return "dog";
            case 6:
                return "Bear";
            case 7:
                return "Koza";
            default:
                Debug.Log("debil");
                return "rabbit";
        }
    }

    void SwitchStatePoint()
    {
        switch (persName)
        {
            case "rabbit":
                StatePoint = 1;
                return;
            case "ball":
                StatePoint = 2;
                return;
            case "gera":
                StatePoint = 3;
                return;
            case "kuritsa":
                StatePoint = 4;
                return;
            case "dog":
                StatePoint = 5;
                return;
            case "Bear":
                StatePoint = 6;
                return;
            case "Koza":
                StatePoint = 7;
                return;
            default: 
                StatePoint = 1;
                persName = "rabbit";
                Debug.Log("debil");
                return;
        }
    }

    void OnDestroy()
    {
        persName = parsePersName(StatePoint);

        try
        {
            rewriteXML();
        }
        catch
        {
            PlayerPrefs.DeleteKey("GSAGSAGSA");
            createXML();

            try
            {
                rewriteXML();
            }
            catch
            {
                PlayerPrefs.DeleteKey("GSAGSAGSA");
                createXML();
            }
        }
        
        PlayerPrefs.Save();
    }
}