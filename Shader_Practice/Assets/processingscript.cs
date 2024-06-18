using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class processingscript : MonoBehaviour
{
    // Start is called before the first frame update
    public int N;
    public float viscosity;
    public float diffusion;
    public float dt;
    public float velocityX;
    public float velocityY;
    public float DyeDensity;
    public Color color;
    Vector2 prevPosition;
    
    float[] Vx;
    float[] Vy;
    float[] Vx0;
    float[] Vy0;
    float[] s;
    float[] density;
    public float[] delV;
    float[] p;

    [SerializeField]
    Camera cam;
    ComputeBuffer densityNew;
    ComputeBuffer densityOld;
    ComputeBuffer velocityNewX;
    ComputeBuffer velocityNewY;
    ComputeBuffer velocityOldX;
    ComputeBuffer velocityOldY;
    ComputeBuffer pBuffer;
    ComputeBuffer delVBuffer;
    public RenderTexture ouputTexture;
    RenderTexture DensityTexture;
    [SerializeField]
    ComputeShader Compute;
    
    void step()
    {
        Compute.Dispatch(10,N/8,N/8,1);
        Compute.Dispatch(6,N/8,N/8,1);
        for(int i = 0;i<20;i++){
            Compute.Dispatch(3,N/8,N/8,1);//diffuse velocity
        }
        Compute.Dispatch(7,N/8,N/8,1);
        for(int i = 0;i<20;i++){
            Compute.Dispatch(8,N/8,N/8,1);
        }
        Compute.Dispatch(9,N/8,N/8,1);
        Compute.Dispatch(6,N/8,N/8,1);
        Compute.Dispatch(4,N/8,N/8,1);//advect velocity
        Compute.Dispatch(7,N/8,N/8,1);
        for(int i = 0;i<20;i++){
            Compute.Dispatch(8,N/8,N/8,1);
        }
        Compute.Dispatch(9,N/8,N/8,1);
        Compute.Dispatch(5,N/8,N/8,1);//swaps density
        for(int i = 0;i<20;i++){
            Compute.Dispatch(1,N/8,N/8,1);//diffuse density
        }
        
        Compute.Dispatch(5,N/8,N/8,1);//swaps density
        Compute.Dispatch(2,N/8,N/8,1);//advect density
    }
    void Start()
    {   
        prevPosition = Vector2.zero;
        if(DensityTexture == null){
            DensityTexture = new RenderTexture(N,N,32);
            DensityTexture.enableRandomWrite = true;
            DensityTexture.wrapMode = TextureWrapMode.Clamp;
            DensityTexture.Create();
        }
        density = new float[N*N];
        Vx = new float[N*N];
        Vy = new float[N*N];
        Vx0 = new float[N*N];
        Vy0 = new float[N*N];
        s = new float[N*N];
        p = new float[N*N];
        delV = new float[N*N];
        for(int i = 0; i < N*N; i++){
            
            density[i]=0;
            Vx[i]=0;
            Vy[i]=0;
            Vx0[i]=0;
            Vy0[i]=0;
            s[i]=0;
            delV[i]=0;
            p[i] = 0;

        }
        densityNew = new ComputeBuffer(N*N,sizeof(float));
        densityNew.SetData(density);
        densityOld = new ComputeBuffer(N*N, sizeof(float));
        densityOld.SetData(s);
        velocityNewX = new ComputeBuffer(N*N,sizeof(float));
        velocityNewX.SetData(Vx);
        velocityNewY = new ComputeBuffer(N*N,sizeof(float));
        velocityNewY.SetData(Vy);
        velocityOldX = new ComputeBuffer(N*N,sizeof(float));
        velocityOldX.SetData(Vx0);
        velocityOldY = new ComputeBuffer(N*N,sizeof(float));
        velocityOldY.SetData(Vy0);
        pBuffer = new ComputeBuffer(N*N, sizeof(float));
        pBuffer.SetData(p);
        delVBuffer = new ComputeBuffer(N*N, sizeof(float));
        delVBuffer.SetData(delV);


        Compute.SetBuffer(0,"densityNew",densityNew);
        Compute.SetTexture(0,"Result",DensityTexture);
        Compute.SetFloat("width",N);
        Compute.SetFloat("height",N);
        Compute.SetFloat("N",(float)N);
        Compute.SetFloat("dt",dt*Time.deltaTime);
        Compute.SetFloat("vX",2f);
        Compute.SetFloat("dens",100);
        Compute.SetFloat("vY",0);
        Compute.SetFloat("densityDiffusion",diffusion);
        Compute.SetFloat("velocityDiffusion",viscosity);

        Compute.SetBuffer(1,"densityNew",densityNew);
        Compute.SetBuffer(1,"densityOld",densityOld);

        Compute.SetBuffer(2,"densityNew",densityNew);
        Compute.SetBuffer(2,"densityOld",densityOld);
        Compute.SetBuffer(2,"velocityNewX",velocityNewX);
        Compute.SetBuffer(2,"velocityNewY",velocityNewY);

        Compute.SetBuffer(3,"velocityNewX",velocityNewX);
        Compute.SetBuffer(3,"velocityNewY",velocityNewY);
        Compute.SetBuffer(3,"velocityOldX",velocityOldX);
        Compute.SetBuffer(3,"velocityOldY",velocityOldY);

        Compute.SetBuffer(4,"velocityNewX",velocityNewX);
        Compute.SetBuffer(4,"velocityNewY",velocityNewY);
        Compute.SetBuffer(4,"velocityOldX",velocityOldX);
        Compute.SetBuffer(4,"velocityOldY",velocityOldY);

        Compute.SetBuffer(5,"densityNew",densityNew);
        Compute.SetBuffer(5,"densityOld",densityOld);

        Compute.SetBuffer(6,"velocityNewX",velocityNewX);
        Compute.SetBuffer(6,"velocityNewY",velocityNewY);
        Compute.SetBuffer(6,"velocityOldX",velocityOldX);
        Compute.SetBuffer(6,"velocityOldY",velocityOldY);

        Compute.SetBuffer(7,"velocityNewX",velocityNewX);
        Compute.SetBuffer(7,"velocityNewY",velocityNewY);
        Compute.SetBuffer(7,"velocityOldX",velocityOldX);
        Compute.SetBuffer(7,"velocityOldY",velocityOldY);

        Compute.SetBuffer(8,"velocityNewX",velocityNewX);
        Compute.SetBuffer(8,"velocityNewY",velocityNewY);
        Compute.SetBuffer(8,"velocityOldX",velocityOldX);
        Compute.SetBuffer(8,"velocityOldY",velocityOldY);

        Compute.SetBuffer(9,"velocityNewX",velocityNewX);
        Compute.SetBuffer(9,"velocityNewY",velocityNewY);
        Compute.SetBuffer(9,"velocityOldX",velocityOldX);
        Compute.SetBuffer(9,"velocityOldY",velocityOldY);
        
        Compute.SetBuffer(10,"densityNew",densityNew);
        Compute.SetBuffer(10,"densityOld",densityOld);
        Compute.SetBuffer(10,"velocityNewX",velocityNewX);
        Compute.SetBuffer(10,"velocityNewY",velocityNewY);
        Compute.SetBuffer(10,"velocityOldX",velocityOldX);
        Compute.SetBuffer(10,"velocityOldY",velocityOldY);

    }

    void Update()
    {   
        Compute.SetVector("Color",Color.HSVToRGB(Mathf.Abs(Mathf.Sin(Time.time*6.28f*.1f)),1,1));
        if(Input.GetKey(KeyCode.Mouse0)){
            Vector3 mouse_pos_pixel_coord = Input.mousePosition;
            Vector2 mouse_pos_normalized  = cam.ScreenToViewportPoint(mouse_pos_pixel_coord);
            mouse_pos_normalized  = new Vector2(Mathf.Clamp01(mouse_pos_normalized.x), Mathf.Clamp01(mouse_pos_normalized.y));
            Vector2 mouse_sim = new Vector2((int)(mouse_pos_normalized.x*N),(int)(mouse_pos_normalized.y*N));
            Vector2 mouse_velocity = (mouse_sim/50.0f-prevPosition);
            Compute.SetInt("xCoord",(int)mouse_sim.x);
            Compute.SetInt("yCoord",(int)mouse_sim.y);
            Compute.SetFloat("dens",DyeDensity);
            Compute.SetFloat("vX",mouse_velocity.x);
            Compute.SetFloat("vY",mouse_velocity.y);
            //Debug.Log(mouse_sim.x);
            prevPosition = mouse_sim/50.0f;
        }
        if(!Input.anyKey){
            Vector3 mouse_pos_pixel_coord = Input.mousePosition;
            Vector2 mouse_pos_normalized  = cam.ScreenToViewportPoint(mouse_pos_pixel_coord);
            mouse_pos_normalized  = new Vector2(Mathf.Clamp01(mouse_pos_normalized.x), Mathf.Clamp01(mouse_pos_normalized.y));
            Vector2 mouse_sim = new Vector2((int)(mouse_pos_normalized.x*N),(int)(mouse_pos_normalized.y*N));
            Compute.SetFloat("dens",0);
            Compute.SetFloat("vX",0);
            Compute.SetFloat("vY",0);
            prevPosition = mouse_sim/50.0f;
        }
        Compute.SetFloat("dt",dt*Time.deltaTime);
        Compute.SetFloat("densityDiffusion",diffusion);
        Compute.SetFloat("velocityDiffusion",viscosity);
        step();
        
        Compute.Dispatch(0,N/8,N/8,1);
        Graphics.Blit(DensityTexture,ouputTexture);
    }
   
    private void OnDestroy() {
        densityNew.Release();
        densityOld.Release();
        velocityNewX.Release();
        velocityNewY.Release();
        velocityOldX.Release();
        velocityOldY.Release();
        pBuffer.Release();
        delVBuffer.Release();
    }
}
