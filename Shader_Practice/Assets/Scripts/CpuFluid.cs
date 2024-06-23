using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using TMPro;
public class CpuFluid : MonoBehaviour
{

    public RenderTexture ouputTexture;
    RenderTexture DensityTexture;
    [SerializeField]
    ComputeShader Compute;
    [SerializeField]
    public int resolutionX;
    public int resolutionY;
    public int resolution;
    [SerializeField]
    float diffusion;
    [SerializeField]
    Camera cam;
    ComputeBuffer buffer;
    public float[] densityFinal;

    float[] densityOld;
    Vector3[] velocityOld;
    Vector3[] velocityNew;
    float[] pNew;
    float[] pOld;
    float[] delV;
    bool mouse_enabled = false;
    Vector2 mouse_sim;
    // Start is called before the first frame update
    private void OnDestroy() {
        buffer.Release();
    }
    void Start()
    {
        resolution = resolutionX*resolutionY;
        densityFinal = new float[resolution];
        if(DensityTexture == null){
            DensityTexture = new RenderTexture(resolutionX,resolutionY,32);
            DensityTexture.enableRandomWrite = true;
            
            DensityTexture.Create();
        }
        
        buffer = new ComputeBuffer(resolution,sizeof(float));
        buffer.SetData(densityFinal);
        Compute.SetBuffer(0,"density",buffer);
        Compute.SetTexture(0,"Result",DensityTexture);
        Compute.SetFloat("width",resolutionX);
        Compute.SetFloat("height",resolutionY);
        densityOld = new float[resolution];
        velocityOld = new Vector3[resolution];
        velocityNew = new Vector3[resolution];
        pNew = new float[resolution];
        pOld = new float[resolution];
        delV = new float[resolution];
        for(int i = 0; i<resolution;i++){
            densityFinal[i] = 0;
            densityOld[i] = 0;
            velocityOld[i] = Vector3.zero;
            velocityNew[i] = Vector3.zero;
            pNew[i] = 0;
            pOld[i] = 0;
            delV[i] = 0;

        }
        print(indexFromIdCoords(0,0,1));

      
        
        
        
    }
    int indexFromIdCoords(int x,int y, int z){
        return y*(resolutionX)+x;
    }
    void setBoundaryDensity(){
        densityFinal[indexFromIdCoords(0,0,1)] = 0.5f *(densityFinal[indexFromIdCoords(1,0,1)]+densityFinal[indexFromIdCoords(0,1,1)]);
        densityFinal[indexFromIdCoords(0,resolutionY-1,1)] = 0.5f *(densityFinal[indexFromIdCoords(1,resolutionY-1,1)]+densityFinal[indexFromIdCoords(0,resolutionY-2,1)]);
        densityFinal[indexFromIdCoords(resolutionX-1,0,1)] = 0.5f *(densityFinal[indexFromIdCoords(resolutionX-2,0,1)]+densityFinal[indexFromIdCoords(resolutionX-1,1,1)]);
        densityFinal[indexFromIdCoords(resolutionX-1,resolutionY-1,1)] = 0.5f *(densityFinal[indexFromIdCoords(resolutionX-2,resolutionY-1,1)]+densityFinal[indexFromIdCoords(resolutionX-1,resolutionY-2,1)]);
    }
    void setBoundaryVelocity(){
        for(int i = 0 ; i<resolutionX-1;i++){
            velocityNew[indexFromIdCoords(0,i,1)] = -velocityNew[indexFromIdCoords(1,i,1)];
            velocityNew[indexFromIdCoords(resolutionX-1,i,1)] = -velocityNew[indexFromIdCoords(resolutionX-2,i,1)];
            velocityNew[indexFromIdCoords(i,0,1)] = -velocityNew[indexFromIdCoords(i,1,1)];
            velocityNew[indexFromIdCoords(i,resolutionY-1,1)] = -velocityNew[indexFromIdCoords(i,resolutionY-2,1)];
        }
        velocityNew[indexFromIdCoords(0,0,1)] = 0.5f *(velocityNew[indexFromIdCoords(1,0,1)]+velocityNew[indexFromIdCoords(0,1,1)]);
        velocityNew[indexFromIdCoords(0,resolutionY-1,1)] = 0.5f *(velocityNew[indexFromIdCoords(1,resolutionY-1,1)]+velocityNew[indexFromIdCoords(0,resolutionY-2,1)]);
        velocityNew[indexFromIdCoords(resolutionX-1,0,1)] = 0.5f *(velocityNew[indexFromIdCoords(resolutionX-2,0,1)]+velocityNew[indexFromIdCoords(resolutionX-1,1,1)]);
        velocityNew[indexFromIdCoords(resolutionX-1,resolutionY-1,1)] = 0.5f *(velocityNew[indexFromIdCoords(resolutionX-2,resolutionY-1,1)]+velocityNew[indexFromIdCoords(resolutionX-1,resolutionY-2,1)]);
    }
    void setBoundaryDelV(){
        
        delV[indexFromIdCoords(0,0,1)] = 0.5f *(delV[indexFromIdCoords(1,0,1)]+delV[indexFromIdCoords(0,1,1)]);
        delV[indexFromIdCoords(0,resolutionY-1,1)] = 0.5f *(delV[indexFromIdCoords(1,resolutionY-1,1)]+delV[indexFromIdCoords(0,resolutionY-2,1)]);
        delV[indexFromIdCoords(resolutionX-1,0,1)] = 0.5f *(delV[indexFromIdCoords(resolutionX-1,0,1)]+delV[indexFromIdCoords(resolutionX-1,1,1)]);
        delV[indexFromIdCoords(resolutionX-1,resolutionY-1,1)] = 0.5f *(delV[indexFromIdCoords(resolutionX-1,resolutionY-1,1)]+delV[indexFromIdCoords(resolutionX-1,resolutionY-2,1)]);
    }
    void setBoundaryP(){
        
        pNew[indexFromIdCoords(0,0,1)] = 0.5f *(pNew[indexFromIdCoords(1,0,1)]+pNew[indexFromIdCoords(0,1,1)]);
        pNew[indexFromIdCoords(0,resolutionY-1,1)] = 0.5f *(pNew[indexFromIdCoords(1,resolutionY-1,1)]+pNew[indexFromIdCoords(0,resolutionY-2,1)]);
        pNew[indexFromIdCoords(resolutionX-1,0,1)] = 0.5f *(pNew[indexFromIdCoords(resolutionX-1,0,1)]+pNew[indexFromIdCoords(resolutionX-1,1,1)]);
        pNew[indexFromIdCoords(resolutionX-1,resolutionY-1,1)] = 0.5f *(pNew[indexFromIdCoords(resolutionX-1,resolutionY-1,1)]+pNew[indexFromIdCoords(resolutionX-1,resolutionY-2,1)]);
    }
    void swapDensity(){
        densityOld = densityFinal;
    }
    void swapVelocity(){
        velocityOld = velocityNew;
    }
    void diffuseDensity(int x, int y){
        float scale = diffusion; //* Time.deltaTime *resolution;
        //for(int i = 0;i<20;i++){
            densityFinal[indexFromIdCoords(x,y,1)] = (densityOld[indexFromIdCoords(x,y,1)] + scale*(densityFinal[indexFromIdCoords(x+1,y,1)]+
            densityFinal[indexFromIdCoords(x-1,y,1)] + densityFinal[indexFromIdCoords(x,y-1,1)] + densityFinal[indexFromIdCoords(x,y+1,1)]))/(1f+4f*scale);
       // }
        
    }
    void diffuseVelocity(int x, int y){
        float scale = diffusion; //* Time.deltaTime *resolution;
        //for(int i = 0;i<20;i++){
            velocityNew[indexFromIdCoords(x,y,1)] = (velocityOld[indexFromIdCoords(x,y,1)] + scale*(velocityNew[indexFromIdCoords(x+1,y,1)]+
            velocityNew[indexFromIdCoords(x-1,y,1)] + velocityNew[indexFromIdCoords(x,y-1,1)] + velocityNew[indexFromIdCoords(x,y+1,1)]))/(1f+4f*scale);
       // }
    }
    void advectionVelocity(int x, int y){
        float dt = Time.deltaTime * resolution;
       
        Vector3 f = new Vector3(x,y,0) - velocityOld[indexFromIdCoords(x,y,1)];//*dt;

        if(f.x<0){
            f.x = 1;
        }
        else if(f.x>resolutionX-2){
            f.x = resolutionX-2;
        }
        if(f.y<0){
            f.y = 1;
        }
        else if(f.y>resolutionY-2){
            f.y = resolutionY-2;
        }

        int ix = (int)Mathf.Floor(f.x); float jx = math.frac(f.x);
        int iy = (int)Mathf.Floor(f.y); float jy = math.frac(f.y);

        Vector3 z1 = Vector3.Lerp(velocityOld[indexFromIdCoords(ix,iy,1)],velocityOld[indexFromIdCoords(ix+1,iy,1)],jx);
        Vector3 z2 = Vector3.Lerp(velocityOld[indexFromIdCoords(ix,iy+1,1)],velocityOld[indexFromIdCoords(ix+1,iy+1,1)],jy);
        
        velocityNew[indexFromIdCoords(x,y,1)] = Vector3.Lerp(z1,z2,jy);
    }
    void advectionDensity(int x,int y){
        float dt = Time.deltaTime * resolutionX;
       
        Vector3 f = new Vector3(x,y,0) - velocityOld[indexFromIdCoords(x,y,1)];//*dt;
        /*
        Debug.Log(y);
        Debug.Log(velocityOld[indexFromIdCoords(x,y,1)]);
        Debug.Log(new Vector3(x,y,0));*/
        if(f.x<0){
            f.x = 0;
        }
        if(f.x>resolutionX-2){
            f.x = resolutionX-2;
        }
        if(f.y<0){
            f.y = 0;
        }
        if(f.y>resolutionY-2){
            f.y = resolutionY-2;
        }

        int ix = Mathf.FloorToInt(f.x); float jx = math.frac(f.x);
        int iy = Mathf.FloorToInt(f.y); float jy = math.frac(f.y);
        /*Debug.Log(ix +","+f.x);
        Debug.Log(iy +","+f.y);
        Debug.Log(jx);
        Debug.Log(jy);*/
        //Debug.Log(Mathf.FloorToInt(0));
        float z1 = Mathf.Lerp(densityOld[indexFromIdCoords(ix,iy,1)],densityOld[indexFromIdCoords(ix+1,iy,1)],jx);
        float z2 = Mathf.Lerp(densityOld[indexFromIdCoords(ix,iy+1,1)],densityOld[indexFromIdCoords(ix+1,iy+1,1)],jy);
        
        densityFinal[indexFromIdCoords(x,y,1)] = Mathf.Lerp(z1,z2,jy);

    }
    void densityAlgorithm(){
        for(int i = 0;i<20;i++){
            for(int x = 1;x<resolutionX-1;x++){
                for(int y = 1;y<resolutionY-1;y++){
                    //print(x + "," + y );
                    diffuseDensity(x,y);
                }
            }
            setBoundaryDensity();
        }
        swapDensity();
        
        for(int x = 1;x<resolutionX-1;x++){
            for(int y = 1;y<resolutionY-1;y++){
                advectionDensity(x,y);
            }
        }
        setBoundaryDensity();
        swapDensity();
    }
    void velocityAlgorithm(){
        for(int i = 0;i<20;i++){
            for(int x = 1;x<resolutionX-1;x++){
                for(int y = 1;y<resolutionY-1;y++){
                    //print(x + "," + y );
                    diffuseVelocity(x,y);
                }
            }
            setBoundaryVelocity();
        }
        projection();
        swapVelocity();
        for(int x = 1;x<resolutionX-1;x++){
            for(int y = 1;y<resolutionY-1;y++){
                advectionVelocity(x,y);
            }
        }
        setBoundaryVelocity();
        projection();
        swapVelocity();
    }
    void projection(){
        float h = 1/resolutionX;
        for(int x = 1 ; x<resolutionX-1;x++){
            for(int y = 1; y<resolutionY-1;y++){
                delV[indexFromIdCoords(x,y,1)] = -0.5f  *(velocityOld[indexFromIdCoords(x+1,y,1)].x-velocityOld[indexFromIdCoords(x-1,y,1)].x +
                velocityOld[indexFromIdCoords(x,y+1,1)].y-velocityOld[indexFromIdCoords(x,y-1,1)].y);
                pNew[indexFromIdCoords(x,y,1)] = 0;  
            }
        }
        setBoundaryDelV(); setBoundaryP();
        for(int k = 0;k < 20;k++){
            for(int x = 1 ; x<resolutionX-1;x++){
                for(int y = 1; y<resolutionY-1;y++){
                    pNew[indexFromIdCoords(x,y,1)] = (delV[indexFromIdCoords(x,y,1)] +
                    pNew[indexFromIdCoords(x+1,y,1)]+pNew[indexFromIdCoords(x-1,y,1)]+
                    pNew[indexFromIdCoords(x,y-1,1)] + pNew[indexFromIdCoords(x,y+1,1)])/4f;  
                }
            }
            setBoundaryP();
        }

        for(int x = 1 ; x<resolutionX-1;x++){
            for(int y = 1; y<resolutionY-1;y++){
                velocityNew[indexFromIdCoords(x,y,1)] -= 0.5f * new Vector3(pNew[indexFromIdCoords(x+1,y,1)]-pNew[indexFromIdCoords(x-1,y,1)],
                pNew[indexFromIdCoords(x,y+1,1)]-pNew[indexFromIdCoords(x,y-1,1)],0);
            }
        }
        setBoundaryVelocity();
 

    }
    void Update()
    {
        if(Input.GetKey(KeyCode.Mouse0)){
             mouse_enabled = true;
            Vector3 mouse_pos_pixel_coord = Input.mousePosition;
            Vector2 mouse_pos_normalized  = cam.ScreenToViewportPoint(mouse_pos_pixel_coord);
            mouse_pos_normalized  = new Vector2(Mathf.Clamp01(mouse_pos_normalized.x), Mathf.Clamp01(mouse_pos_normalized.y));
            mouse_sim = new Vector2((int)(mouse_pos_normalized.x*resolutionX),(int)(mouse_pos_normalized.y*resolutionY));
            //print(mouse_sim);
            //Vector2 velocity = (mouse_sim-prevPosition);
            //Debug.Log(velocity);
            //computeShader.SetFloat("diffuseOn",1);
            //computeShader.SetVector("pos",mouse_sim);
            //computeShader.SetVector("vel",velocity);
            //prevPosition = mouse_sim;
            //Compute.SetVector("densityAdd",mouse_sim);
            //Compute.SetBool("enabled",true);
        }
        if(!Input.anyKey){
            //DensityCompute.SetBool("enabled",false);
            mouse_enabled = false;
        }

        if(mouse_enabled){
            densityOld[indexFromIdCoords((int)mouse_sim.x,(int)mouse_sim.y,1)] = 10;
            velocityOld[indexFromIdCoords((int)mouse_sim.x,(int)mouse_sim.y,1)] = new Vector3(150f,0,0);
        }
        //densityOld[indexFromIdCoords(resolutionX/2,resolutionY/2,1)] = 1;
        
        densityAlgorithm();
        velocityAlgorithm();
        //print(densityOld[indexFromIdCoords(resolutionX/2-2,resolutionY/2,1)]);
        buffer.SetData(densityFinal);
        
        Compute.Dispatch(0,resolutionX/8,resolutionY/8,1);
        Graphics.Blit(DensityTexture,ouputTexture);
        
      
    }
    
}
