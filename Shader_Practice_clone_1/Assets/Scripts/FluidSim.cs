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
    public int N;
    public float dt;
    public int resolution;
    [SerializeField]
    float Densitydiffusion;
    [SerializeField]
    float Velocitydiffusion;
    fluid[] data;
    [SerializeField]
    Camera cam;
    public float[] densityData;
    ComputeBuffer dataBuffer;
    ComputeBuffer densityNewBuffer;
    ComputeBuffer densityOldBuffer;
    ComputeBuffer velocityNewBufferx;
    ComputeBuffer velocityOldBufferx;
    ComputeBuffer velocityNewBuffery;
    ComputeBuffer velocityOldBuffery;
    ComputeBuffer pBuffer;
    ComputeBuffer delVBuffer;


    public TMP_Text text;
    
    const int DiffuseDensityKernel = 0;
    const int AdvectDensityKernel= 1;
    const int DiffuseVelocityKernel = 2;
    const int AdvectVelocityKernel = 3;
    const int findDiverenceKernel = 4;
    const int solvePKernel = 5;
    const int gradientKernel = 6;
    const int SwapVelocityKernel = 7;
    const int DrawKernel = 8;
    
    
    

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
        densityNewBuffer.Release();
        densityOldBuffer.Release();
        velocityNewBufferx.Release();
        velocityOldBufferx.Release();
        velocityNewBuffery.Release();
        velocityOldBuffery.Release();
        pBuffer.Release();
        delVBuffer.Release();
    }
    void Start()
    {
        if(DensityTexture == null){
            DensityTexture = new RenderTexture(N,N,32);
            DensityTexture.enableRandomWrite = true;
            
            DensityTexture.Create();
        }
        resolution = N*N; 
        densityData = new float[resolution];
        float[] densityNew = new float[resolution];
        float[] densityOld = new float[resolution];
        float[] velocityNewx = new float[resolution];
        float[] velocityOldx = new float[resolution];
        float[] velocityNewy = new float[resolution];
        float[] velocityOldy = new float[resolution];
        float[] p = new float[resolution];
        float[] delV = new float[resolution]; 
        data = new fluid[resolution];
        
        for(int i = 0;i<resolution;i++){
            data[i].densityOld = 0; 
            data[i].densityNew = 0;
            data[i].velocityOld = new Vector2(0,0);
            data[i].velocityNew = new Vector2(0,0);
            data[i].p_new = 0;
            data[i].p_old = 0;
            data[i].delV = 0;
        }
        dataBuffer = new ComputeBuffer(resolution,sizeof(float)+2*sizeof(float)+sizeof(float)+sizeof(float)+2*sizeof(float)+sizeof(float)+sizeof(float));
        dataBuffer.SetData(data);
        
        densityNewBuffer = new ComputeBuffer(resolution,sizeof(float));
        densityNewBuffer.SetData(densityNew);
        densityOldBuffer = new ComputeBuffer(resolution,sizeof(float));
        densityOldBuffer.SetData(densityOld);
        velocityNewBufferx = new ComputeBuffer(resolution,sizeof(float));
        velocityNewBufferx.SetData(velocityNewx);
        velocityOldBufferx = new ComputeBuffer(resolution,sizeof(float));
        velocityOldBufferx.SetData(velocityOldx);
        velocityNewBuffery = new ComputeBuffer(resolution,sizeof(float));
        velocityNewBuffery.SetData(velocityNewy);
        velocityOldBuffery = new ComputeBuffer(resolution,sizeof(float));
        velocityOldBuffery.SetData(velocityOldy);
        pBuffer = new ComputeBuffer(resolution,sizeof(float));
        pBuffer.SetData(p);
        delVBuffer = new ComputeBuffer(resolution,sizeof(float));
        delVBuffer.SetData(delV);

        DensityCompute.SetBuffer(DiffuseDensityKernel,"densityNew",densityNewBuffer);
        DensityCompute.SetBuffer(DiffuseDensityKernel,"densityOld",densityOldBuffer);
        DensityCompute.SetBuffer(DiffuseDensityKernel,"velocityNewx",velocityNewBufferx);
        DensityCompute.SetBuffer(DiffuseDensityKernel,"velocityNewy",velocityNewBuffery);

        DensityCompute.SetBuffer(AdvectDensityKernel,"densityNew",densityNewBuffer);
        DensityCompute.SetBuffer(AdvectDensityKernel,"densityOld",densityOldBuffer);
        DensityCompute.SetBuffer(AdvectDensityKernel,"velocityNewx",velocityNewBufferx);
        DensityCompute.SetBuffer(AdvectDensityKernel,"velocityNewy",velocityNewBuffery);
        DensityCompute.SetBuffer(AdvectDensityKernel,"velocityOldx",velocityOldBufferx);
        DensityCompute.SetBuffer(AdvectDensityKernel,"velocityOldy",velocityOldBuffery);

        DensityCompute.SetBuffer(DiffuseVelocityKernel,"velocityNewx",velocityNewBufferx);
        DensityCompute.SetBuffer(DiffuseVelocityKernel,"velocityNewy",velocityNewBuffery);
        DensityCompute.SetBuffer(DiffuseVelocityKernel,"velocityOldx",velocityOldBufferx);
        DensityCompute.SetBuffer(DiffuseVelocityKernel,"velocityOldy",velocityOldBuffery);

        DensityCompute.SetBuffer(AdvectVelocityKernel,"velocityNewx",velocityNewBufferx);
        DensityCompute.SetBuffer(AdvectVelocityKernel,"velocityNewy",velocityNewBuffery);
        DensityCompute.SetBuffer(AdvectVelocityKernel,"velocityOldx",velocityOldBufferx);
        DensityCompute.SetBuffer(AdvectVelocityKernel,"velocityOldy",velocityOldBuffery);

        //const int solvePKernel = 8;
        
        //const int gradientKernel = 10;
        //const int findDiverenceKernel = 9;
        DensityCompute.SetBuffer(findDiverenceKernel,"velocityNewx",velocityNewBufferx);
        DensityCompute.SetBuffer(findDiverenceKernel,"velocityNewy",velocityNewBuffery);
        DensityCompute.SetBuffer(findDiverenceKernel,"p",pBuffer);
        DensityCompute.SetBuffer(findDiverenceKernel,"delV",delVBuffer);

        //const int solvePKernel = 8;
        DensityCompute.SetBuffer(solvePKernel,"p",pBuffer);
        DensityCompute.SetBuffer(solvePKernel,"delV",delVBuffer);

        DensityCompute.SetBuffer(gradientKernel,"velocityNewx",velocityNewBufferx);
        DensityCompute.SetBuffer(gradientKernel,"velocityNewy",velocityNewBuffery);
        DensityCompute.SetBuffer(gradientKernel,"p",pBuffer);

        //swap
        DensityCompute.SetBuffer(SwapVelocityKernel,"velocityNewx",velocityNewBufferx);
        DensityCompute.SetBuffer(SwapVelocityKernel,"velocityNewy",velocityNewBuffery);
        DensityCompute.SetBuffer(SwapVelocityKernel,"velocityOldx",velocityOldBufferx);
        DensityCompute.SetBuffer(SwapVelocityKernel,"velocityOldy",velocityOldBuffery);
        /*
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
        */
        DensityCompute.SetBuffer(DrawKernel,"densityNew",densityNewBuffer);
        DensityCompute.SetTexture(DrawKernel,"Density",DensityTexture);
        DensityCompute.SetVector("densityAdd",new Vector2(100,100));
        DensityCompute.SetFloat("diffusionFactor",Densitydiffusion);
        DensityCompute.SetFloat("velocityDiffusion",Velocitydiffusion);
        DensityCompute.SetFloat("N",N);
        DensityCompute.SetFloat ("dt", dt);
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Mouse0)){
            Vector3 mouse_pos_pixel_coord = Input.mousePosition;
            Vector2 mouse_pos_normalized  = cam.ScreenToViewportPoint(mouse_pos_pixel_coord);
            mouse_pos_normalized  = new Vector2(Mathf.Clamp01(mouse_pos_normalized.x), Mathf.Clamp01(mouse_pos_normalized.y));
            Vector2 mouse_sim = new Vector2((int)(mouse_pos_normalized.x*N),(int)(mouse_pos_normalized.y*N));
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
        
        
        //velocityAlgorithm();
        densityAlgorithm();
        Draw();

        Graphics.Blit(DensityTexture,ouputTexture);
        densityNewBuffer.GetData(densityData);
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
            DensityCompute.Dispatch(DiffuseDensityKernel,N/8,N/8,1);
        }
        DensityCompute.Dispatch(AdvectDensityKernel,N/8,N/8,1);
        
    }
    
    void velocityAlgorithm(){

        for(int i = 0 ;i<20;i++){
            DensityCompute.Dispatch(DiffuseVelocityKernel,N/8,N/8,1);
        }
        DensityCompute.Dispatch(findDiverenceKernel,N/8,N/8,1);
        for(int i = 0; i<20;i++){
            DensityCompute.Dispatch(solvePKernel,N/8,N/8,1);
        }
        DensityCompute.Dispatch(gradientKernel,N/8,N/8,1);
        DensityCompute.Dispatch(SwapVelocityKernel,N/8,N/8,1);
        DensityCompute.Dispatch(AdvectVelocityKernel,N/8,N/8,1);
        DensityCompute.Dispatch(findDiverenceKernel,N/8,N/8,1);
        for(int i = 0; i<20;i++){
            DensityCompute.Dispatch(solvePKernel,N/8,N/8,1);
        }
        DensityCompute.Dispatch(gradientKernel,N/8,N/8,1);

        //diffuse both x and y
        //project 
        //findDivergence
        //solveP
        //gradient
        //advect both x and y
        //project
    }
    
    void Draw(){
        DensityCompute.Dispatch(DrawKernel,N/8,N/8,1);
    }
    
}
