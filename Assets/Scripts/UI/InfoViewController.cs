using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoViewController : MonoBehaviour
{
    // Multi-Select UI elements
    public RectTransform _multiUITransform;
    
    // Single-Select UI elements
    public RectTransform _singleUITransform;

    public RawImage[] MiniProfiles;
    
    public Text LabelText;

    public Text StoneCount;

    public Text WoodCount;

    public Text LevelText;

    public Text CrabLevelText;

    public Slider HealthSlider;

    public Slider ExpSlider;

    public RawImage SelectedImage;

    public RawImage InvSlot1;

    public RawImage InvSlot2;

    public RawImage InvSlot3;

    public Button CraftButton;

    public Button SubCraftButton;

    public Text SpearText;

    public Text HammerText;

    public Text BowText;

    public Text ShieldText;

    public Button DismantleButton;
    
    const string _emptyString = "Nothing";
    const int _miniProfileCount = 16;
    
    DebugComponent _debug;
    
    // Start is called before the first frame update
    void Start()
    {
	    SelectedImage.texture = null;
	    CraftButton.gameObject.SetActive(false);
	    SubCraftButton.gameObject.SetActive(false);

	    LabelText.text = _emptyString;
	    StoneCount.text = "";
	    WoodCount.text = "";

	    _debug = GetComponent<DebugComponent>();
        
	    if (!GUIHookedIn())
		    Debug.Log("GUI isn't completely connected.");
    }

    public void SetViewMode(bool isMulti)
    {
	    _multiUITransform.gameObject.SetActive(isMulti);
	    _singleUITransform.gameObject.SetActive(!isMulti);
    }

    /// <summary>
    /// Are all GUI components instantiated?
    /// </summary>
    /// <returns><c>true</c>, if GUI is hooked in, <c>false</c> otherwise.</returns>
    public bool GUIHookedIn()
    {
	    bool count = (StoneCount && WoodCount);
	    bool invSlots = (InvSlot1 && InvSlot2 && InvSlot3);
	    bool buttons = (CraftButton && SubCraftButton && DismantleButton);
	    bool multiSelect = AreAllProfilesHookedIn();
	    bool uiTransforms = (_multiUITransform && _singleUITransform);

	    if (!LabelText)
	    {
		    _debug.LogMessage("LabelText isn't hooked up!");
		    return false;
	    }

	    if (!count)
	    {
		    _debug.LogMessage("Stone or wood count isn't hooked up!");
		    return false;
	    }

	    if (!HealthSlider)
	    {
		    _debug.LogMessage("Health slider isn't hooked up!");
		    return false;
	    }

	    if (!SelectedImage)
	    {
		    _debug.LogMessage("Selected image isn't hooked up!");
		    return false;
	    }

	    if (!invSlots)
	    {
		    _debug.LogMessage("One of the inventory slots isn't hooked up!");
		    return false;
	    }

	    if (!buttons)
	    {
		    _debug.LogMessage("A button isn't hooked up!");
		    return false;
	    }

	    if (!multiSelect)
	    {
		    _debug.LogMessage("A multi-select element isn't hooked up!");
		    return false;
	    }

	    if (!uiTransforms)
	    {
		    _debug.LogMessage("A transform isn't hooked up!");
		    return false;
	    }

	    return true;
    }

    bool AreAllProfilesHookedIn()
    {
	    for (int i = 0; i < _miniProfileCount; i++)
	    {
		    if (MiniProfiles[i] == null)
		    {
			    _debug.LogMessage("Profile " + i + " isn't hooked up");
			    return false;
		    }
	    }

	    return true;
    }

    public void PopulateMiniProfiles(List<GameObject> selectedList)
    {
	    if (selectedList.Count <= MiniProfiles.Length) 
	    {
		    for (int i = 0; i < selectedList.Count; i++)
		    {
			    selectedList[i].GetComponent<Attackable>().SetHealth(MiniProfiles[i].GetComponentInChildren<Slider>()); 
		    }
	    }
    }
    
    /// <summary>
    /// Sets the label text.
    /// </summary>
    /// <param name="selected">Selected object.</param>
    public void SetLabel(GameObject selected)
    {
	    if (IdUtility.IsCrab(selected))
	    {
		    LabelText.text = selected.GetComponent<CrabController>().Type.ToString().ToLower() + " crab"; 
	    }
	    else if (selected.CompareTag(Tags.Ghost))
	    {
		    LabelText.text = selected.tag + " " + selected.GetComponent<GhostBuilder>().Original.tag; 
	    }
	    else
	    {
		    LabelText.text = selected.tag; 
	    }

	    if (!Player.Instance.CanCommand && !IdUtility.IsResource(selected))
	    {
		    LabelText.text = "Enemy " + LabelText.text; 
	    }
    }

    public void SetCrabView()
    {
	    HealthSlider.gameObject.SetActive(true);
	    InvSlot1.gameObject.SetActive(true);
	    InvSlot2.gameObject.SetActive(true);
	    InvSlot3.gameObject.SetActive(true);
	    CrabLevelText.gameObject.SetActive(true);
	    ExpSlider.gameObject.SetActive(true);
    }

    public void SetCastleView()
    {
	    HealthSlider.gameObject.SetActive(true);
	    LevelText.gameObject.SetActive(true);
	    WoodCount.gameObject.SetActive(true);
	    StoneCount.gameObject.SetActive(true);
    }

    public void SetGhostView()
    {
	    WoodCount.gameObject.SetActive(true);
	    StoneCount.gameObject.SetActive(true);
    }

    public void SetArmouryView()
    {
	    SpearText.gameObject.SetActive(true);
	    HammerText.gameObject.SetActive(true);
	    BowText.gameObject.SetActive(true);
	    ShieldText.gameObject.SetActive(true);
	    SetBuildingView();
    }

    public void SetBuildingView()
    {
	    HealthSlider.gameObject.SetActive(true);
	    DismantleButton.gameObject.SetActive(true);
    }

    public void SetMultiView(Player player)
    {
	    if (player.SelectedList.Count > MiniProfiles.Length)
	    {
		    for (int i = 0; i < MiniProfiles.Length; i++)
		    {
			    MiniProfiles[i].gameObject.SetActive(true); 
		    }
	    }
	    else 
	    {
		    for (int i = 0; i < player.SelectedList.Count; i++)
		    {
			    MiniProfiles[i].gameObject.SetActive(true); 
		    }
	    }
	    
	    for (int i = 0; i < player.SelectedList.Count; i++)
	    {
		    if (i >= _miniProfileCount)
		    {
			    break;
		    }

		    _debug.LogMessage(i.ToString());
		    MiniProfiles[i].texture = Resources.Load<Texture>("Textures/" + player.SelectedList[i].tag);

		    player.SelectedList[i].GetComponent<Attackable>().SetHealth(MiniProfiles[i].GetComponentInChildren<Slider>());
	    }
    }
    
    /// <summary>
    /// Gets the inv slot image.
    /// </summary>
    /// <returns>The inv slot image.</returns>
    /// <param name="slot">Slot number.</param>
    public void SetInvView(string[] inv)
    {
	    InvSlot1.texture = GetTexture(inv[0]);
	    InvSlot2.texture = GetTexture(inv[1]);
	    InvSlot3.texture = GetTexture(inv[2]);
    }
    
    /// <summary>
    /// Gets the texture given the name.
    /// </summary>
    /// <returns>The texture.</returns>
    /// <param name="textureName">texture name.</param>
    Texture GetTexture(string textureName)
    {
	    Texture newTexture = Resources.Load<Texture>("Textures/Empty");
	    if (textureName == null)
	    {
		    return newTexture; 
	    }

	    newTexture = Resources.Load<Texture>("Textures/" + textureName);
	    if (!newTexture)
	    {
		    newTexture = Resources.Load<Texture>("Textures/Empty"); 
	    }

	    return newTexture;
    }
    
    /// <summary>
    /// Sets the profile button.
    /// </summary>
    /// <param name="pos">Position.</param>
    public void SetProfileButton(int pos)
    {
	    Player player = Player.Instance;
	    GameObject crab = player.SelectedList[pos];

	    _debug.LogMessage(player.SelectedList.Count + " trying to access " + pos);
		
	    player.Deselect();
	    player.SelectedList.Add(crab);
	    player.HaloList.Add(Instantiate(Resources.Load<GameObject>("Prefabs/GUI/HaloCanvas")));
	    player.Select(crab);
    }
    
    /// <summary>
    /// Deactivates the GUI components.
    /// </summary>
    public void DeactivateGUIComponents()
    {
        LabelText.gameObject.SetActive(false);
        HealthSlider.gameObject.SetActive(false);
        SelectedImage.gameObject.SetActive(false);
        InvSlot1.gameObject.SetActive(false);
        InvSlot2.gameObject.SetActive(false);
        InvSlot3.gameObject.SetActive(false);
        WoodCount.gameObject.SetActive(false);
        StoneCount.gameObject.SetActive(false);
        LevelText.gameObject.SetActive(false);
        CraftButton.gameObject.SetActive(false);
        SubCraftButton.gameObject.SetActive(false);
        CrabLevelText.gameObject.SetActive(false);
        ExpSlider.gameObject.SetActive(false);
        SpearText.gameObject.SetActive(false);
        HammerText.gameObject.SetActive(false);
        BowText.gameObject.SetActive(false);
        ShieldText.gameObject.SetActive(false);
        DismantleButton.gameObject.SetActive(false);

        for (int i = 0; i < MiniProfiles.Length; i++)
        {
            MiniProfiles[i].gameObject.SetActive(false); 
        }
    }
}
