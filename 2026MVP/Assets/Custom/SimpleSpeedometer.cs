using UnityEngine;                                                                
using TMPro;                                                                      
                                                                                
public class SimpleSpeedometer : MonoBehaviour                                    
{                                                                                 
    public TextMeshProUGUI speedText;                                             
    private Rigidbody playerRb;                                                   
                                                                                
    void Start()                                                                  
    {                                                                             
        GameObject player = GameObject.Find("Player");                            
        if (player != null)                                                       
        {                                                                         
            playerRb = player.GetComponent<Rigidbody>();                          
            Debug.Log("Found player and rigidbody!");                             
        }                                                                         
        else                                                                      
        {                                                                         
            Debug.Log("Player NOT found!");                                       
        }                                                                         
    }                                                                             
                                                                                
    void Update()                                                                 
    {                                                                             
        if (playerRb != null && speedText != null)                                
        {                                                                         
            float speed = playerRb.linearVelocity.magnitude * 3.6f;                     
            speedText.text = $"{speed:F0} km/h";                                  
        }                                                                         
    }                                                                             
}   