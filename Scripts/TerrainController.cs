using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TerrainController : MonoBehaviour {


    public TerrainData _terrainData;
    public Terrain ter;
    public Material materialTemplate;
	public GameObject treePrefab;

    GameObject go;
    
    public float terSizeZ;

    //isDrawnTerrain
    GameObject isDrawnGO;
    Toggle isDrawnToggle;

    // castShadows
    GameObject isShadowsCastGO;
    Toggle isShadowsCastToggle;

    //Pixel Error
    Slider pixelErrorSlider;
    GameObject pixErrorSliderGo;
    GameObject pixErrorTextGo;
    InputField pixErrorInputField;

    //Base Map Dist.
    Slider baseMapDistSlider;
    GameObject baseMapDistSliderGo;
    GameObject baseMapDistTextGo;
    InputField baseMapDistInputField;

    // Thickness
    GameObject thicknessTextGo;
    InputField thicknessInputField;

    //Material
    GameObject materialGo;
    Dropdown materialDropdown;

    //Width
    GameObject terWidthGo;
    InputField terWidthInputField;

    //Height
    GameObject terHeightGo;
    InputField terHeightInputField;

    //Length
    GameObject terLengthGo;
    InputField terLengthInputField;

    //HeightMapResolution
    GameObject terHeightMapResGo;
    InputField terHeightMapResInputField;

    //DetailResolution
    GameObject detailResolutionGo;
    InputField detailResolutionInputField;

    //DetailResolution
    GameObject resPerPatchGo;
    InputField resPerPatchInputField;

    //ControlTextureResolution
    GameObject contTexResGo;
    InputField contTexResInputField;

    //BaseMapResolution
    GameObject baseMapResGo;
    InputField baseMapResInputField;

	//isDrawnFoliage
	GameObject isDrawnFoliageGO;
	Toggle isDrawnFoliageToggle;


    void Awake()
    {
       
    }
    

    // Use this for initialization
    void Start ()
    {
        //Initialize

        // isDrawnTerrain
        isDrawnGO = GameObject.Find("isDrawnToggle");
        isDrawnToggle = isDrawnGO.GetComponent<Toggle>();

		// isDrawnFoliage
		isDrawnFoliageGO = GameObject.Find("isDrawnFoliageToggle");
		isDrawnFoliageToggle = isDrawnGO.GetComponent<Toggle>();

        // isShadowsCast
        isShadowsCastGO = GameObject.Find("ShadowsCastToggle");
        isShadowsCastToggle = isShadowsCastGO.GetComponent<Toggle>();


        //Pixel Error
        pixErrorSliderGo = GameObject.Find("PixelErrorSlider");
        pixelErrorSlider = pixErrorSliderGo.GetComponent<Slider>();
        pixErrorTextGo = GameObject.Find("PixelErrorInputField");
        pixErrorInputField = pixErrorTextGo.GetComponent<InputField>();

        //Base Map Dist.
        baseMapDistSliderGo = GameObject.Find("BaseMapDistortionSlider");
        baseMapDistSlider = baseMapDistSliderGo.GetComponent<Slider>();
        baseMapDistTextGo = GameObject.Find("BaseMapDistortionInputField");
        baseMapDistInputField = baseMapDistTextGo.GetComponent<InputField>();

        //Thickness
        thicknessTextGo = GameObject.Find("ThicknessInputField");
        thicknessInputField = thicknessTextGo.GetComponent<InputField>();

        //Material
        materialGo = GameObject.Find("MaterialDropdown");
        materialDropdown = materialGo.GetComponent<Dropdown>();

        //Width
        terWidthGo = GameObject.Find("WidthInputField");
        terWidthInputField = terWidthGo.GetComponent<InputField>();

        //Height
        terHeightGo = GameObject.Find("HeightInputField");
        terHeightInputField = terHeightGo.GetComponent<InputField>();

        //Length
        terLengthGo = GameObject.Find("LengthInputField");
        terLengthInputField = terLengthGo.GetComponent<InputField>();

        //HeightmapResolution
        terHeightMapResGo = GameObject.Find("HeightmapResInputField");
        terHeightMapResInputField = terHeightMapResGo.GetComponent<InputField>();

        //DetailResolution
        detailResolutionGo = GameObject.Find("DetailResInputField");
        detailResolutionInputField = detailResolutionGo.GetComponent<InputField>();

        //Resolution Per Patch
        resPerPatchGo = GameObject.Find("ResPerPatchInputField");
        resPerPatchInputField = resPerPatchGo.GetComponent<InputField>();

        //ControlTextureResolution
        contTexResGo = GameObject.Find("ControlTextureInputField");
        contTexResInputField = contTexResGo.GetComponent<InputField>();

        //BaseMapResolution
        baseMapResGo = GameObject.Find("BaseMapResInputField");
        baseMapResInputField = baseMapResGo.GetComponent<InputField>();

    }


    public void InitializeTerrainData()
    {
        _terrainData = new TerrainData();
        const int size = 513;

        _terrainData.heightmapResolution = size;
        _terrainData.size = new Vector3(2000, 600, 2000);

        _terrainData.baseMapResolution = 512;
        _terrainData.SetDetailResolution(1024, 8);

        // AssetDatabase.CreateAsset(_terrainData, "Assets/New Terrain.asset"); Disabled since we won't use Unity Editor

    }

    public void CreateTerrain()
    {
        go = Terrain.CreateTerrainGameObject(_terrainData);
        // ter = go.GetComponent<Terrain>();
        ter = Terrain.activeTerrain;
    }

    public void TerrainHeightmapNotDrawn()
    {

        if (ter == null)
            return;

        if (ter.drawHeightmap == true)
        {

            ter.drawHeightmap = false;
        }

        else
        {
            ter.drawHeightmap = true;
        }

    }

	public void TerrainFoliageNotDrawn()
	{

		if (ter == null)
			return;

		if (ter.drawTreesAndFoliage == true)
		{

			ter.drawTreesAndFoliage = false;
		}

		else
		{
			ter.drawTreesAndFoliage = true;
		}

	}


    public void TerrainPixelErrorSlider()
    {
        if (ter == null)
            return;
        ter.heightmapPixelError = pixelErrorSlider.value;
        pixErrorInputField.text = pixelErrorSlider.value.ToString();
        
        

    }

    public void TerrainPixelErrorInput()
    {
        if (ter == null)
            return;
        ter.heightmapPixelError = float.Parse(pixErrorInputField.text);
        pixelErrorSlider.value = ter.heightmapPixelError;
    }


    public void TerrainBaseMapDistSlider()
    {
        if (ter == null)
            return;
        ter.basemapDistance = baseMapDistSlider.value;
        baseMapDistInputField.text = baseMapDistSlider.value.ToString();

    }

    public void TerrainBaseMapDistInput()
    {
        if (ter == null)
            return;
        ter.basemapDistance = float.Parse(baseMapDistInputField.text);
        baseMapDistSlider.value = ter.basemapDistance;
    }


    public void TerrainThicknessInput()
    {
        if (ter == null)
            return;
        _terrainData.thickness = float.Parse(thicknessInputField.text);
    }



    public void TerCastShadows()

    {

        if (ter == null)
            return;
        if (ter.castShadows == true)
        {
            ter.castShadows = false;
        }

        else
        {
            ter.castShadows = true;
        }

    }

    public void TerSetMaterial()
    {
        if (ter == null)
            return;

        if (materialDropdown.value == 0)
            ter.materialType = Terrain.MaterialType.BuiltInStandard;

        if (materialDropdown.value == 1)
            ter.materialType = Terrain.MaterialType.BuiltInLegacyDiffuse;

        if (materialDropdown.value == 2)
            ter.materialType = Terrain.MaterialType.BuiltInLegacySpecular;

        if (materialDropdown.value == 3)  
            ter.materialType = Terrain.MaterialType.Custom;
            ter.materialTemplate = materialTemplate;
    }

    public void TerSetHeightMapRes()
    {
        if (ter == null)
            return;
        _terrainData.heightmapResolution = int.Parse(terHeightMapResInputField.text);
    }


    public void TerrainSizeInput()
    {
        if (ter == null)
            return;
        _terrainData.size = new Vector3(float.Parse(terWidthInputField.text), float.Parse(terHeightInputField.text), float.Parse(terLengthInputField.text));
    }

    public void TerrainSetDetailResolution()
    {
        if (ter == null)
            return;
        _terrainData.SetDetailResolution(int.Parse(detailResolutionInputField.text), int.Parse(resPerPatchInputField.text));
    }

    public void TerrainSetControlTextureResolution()
    {
        if (ter == null)
            return;
        _terrainData.alphamapResolution= int.Parse(contTexResInputField.text);

    }

    public void TerrainSetBaseMapResolution()
    {
        if (ter == null)
            return;
        _terrainData.baseMapResolution = int.Parse(baseMapResInputField.text);

    }

	public void TerrainInstantiateTrees()

	{
		if (ter == null) 
			return;
		
		int posX = 0;
		int posZ = 0;
		int distance = 20;
		float terSizeX = _terrainData.size[0];
		float terSizeY = _terrainData.size[1];
		
		for (int i = 0; i < terSizeX; i++) 
		{
			for (int j = 0; j < terSizeY; j++)
			{
				Vector3 position = new Vector3(posX, 0, posZ);
				Instantiate (treePrefab, position, Quaternion.identity);
				posX = (posX + distance);
				posZ = (posZ + distance);
			}

		}

	}



    // Update is called once per frame
    void Update ()
    {
        if (ter == null)
        { 
            isDrawnToggle.interactable = false;
			isDrawnFoliageToggle.interactable = false;
            isShadowsCastToggle.interactable = false;
        }

        else
        {
            isDrawnToggle.interactable = true;
			isDrawnFoliageToggle.interactable = true;
            isShadowsCastToggle.interactable = true;

        }
    }
}
