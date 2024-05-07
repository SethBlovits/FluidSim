
using UnityEngine;

public class AddVelocity : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture planeTexture;
    public RenderTexture velTexture;
    RenderTexture velocityTexture;
    RenderTexture diffusionTexture;
    RenderTexture pTexture;
    [SerializeField]
    Camera cam;
    [SerializeField]
    float diffusion;
    [SerializeField]
    float velocityDiffusion;
    private Vector2 prevPosition;
    // Start is called before the first frame update
    void Start(){
        if(velocityTexture == null){
            velocityTexture = new RenderTexture(1920,1080,32,RenderTextureFormat.ARGBFloat);
            velocityTexture.enableRandomWrite = true;
            
            velocityTexture.Create();
        }
        if(diffusionTexture == null){
            diffusionTexture = new RenderTexture(1920,1080,32,RenderTextureFormat.ARGBFloat);
            diffusionTexture.enableRandomWrite = true;
            diffusionTexture.Create();
        }
         if(pTexture == null){
            pTexture = new RenderTexture(1920,1080,32,RenderTextureFormat.ARGBFloat);
            pTexture.enableRandomWrite = true;
            pTexture.Create();
        }
        computeShader.SetTexture(0, "Velocity", velocityTexture);
        computeShader.SetTexture(0, "Diffusion", diffusionTexture);
        computeShader.SetTexture(0, "p", pTexture);
        computeShader.SetVector("pos",new Vector2(100,100));
        computeShader.SetFloat("diffusionFactor",diffusion);
        computeShader.SetFloat("velocityDiffusion",velocityDiffusion);
        computeShader.Dispatch(0, diffusionTexture.width/8,diffusionTexture.height/8,1); 
        Graphics.Blit(diffusionTexture,planeTexture);   
    }
    void Update(){
        if(Input.GetKeyDown(KeyCode.Mouse0)){//perform the first check for previous vector so I can then do velocity check
            Vector3 mouse_pos_pixel_coord = Input.mousePosition;
            Vector2 mouse_pos_normalized  = cam.ScreenToViewportPoint(mouse_pos_pixel_coord);
            mouse_pos_normalized  = new Vector2(Mathf.Clamp01(mouse_pos_normalized.x), Mathf.Clamp01(mouse_pos_normalized.y));
            Vector2 mouse_sim = new Vector2((int)(mouse_pos_normalized.x*1920),(int)(mouse_pos_normalized.y*1080));
            prevPosition  = mouse_sim;
        }
        if(Input.GetKey(KeyCode.Mouse0)){
            Vector3 mouse_pos_pixel_coord = Input.mousePosition;
            Vector2 mouse_pos_normalized  = cam.ScreenToViewportPoint(mouse_pos_pixel_coord);
            mouse_pos_normalized  = new Vector2(Mathf.Clamp01(mouse_pos_normalized.x), Mathf.Clamp01(mouse_pos_normalized.y));
            Vector2 mouse_sim = new Vector2((int)(mouse_pos_normalized.x*1920),(int)(mouse_pos_normalized.y*1080));
            Vector2 velocity = (mouse_sim-prevPosition);
            Debug.Log(velocity);
            computeShader.SetFloat("diffuseOn",1);
            computeShader.SetVector("pos",mouse_sim);
            computeShader.SetVector("vel",velocity);
            prevPosition = mouse_sim;
        }
        if(!Input.anyKey){
            computeShader.SetVector("vel",Vector2.zero);
            computeShader.SetFloat("diffuseOn",0);
        }
        computeShader.SetFloat("diffusionFactor",diffusion);
        computeShader.Dispatch(0, diffusionTexture.width/8,diffusionTexture.height/8,1); 
        Graphics.Blit(diffusionTexture,planeTexture);
        Graphics.Blit(velocityTexture,velTexture);
        
        
    }

    
}
