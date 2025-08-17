using UnityEngine;

public class FlowerController : MonoBehaviour
{
    [Header("Flower Settings")]
    [SerializeField] private Renderer flowerRenderer;
  
    
    [Header("Dead Flower Color")]
    [SerializeField] private Color deadColor = new Color(0.4f, 0.3f, 0.2f, 1f); // Cor acastanhada
    
    void Start()
    {
        if (flowerRenderer == null)
            flowerRenderer = GetComponent<Renderer>();
            
        MakeFlowersDead();
    }
    
    private void MakeFlowersDead()
    {
       
       
            // Modificar cor do material atual
            flowerRenderer.material.color = deadColor;
            
            // Opcional: Reduzir o brilho/metallic para parecer mais seco
            if (flowerRenderer.material.HasProperty("_Metallic"))
                flowerRenderer.material.SetFloat("_Metallic", 0f);
                
            if (flowerRenderer.material.HasProperty("_Smoothness"))
                flowerRenderer.material.SetFloat("_Smoothness", 0.2f);

    }
}
