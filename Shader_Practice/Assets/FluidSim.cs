using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class FluidSim : MonoBehaviour
{

    public RenderTexture ouputTexture;
    RenderTexture DensityTexture;
    [SerializeField]
    ComputeShader DensityCompute;
    [SerializeField]
    ComputeShader VelocityCompute;
    public int resolutionX;
    public int resolutionY;
    public int resolution;
    [SerializeField]
    float diffusion;
    fluid[] data;
    [SerializeField]
    Camera cam;
    ComputeBuffer dataBuffer;
    public fluid[] velocityData; 
    public TMP_Text text;
    
    public GameObject arrow;
    public GameObject[] arrows;
    const int DiffuseDensityKernel = 0;
    const int AdvectDensityKernel= 1;
    const int SwapDensityKernel = 2;
    const int DiffuseVelocityKernel = 3;
    const int AdvectVelocityKernel = 4;
    const int ProjectKernel = 5;
    const int SwapVelocityKernel = 6;
    const int DrawKernel = 7;
    const int SwapPKernel = 8;
    const int findDiverenceKernel = 9;
    public Vector2[,] spawnGrid;
    // Start is called before the first frame update
    public struct fluid{
        public float densityOld;
        public float densityNew;
        public Vector2 velocityOld;
        public Vector2 velocityNew;
        public float p_new;
        public float p_old;
        public float delV;
    }
    private void OnDestroy() {
        dataBuffer.Release();
    }
    void Start()
    {
        spawnGrid = new Vector2[resolutionX, resolutionY];
        if(DensityTexture == null){
            DensityTexture = new RenderTexture(resolutionX,resolutionY,32);
            DensityTexture.enableRandomWrite = true;
            
            DensityTexture.Create();
        }
        resolution = resolutionX*resolutionY; 
        velocityData = new fluid[resolution];
        data = new fluid[resolution];
        /*
        arrows = new GameObject[resolution];*/
        for(int i = 0;i<resolution;i++){
            data[i].densityOld = 0; 
            data[i].densityNew = 0;
            data[i].velocityOld = new Vector2(0,0);
            data[i].velocityNew = new Vector2(0,0);
            data[i].p_new = 0;
            data[i].p_old = 0;
            data[i].delV = 0;
            /*
            GameObject arrowImage = Instantiate(arrow);
            arrowImage.GetComponent<RectTransform>().localPosition = new Vector3(i%resolutionX,i/resolutionX,0);
            arrows[i] = arrowImage;*/
        }
        dataBuffer = new ComputeBuffer(resolution,sizeof(float)+2*sizeof(float)+sizeof(float)+sizeof(float)+2*sizeof(float)+sizeof(float)+sizeof(float));
        dataBuffer.SetData(data);
        DensityCompute.SetBuffer(DiffuseDensityKernel,"fluidData",dataBuffer);
        DensityCompute.SetBuffer(AdvectDensityKernel,"fluidData",dataBuffer);
        DensityCompute.SetBuffer(SwapDensityKernel,"fluidData",dataBuffer);
        DensityCompute.SetBuffer(DiffuseVelocityKernel,"fluidData",dataBuffer);
        DensityCompute.SetBuffer(AdvectVelocityKernel,"fluidData",dataBuffer);
        DensityCompute.SetBuffer(ProjectKernel,"fluidData",dataBuffer);
        DensityCompute.SetBuffer(SwapVelocityKernel,"fluidData",dataBuffer);
        DensityCompute.SetBuffer(DrawKernel,"fluidData",dataBuffer);
        DensityCompute.SetBuffer(SwapPKernel,"fluidData",dataBuffer);
        DensityCompute.SetBuffer(findDiverenceKernel,"fluidData",dataBuffer);
        DensityCompute.SetTexture(DrawKernel,"Density",DensityTexture);
        DensityCompute.SetVector("densityAdd",new Vector2(100,100));
        DensityCompute.SetFloat("diffusionFactor",diffusion);
        DensityCompute.SetFloat("width",resolutionX);
        DensityCompute.SetFloat("height",resolutionY);
        DensityCompute.SetVector ("_Time", Shader.GetGlobalVector ("_Time"));
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Mouse0)){
            Vector3 mouse_pos_pixel_coord = Input.mousePosition;
            Vector2 mouse_pos_normalized  = cam.ScreenToViewportPoint(mouse_pos_pixel_coord);
            mouse_pos_normalized  = new Vector2(Mathf.Clamp01(mouse_pos_normalized.x), Mathf.Clamp01(mouse_pos_normalized.y));
            Vector2 mouse_sim = new Vector2((int)(mouse_pos_normalized.x*resolutionX),(int)(mouse_pos_normalized.y*resolutionY));
            //Vector2 velocity = (mouse_sim-prevPosition);
            //Debug.Log(velocity);
            //computeShader.SetFloat("diffuseOn",1);
            //computeShader.SetVector("pos",mouse_sim);
            //computeShader.SetVector("vel",velocity);
            //prevPosition = mouse_sim;
            DensityCompute.SetVector("densityAdd",mouse_sim);
            DensityCompute.SetBool("enabled",true);
        }
        if(!Input.anyKey){
            DensityCompute.SetBool("enabled",false);
        }
        densityAlgorithm();
        velocityAlgorithm();
        Draw();

        Graphics.Blit(DensityTexture,ouputTexture);
        /*
        dataBuffer.GetData(velocityData);
        text.text = "";
        for(int x = 0;x<resolutionX;x++){
            for(int y = 0; y< resolutionY;y++){
                spawnGrid[x,y] = velocityData[x * resolutionX + y ].velocityNew;
                //Debug.DrawLine(new Vector3(x,y,100),new Vector3(x+velocityData[x * resolutionX + y ].velocityNew.x,y+velocityData[x * resolutionX + y ].velocityNew.y,100));
                text.text += velocityData[x * resolutionX + y ].delV;
                text.text += " ";
            }
            text.text += "\n";
        }*/
        
        
    }
    void densityAlgorithm(){
        for(int i = 0;i<20;i++){
            DensityCompute.Dispatch(DiffuseDensityKernel,resolutionX/8,resolutionY/8,1);
            DensityCompute.Dispatch(SwapDensityKernel,resolutionX/8,resolutionY/8,1);
        }
        
        DensityCompute.Dispatch(AdvectDensityKernel,resolutionX/8,resolutionY/8,1);
        DensityCompute.Dispatch(SwapDensityKernel,resolutionX/8,resolutionY/8,1);
        
    }
    void velocityAlgorithm(){
        for(int i = 0;i<20;i++){
            DensityCompute.Dispatch(DiffuseVelocityKernel,resolutionX/8,resolutionY/8,1);
            DensityCompute.Dispatch(SwapVelocityKernel,resolutionX/8,resolutionY/8,1);
        }
        for(int i = 0;i<20;i++){
            DensityCompute.Dispatch(findDiverenceKernel,resolutionX/8,resolutionY/8,1);
            DensityCompute.Dispatch(SwapPKernel,resolutionX/8,resolutionY/8,1);
        }
        //DensityCompute.Dispatch(findDiverenceKernel,resolutionX/8,resolutionY/8,1);
        //DensityCompute.Dispatch(SwapPKernel,resolutionX/8,resolutionY/8,1);
        DensityCompute.Dispatch(ProjectKernel,resolutionX/8,resolutionY/8,1);
        
        
        DensityCompute.Dispatch(SwapVelocityKernel,resolutionX/8,resolutionY/8,1);
        
        DensityCompute.Dispatch(AdvectVelocityKernel,resolutionX/8,resolutionY/8,1);
        
        
        for(int i = 0;i<20;i++){
            DensityCompute.Dispatch(findDiverenceKernel,resolutionX/8,resolutionY/8,1);
            DensityCompute.Dispatch(SwapPKernel,resolutionX/8,resolutionY/8,1);
        }
        //DensityCompute.Dispatch(findDiverenceKernel,resolutionX/8,resolutionY/8,1);
        //DensityCompute.Dispatch(SwapPKernel,resolutionX/8,resolutionY/8,1);
        DensityCompute.Dispatch(ProjectKernel,resolutionX/8,resolutionY/8,1);
        
        DensityCompute.Dispatch(SwapVelocityKernel,resolutionX/8,resolutionY/8,1); 
    }
    void Draw(){
        DensityCompute.Dispatch(DrawKernel,resolutionX/8,resolutionY/8,1);
    }
}
