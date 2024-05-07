using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    // Start is called before the first frame update
    public struct fluid{
        public float densityOld;
        public float densityNew;
        public Vector2 velocityOld;
        public Vector2 velocityNew;
        public float p;
    }
    private void OnDestroy() {
        dataBuffer.Release();
    }
    void Start()
    {
        if(DensityTexture == null){
            DensityTexture = new RenderTexture(resolutionX,resolutionY,32);
            DensityTexture.enableRandomWrite = true;
            
            DensityTexture.Create();
        }
        resolution = resolutionX*resolutionY; 
        data = new fluid[resolution];
        for(int i = 0;i<resolution;i++){
            data[i].densityOld = 0; 
            data[i].densityNew = 0;
            data[i].velocityOld = new Vector2(0,0);
            data[i].velocityNew = new Vector2(0,0);
            data[i].p = 0;
        }
        dataBuffer = new ComputeBuffer(resolution,sizeof(float)+2*sizeof(float)+sizeof(float)+sizeof(float)+2*sizeof(float));
        dataBuffer.SetData(data);
        DensityCompute.SetBuffer(0,"fluidData",dataBuffer);
        DensityCompute.SetBuffer(1,"fluidData",dataBuffer);
        DensityCompute.SetBuffer(2,"fluidData",dataBuffer);
        DensityCompute.SetBuffer(3,"fluidData",dataBuffer);
        DensityCompute.SetBuffer(4,"fluidData",dataBuffer);
        DensityCompute.SetBuffer(5,"fluidData",dataBuffer);
        DensityCompute.SetBuffer(6,"fluidData",dataBuffer);
        DensityCompute.SetTexture(2,"Density",DensityTexture);
        DensityCompute.SetVector("densityAdd",new Vector2(100,100));
        DensityCompute.SetFloat("diffusionFactor",diffusion);
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Mouse0)){
            Vector3 mouse_pos_pixel_coord = Input.mousePosition;
            Vector2 mouse_pos_normalized  = cam.ScreenToViewportPoint(mouse_pos_pixel_coord);
            mouse_pos_normalized  = new Vector2(Mathf.Clamp01(mouse_pos_normalized.x), Mathf.Clamp01(mouse_pos_normalized.y));
            Vector2 mouse_sim = new Vector2((int)(mouse_pos_normalized.x*1920),(int)(mouse_pos_normalized.y*1080));
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

        Graphics.Blit(DensityTexture,ouputTexture);
    }
    void densityAlgorithm(){
        DensityCompute.Dispatch(0,resolutionX/8,resolutionY/8,1);
        DensityCompute.Dispatch(2,resolutionX/8,resolutionY/8,1);
        DensityCompute.Dispatch(1,resolutionX/8,resolutionY/8,1);
        DensityCompute.Dispatch(2,resolutionX/8,resolutionY/8,1);
    }
    void velocityAlgorithm(){
        DensityCompute.Dispatch(3,resolutionX/8,resolutionY/8,1);
        DensityCompute.Dispatch(6,resolutionX/8,resolutionY/8,1);
        DensityCompute.Dispatch(5,resolutionX/8,resolutionY/8,1);
        DensityCompute.Dispatch(6,resolutionX/8,resolutionY/8,1);
        DensityCompute.Dispatch(4,resolutionX/8,resolutionY/8,1);
        DensityCompute.Dispatch(5,resolutionX/8,resolutionY/8,1);
    }
}
