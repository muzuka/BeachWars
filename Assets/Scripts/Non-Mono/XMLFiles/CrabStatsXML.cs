using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

[XmlRoot (ElementName = "health")]
public class Health
{
    [XmlAttribute (AttributeName = "name")]
    public string Name { get; set; }
    [XmlAttribute (AttributeName = "id")]
    public string Id { get; set; }
    [XmlText]
    public string Text { get; set; }
}

[XmlRoot (ElementName = "movementSpeed")]
public class MovementSpeed
{
    [XmlAttribute (AttributeName = "name")]
    public string Name { get; set; }
    [XmlAttribute(AttributeName = "id")]
    public string Id { get; set; }
    [XmlText]
    public string Text { get; set; }
}

[XmlRoot(ElementName = "attackDamage")]
public class AttackDamage
{
    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; }
    [XmlAttribute(AttributeName = "id")]
    public string Id { get; set; }
    [XmlText]
    public string Text { get; set; }
}

[XmlRoot(ElementName = "attackSpeed")]
public class AttackSpeed
{
    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; }
    [XmlAttribute(AttributeName = "id")]
    public string Id { get; set; }
    [XmlText]
    public string Text { get; set; }
}

[XmlRoot(ElementName = "scaleFactor")]
public class ScaleFactor
{
    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; }
    [XmlAttribute(AttributeName = "id")]
    public string Id { get; set; }
    [XmlElement (ElementName = "x")]
    public string x { get; set; }
    [XmlElement (ElementName = "y")]
    public string y { get; set; }
    [XmlElement (ElementName = "z")]
    public string z { get; set; }
}

[XmlRoot (ElementName = "crabStats")]
public class CrabStats
{
    [XmlElement (ElementName = "health")]
    public List<Health> Health { get; set; }
    [XmlElement (ElementName = "movementSpeed")]
    public List<MovementSpeed> MovementSpeed { get; set; }
    [XmlElement (ElementName = "attackDamage")]
    public List<AttackDamage> AttackDamage { get; set; }
    [XmlElement (ElementName = "attackSpeed")]
    public List<AttackSpeed> AttackSpeed { get; set; }
    [XmlElement (ElementName = "scaleFactor")]
    public List<ScaleFactor> ScaleFactor { get; set; }

    public static CrabStats load(string path)
    {
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CrabStats));
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                return serializer.Deserialize(stream) as CrabStats;
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Exception loading config file: " + e);

            return null;
        }
    }
}
