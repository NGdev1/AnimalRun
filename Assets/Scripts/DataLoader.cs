//#define PC

using UnityEngine;
using System.Collections;
using System.Xml;

public class DataLoader
{
    public SpawnSequence[] spSeq;
    public SpawnSequence[] forestSeq;

    private int spSeqId;
    private int id;
    private int line;
    private int carId;
    private bool inTheForest;

    public void LoadData()
    {
        init();
        XmlDocument xmlDoc = new XmlDocument();
#if PC
        xmlDoc.Load(Application.dataPath + "/SpawnData.xml");
#else
        TextAsset xmlAsset = Resources.Load("SpawnData") as TextAsset;
        xmlDoc.LoadXml(xmlAsset.text);
#endif

        LoadTree(xmlDoc.DocumentElement);
    }

    void init()
    {
        id = 0;
        line = 0;
        carId = 0;
        inTheForest = false;
    }

    public DataLoader()
    {
        init();
        LoadData();
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
        int i = 0;

        if (node.Name == "Podstavi")
        {
            if (XmlNodeType.Element == node.NodeType)
            {
                foreach (XmlNode attrnode in map)
                {
                    if (attrnode.Name == "count")
                    {
                        int count = int.Parse(attrnode.Value);
                        spSeq = new SpawnSequence[count];
                        spSeqId = -1;
                    }
                }
            }
        }

        if(node.Name == "LesoPodstavi")
        {
            inTheForest = true;
            if (XmlNodeType.Element == node.NodeType)
            {
                foreach (XmlNode attrnode in map)
                {
                    if (attrnode.Name == "count")
                    {
                        int count = int.Parse(attrnode.Value);
                        forestSeq = new SpawnSequence[count];
                        spSeqId = -1;
                    }
                }
            }
        }

        if (node.Name == "SpawnSequence")
        {
            spSeqId++;
            id = -1;
            if (XmlNodeType.Element == node.NodeType)
            {
                foreach (XmlNode attrnode in map)
                {
                    if (attrnode.Name == "count")
                    {
                        int count = int.Parse(attrnode.Value);
                        spSeq[spSeqId] = new SpawnSequence(count);

                        for (; i < count; i++)
                        {
                            spSeq[spSeqId].spawnData[i] = new SpawnData();
                        }
                    }
                    else if (attrnode.Name == "free")
                    {
                        spSeq[spSeqId].free = parseBoolean(attrnode.Value);
                    }
                }
            }
        }
        if (node.Name == "ForestSequence")
        {
            spSeqId++;
            id = -1;
            if (XmlNodeType.Element == node.NodeType)
            {
                foreach (XmlNode attrnode in map)
                {
                    if (attrnode.Name == "count")
                    {
                        int count = int.Parse(attrnode.Value);
                        forestSeq[spSeqId] = new SpawnSequence(count);
                        for (; i < count; i++)
                        {
                            forestSeq[spSeqId].spawnData[i] = new SpawnData();
                        }
                    }
                    else if (attrnode.Name == "free")
                    {
                        forestSeq[spSeqId].free = parseBoolean(attrnode.Value);
                    }
                }
            }
        }
        else if (node.Name == "SpawnData")
        {
            id++;
            if (XmlNodeType.Element == node.NodeType)
            {
                foreach (XmlNode attrnode in map)
                {
                    if (attrnode.Name == "time")
                    {
                        if(inTheForest)
                            forestSeq[spSeqId].spawnData[id].time = float.Parse(attrnode.Value);
                        else
                            spSeq[spSeqId].spawnData[id].time = float.Parse(attrnode.Value);
                    }
                }
            }
        }
        else if (node.Name == "EntityData")
        {
            carId = 0;
            if (XmlNodeType.Element == node.NodeType)
            {
                foreach (XmlNode attrnode in map)
                {
                    if (attrnode.Name == "line")
                    {
                        line = int.Parse(attrnode.Value);
                    }
                    else if (attrnode.Name == "count")
                    {
                        int count = int.Parse(attrnode.Value);
                        forestSeq[spSeqId].spawnData[id].lineData[line] = new LineData(count);

                        for (; i < count; i++)
                        {
                            forestSeq[spSeqId].spawnData[id].lineData[line].carData[i] = new CarData();
                        }
                    }
                }
            }
        }
        else if (node.Name == "CarData")
        {
            carId = 0;
            if (XmlNodeType.Element == node.NodeType)
            {
                foreach (XmlNode attrnode in map)
                {
                    if (attrnode.Name == "line")
                    {
                        line = int.Parse(attrnode.Value);
                    }
                    else if (attrnode.Name == "count")
                    {
                        int count = int.Parse(attrnode.Value);
                        spSeq[spSeqId].spawnData[id].lineData[line] = new LineData(count);

                        for (; i < count; i++)
                        {
                            spSeq[spSeqId].spawnData[id].lineData[line].carData[i] = new CarData();
                        }
                    }
                }
            }
        }
        else if (node.Name == "Car")
        {
            if (XmlNodeType.Element == node.NodeType)
            {
                foreach (XmlNode attrnode in map)
                {
                    if (attrnode.Name == "speed")
                    {
                        float speed = float.Parse(attrnode.Value);
                        spSeq[spSeqId].spawnData[id].lineData[line].carData[carId].carspeed = speed;
                    }
                    else if (attrnode.Name == "dist")
                    {
                        float dist = float.Parse(attrnode.Value);
                        spSeq[spSeqId].spawnData[id].lineData[line].carData[carId].dist = dist;
                    }
                    else if (attrnode.Name == "type")
                    {
                        Type type = parseType(attrnode.Value);
                        spSeq[spSeqId].spawnData[id].lineData[line].carData[carId].type = type;
                    }
                    else if (attrnode.Name == "bonusId")
                    {
                        int bonusId = int.Parse(attrnode.Value);
                        spSeq[spSeqId].spawnData[id].lineData[line].carData[carId].bonusId = bonusId;
                    }
                }
            }
            carId++;
        }
        else if (node.Name == "Entity")
        {
            if (XmlNodeType.Element == node.NodeType)
            {
                foreach (XmlNode attrnode in map)
                {
                    if (attrnode.Name == "speed")
                    {
                        float speed = float.Parse(attrnode.Value);
                        forestSeq[spSeqId].spawnData[id].lineData[line].carData[carId].carspeed = speed;
                    }
                    else if (attrnode.Name == "dist")
                    {
                        float dist = float.Parse(attrnode.Value);
                        forestSeq[spSeqId].spawnData[id].lineData[line].carData[carId].dist = dist;
                    }
                    else if (attrnode.Name == "type")
                    {
                        Type type = parseType(attrnode.Value);
                        forestSeq[spSeqId].spawnData[id].lineData[line].carData[carId].type = type;
                    }
                    else if(attrnode.Name == "bonusId")
                    {
                        int bonusId = int.Parse(attrnode.Value);
                        forestSeq[spSeqId].spawnData[id].lineData[line].carData[carId].bonusId = bonusId;
                    }
                }
            }
            carId++;
        }
    }

    Type parseType(string text)
    {
        switch (text)
        {
            case "mega": return Type.mega;
            case "megamega": return Type.megamega;
            case "big": return Type.big;
            case "car": return Type.car;
            case "lift": return Type.lift;
            case "barrier": return Type.barrier;
            case "empty": return Type.empty;
            case "moto": return Type.moto;
            case "Fbig": return Type.Fbig;
            default: return Type.car;
        }
    }

    bool parseBoolean(string text)
    {
        if (text == "true") return true;
        else return false;
    }
}