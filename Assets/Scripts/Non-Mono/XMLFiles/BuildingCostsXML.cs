
using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

[XmlRoot (ElementName = "woodCost")]
public class WoodCost
{
	[XmlAttribute (AttributeName = "name")]
	public string Name { get; set; }
	[XmlText]
	public string Text { get; set; }
}

[XmlRoot (ElementName = "stoneCost")]
public class StoneCost
{
	[XmlAttribute (AttributeName = "name")]
	public string Name { get; set; }
	[XmlText]
	public string Text { get; set; }
}

[XmlRoot (ElementName = "buildingCosts")]
public class BuildingCosts
{
	[XmlElement (ElementName = "woodCost")]
	public List<WoodCost> WoodCost { get; set; }
	[XmlElement (ElementName = "stoneCost")]
	public List<StoneCost> StoneCost { get; set; }

	public static BuildingCosts load(string path) 
	{
		try
        {
			XmlSerializer serializer = new XmlSerializer (typeof (BuildingCosts));
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                return serializer.Deserialize(stream) as BuildingCosts;
            }
		}
        catch (Exception e)
        {
			UnityEngine.Debug.LogError ("Exception loading config file: " + e);

			return null;
		}
	}
}