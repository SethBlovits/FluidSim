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
    



    int IX(int x, int y) {
        return x + y * N;
    }
    void diffuse(int b,float[]x,float[]x0,float diff,float dt){
        float a = dt*diff*(N-2)*(N-2);
        lin_solve(b, x, x0, a, 1 + 4 * a);
    }
    void lin_solve(int b, float[]x, float[]x0, float a, float c) {
        float cRecip = 1.0f / c;
        for (int t = 0; t < 20; t++) {
            for (int j = 1; j < N - 1; j++) {
                for (int  i = 1; i < N - 1; i++) {
                    x[IX(i, j)] =
                    (x0[IX(i, j)] +
                        a *
                        (x[IX(i + 1, j)] +
                            x[IX(i - 1, j)] +
                            x[IX(i, j + 1)] +
                            x[IX(i, j - 1)])) *
                    cRecip;
                }
            }
        set_bnd(b, x);
        }
    }
    void project(float[] velocX, float[] velocY, float[] p, float[] div) {
        
        for (int j = 1; j < N - 1; j++) {
            for (int i = 1; i < N - 1; i++) {
                div[IX(i, j)] =
                    (-0.5f *
                    (velocX[IX(i + 1, j)] -
                        velocX[IX(i - 1, j)] +
                        velocY[IX(i, j + 1)] -
                        velocY[IX(i, j - 1)])) /
                    N;
                p[IX(i, j)] = 0;
                delV[IX(i, j)] = div[IX(i, j)]; 
            }
        }
        
        set_bnd(0, div);
        set_bnd(0, p);

        lin_solve(0, p, div, 1, 4);

        for (int j = 1; j < N - 1; j++) {
            for (int i = 1; i < N - 1; i++) {
                velocX[IX(i, j)] -= 0.5f * (p[IX(i + 1, j)] - p[IX(i - 1, j)]) * N;
                velocY[IX(i, j)] -= 0.5f * (p[IX(i, j + 1)] - p[IX(i, j - 1)]) * N;
            }
        }

        set_bnd(1, velocX);
        set_bnd(2, velocY);
    }
    void mod_project(float[] velocX, float[] velocY, float[] p, float[] div) {
        
        /*for (int j = 1; j < N - 1; j++) {
            for (int i = 1; i < N - 1; i++) {
                div[IX(i, j)] =
                    (-0.5f *
                    (velocX[IX(i + 1, j)] -
                        velocX[IX(i - 1, j)] +
                        velocY[IX(i, j + 1)] -
                        velocY[IX(i, j - 1)])) /
                    N;
                p[IX(i, j)] = 0;
            }
        }*/
       
        //set_bnd(0, div);
        //set_bnd(0, p);

        lin_solve(0, p, div, 1, 4);
        
        for (int j = 1; j < N - 1; j++) {
            for (int i = 1; i < N - 1; i++) {
                velocX[IX(i, j)] -= 0.5f * (p[IX(i + 1, j)] - p[IX(i - 1, j)]) * N;
                velocY[IX(i, j)] -= 0.5f * (p[IX(i, j + 1)] - p[IX(i, j - 1)]) * N;
            }
        }

        set_bnd(1, velocX);
        set_bnd(2, velocY);
    }
    void advect(int b, float[] d, float[] d0, float[] velocX, float[] velocY, float dt) {
        float i0, i1, j0, j1;

        float dtx = dt * (N - 2);
        float dty = dt * (N - 2);

        float s0, s1, t0, t1;
        float tmp1, tmp2, x, y;

        float Nfloat = N - 2;
        float ifloat, jfloat;
        int i, j;

        for (j = 1, jfloat = 1; j < N - 1; j++, jfloat++) {
            for (i = 1, ifloat = 1; i < N - 1; i++, ifloat++) {
                tmp1 = dtx * velocX[IX(i, j)];
                tmp2 = dty * velocY[IX(i, j)];
                x = ifloat - tmp1;
                y = jfloat - tmp2;

                if (x < 0.5) x = 0.5f;
                if (x > Nfloat + 0.5) x = Nfloat + 0.5f;
                i0 = Mathf.Floor(x);
                i1 = i0 + 1.0f;
                if (y < 0.5) y = 0.5f;
                if (y > Nfloat + 0.5f) y = Nfloat + 0.5f;
                j0 = Mathf.Floor(y);
                j1 = j0 + 1.0f;

                s1 = x - i0;
                //Debug.Log(s1);
                s0 = 1.0f - s1;
                t1 = y - j0;
                t0 = 1.0f - t1;

                int i0i = (int)i0;
                int i1i = (int)i1;
                int j0i = (int)j0;
                int j1i = (int)j1;

                d[IX(i, j)] =
                    s0 * (t0 * d0[IX(i0i, j0i)] + t1 * d0[IX(i0i, j1i)]) +
                    s1 * (t0 * d0[IX(i1i, j0i)] + t1 * d0[IX(i1i, j1i)]);
            }
        }

        set_bnd(b, d);
    }
    void set_bnd(int b, float[] x) {
        for (int i = 1; i < N - 1; i++) {
            x[IX(i, 0)] = b == 2 ? -x[IX(i, 1)] : x[IX(i, 1)];
            x[IX(i, N - 1)] = b == 2 ? -x[IX(i, N - 2)] : x[IX(i, N - 2)];
        }
        for (int j = 1; j < N - 1; j++) {
            x[IX(0, j)] = b == 1 ? -x[IX(1, j)] : x[IX(1, j)];
            x[IX(N - 1, j)] = b == 1 ? -x[IX(N - 2, j)] : x[IX(N - 2, j)];
        }

        x[IX(0, 0)] = 0.5f * (x[IX(1, 0)] + x[IX(0, 1)]);
        x[IX(0, N - 1)] = 0.5f * (x[IX(1, N - 1)] + x[IX(0, N - 2)]);
        x[IX(N - 1, 0)] = 0.5f * (x[IX(N - 2, 0)] + x[IX(N - 1, 1)]);
        x[IX(N - 1, N - 1)] = 0.5f * (x[IX(N - 2, N - 1)] + x[IX(N - 1, N - 2)]);
    }
    void step(){
        
        //velocityNewX.SetData(Vx);
        //velocityNewY.SetData(Vy);
        //velocityOldX.SetData(Vx0);
        //velocityOldY.SetData(Vy0);
        Compute.Dispatch(10,N/8,N/8,1);
        Compute.Dispatch(6,N/8,N/8,1);
        for(int i = 0;i<20;i++){
            Compute.Dispatch(3,N/8,N/8,1);//diffuse velocity
        }
        
        //Compute.Dispatch(6,N/8,N/8,1);
        Compute.Dispatch(7,N/8,N/8,1);
        for(int i = 0;i<20;i++){
            Compute.Dispatch(8,N/8,N/8,1);
        }
        Compute.Dispatch(9,N/8,N/8,1);
        Compute.Dispatch(6,N/8,N/8,1);
        /*
        velocityNewX.GetData(Vx);
        velocityNewY.GetData(Vy);
        velocityOldX.GetData(Vx0);
        velocityOldY.GetData(Vy0);
        project(Vx0, Vy0, Vx, Vy);
        velocityNewX.SetData(Vx);
        velocityNewY.SetData(Vy);
        velocityOldX.SetData(Vx0);
        velocityOldY.SetData(Vy0);*/
        //Compute.Dispatch(6,N/8,N/8,1);
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
        //Compute.Dispatch(5,N/8,N/8,1);//swaps density
        
        
        
    }
    void Start()
    {   
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

    // Update is called once per frame
    void Update()
    {
        Compute.SetFloat("dt",dt*Time.deltaTime);
        Compute.SetFloat("vX",velocityX);
        Compute.SetFloat("dens",DyeDensity);
        Compute.SetFloat("vY",velocityY);
        Compute.SetFloat("densityDiffusion",diffusion);
        Compute.SetFloat("velocityDiffusion",viscosity);
        //s[IX(N/2,N/2)] = 10;
        //Vx[IX(N/2,N/2)] = -1;
        //velocityNewX.SetData(Vx);
        //densityNew.SetData(density);
        //densityOld.SetData(s);
        step();
        //density[IX(N/2,N/2)] += 1500;
        
        //velocityNewX.SetData(Vx);
        
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
