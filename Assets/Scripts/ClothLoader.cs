using UnityEngine;
using System.Collections;
using System.Xml;

using Assets.Scripts.Common;

public class ClothLoader : MonoBehaviour
{
    public class Cloth
    {
        public ClothType type = ClothType.head;
        public GameObject entity;
        public Vector3 pos = Vector3.zero;

        public string insteadOf = "null";
        public string parent = "";
        public string name = "";
        public int price;
    }

    public enum ClothType
    {
        material,
        head,
        body,
        legs,
        none
    }

    ClothType ParseClothType(string val)
    {
        switch (val)
        {
            case "material":
                return ClothType.material;
            case "body":
                return ClothType.body;
            case "head":
                return ClothType.head;
            case "legs":
                return ClothType.legs;
            default:
                return ClothType.none;
        }
    }

    public string persName;
    [System.NonSerialized]
    public string description;
    private bool rightPers;
    private Cloth[] cloth;
    public GameObject[] prefabs;
    public Texture2D[] textures;
    public Texture2D InitTexture;
    public string rendererParent;

    private int index;

    void Awake()
    {
        rightPers = false;
        index = -1;

        LoadData();
    }

    public void LoadData()
    {
        XmlDocument xmlDoc = new XmlDocument();
        TextAsset xmlAsset = Resources.Load("PersData") as TextAsset;

        xmlDoc.LoadXml(B64X.Decrypt(xmlAsset.text, "_ip#X1zaMoQ#mSig!%9j"));

        LoadTree(xmlDoc.DocumentElement);
    }

    void LoadTree(XmlNode node)
    {
        if (node != null)
            Load(node);

        if (node.HasChildNodes)
        {
            node = node.FirstChild;
            while (node != null)
            {
                LoadTree(node);
                node = node.NextSibling;
            }
        }
    }

    void Load(XmlNode node)
    {
        if (node.Attributes == null)
            return;

        XmlAttributeCollection map = node.Attributes;

        if (node.Name == persName)
        {
            rightPers = true;
        }
        else if (node.Name == "Description")
        {
            description = node.Value;

            rightPers = false;
        }
        else if (node.Name == "Item" && rightPers)
        {
            index++;
            if (XmlNodeType.Element == node.NodeType)
            {
                foreach (XmlNode attrnode in map)
                {
                    if (attrnode.Name == "name")
                    {
                        cloth[index].name = attrnode.Value;
                    }
                    else if (attrnode.Name == "parent")
                    {
                        cloth[index].parent = attrnode.Value;
                    }
                    else if (attrnode.Name == "insteadOf")
                    {
                        cloth[index].insteadOf = attrnode.Value;
                    }
                    else if (attrnode.Name == "type")
                    {
                        cloth[index].type = ParseClothType(attrnode.Value);
                    }
                    else if (attrnode.Name == "price")
                    {
                        cloth[index].price = int.Parse(attrnode.Value);
                    }
                }
            }
        }
        else if (node.Name == "Pos" && rightPers)
        {
            if (XmlNodeType.Element == node.NodeType)
            {
                foreach (XmlNode attrnode in map)
                {
                    if (attrnode.Name == "x")
                    {
                        cloth[index].pos.x = float.Parse(attrnode.Value);
                    }
                    else if (attrnode.Name == "y")
                    {
                        cloth[index].pos.y = float.Parse(attrnode.Value);
                    }
                    else if (attrnode.Name == "z")
                    {
                        cloth[index].pos.z = float.Parse(attrnode.Value);
                    }
                }
            }
        }
        else if (node.Name == "Shop" && rightPers)
        {
            if (XmlNodeType.Element == node.NodeType)
            {
                foreach (XmlNode attrnode in map)
                {
                    if (attrnode.Name == "count")
                    {
                        int count = int.Parse(attrnode.Value);
                        cloth = new Cloth[count];

                        for (int i = 0; i < count; i++)
                        {
                            cloth[i] = new Cloth();
                        }
                    }
                }
            }
        }
    }

    public int getIndex(string clothName)
    {
        if (clothName == "none") return 0;

        for (int i = 0; i < cloth.Length; i++)
        {
            if (cloth[i].name == clothName)
                return i;
        }

        //Debug.LogError("debil");
        return 0;
    }

    public int getPrice(string clothName)
    {
        if (clothName == "none") return 0;

        for(int i = 0; i < cloth.Length; i++)
        {
            if (cloth[i].name == clothName)
                return cloth[i].price;
        }

        //Debug.LogError("debil");
        return 0;
    }

    public string[] getGridNames()
    {
        string[] gridNames = new string[cloth.Length];

        for(int i = 0; i < cloth.Length; i++)
        {
            gridNames[i] = cloth[i].name;
        }

        return gridNames;
    }

    public void TakeOff()
    {
        for (int i = 0; i < cloth.Length; i++)
        {
            if (cloth[i].entity)
                GameObject.Destroy(cloth[i].entity);
        }

        gameObject.transform.Find(rendererParent).renderer.material.mainTexture = InitTexture;
    }

    //public void TakeOff(string clothName)
    //{
    //    for (int i = 0; i < cloth.Length; i++)
    //    {
    //        if (cloth[i].entity && cloth[i].name == clothName)
    //            GameObject.Destroy(cloth[i].entity);
    //    }        
    //}

    public void Wear(string clothName)
    {
        if (clothName == "none") return;

        //Debug.Log(clothName);
        int prefabIndex = 0, i;
        ClothType type = DetermineType(clothName);

        if (type == ClothType.none) { Debug.Log("debil"); return; }

        if (type == ClothType.material)
        {
            for (i = 0; i < textures.Length; i++)
            {
                if (clothName == textures[i].name)
                {
                    prefabIndex = i;
                }
            }

            for (i = 0; i < cloth.Length; i++)
            {
                if (cloth[i].name == clothName)
                {
                    gameObject.transform.Find(rendererParent).renderer.material.mainTexture = textures[prefabIndex];
                    break;
                }
            }
        }
        else
        {
            DestroyWithType(type);

            for (i = 0; i < prefabs.Length; i++)
            {
                if (clothName == prefabs[i].name)
                {
                    prefabIndex = i;
                }
            }

            for (i = 0; i < cloth.Length; i++)
            {
                if (cloth[i].name == clothName)
                {
                    cloth[i].entity = NGUITools.AddChild(gameObject.transform.Find(cloth[i].parent).gameObject, prefabs[prefabIndex]);
                    //cloth[i].entity = GameObject.Instantiate(prefabs[prefabIndex]) as GameObject;
                    break;
                }
            }

            //cloth[i].entity.transform.parent = gameObject.transform.Find(cloth[i].parent);
            //cloth[i].entity.transform.localPosition = cloth[i].pos;
            cloth[i].entity.transform.localScale = prefabs[prefabIndex].transform.localScale;
            cloth[i].entity.transform.localRotation = prefabs[prefabIndex].transform.rotation;
        }
    }

    void DestroyWithType(ClothType t)
    {
        for (int i = 0; i < cloth.Length; i++)
        {
            if (cloth[i].type == t)
                if (cloth[i].entity)
                    GameObject.Destroy(cloth[i].entity);
        }
    }

    ClothType DetermineType(string name)
    {
        for (int i = 0; i < cloth.Length; i++)
        {
            if (cloth[i].name == name)
                return cloth[i].type;
        }
        return ClothType.none;
    }

    public int DetermineTypeIndex(string name)
    {
        for (int i = 0; i < cloth.Length; i++)
        {
            if (cloth[i].name == name)
                switch (cloth[i].type)
                {
                    case ClothType.head:
                        return 0;
                    case ClothType.material:
                        return 1;
                    case ClothType.body:
                        return 2;
                    case ClothType.legs:
                        return 3;
                }
                
        }
        return 0;
    }
}