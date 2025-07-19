using Grid;
using TMPro;
using Util;

namespace Entities
{
    using UnityEngine;

    public abstract class AbstractEntity : MonoBehaviour
    {
        public float initialHealth;
        public float health;
        public Vector2Int positionRowCol;
        public TextMeshPro healthText;
        public GameObject healthBar;

        public void MoveEntity(Vector2Int newCoords)
        {
            positionRowCol = newCoords;
            GetComponent<LerpPosition>().targetLocation = HexGridManager.GetHexCenter(positionRowCol.x, positionRowCol.y);
            
        }

        public void Update()
        {
          RectTransform rectTransform = healthBar.GetComponent<RectTransform>();

          healthText.text = health + "/" + initialHealth;
          float healthRatio = health / initialHealth;

          
          // Magic number. Change later
          float fullWidth = 19.5f;

          rectTransform.sizeDelta = new Vector2(healthRatio * fullWidth, rectTransform.sizeDelta.y);

        }

        public virtual void Damage(int damage)
        {
            health -= damage;
            health = Mathf.Clamp(health, 0, initialHealth);
            ScreenShake.Instance.Shake(0.2f, 7);
        }

        public abstract void Die();
        
    }
    
    /*
     ENEMY DESIGN IDEAS
     
     <!DOCTYPE html>
       <html lang="en">
       <head>
         <meta charset="UTF-8">
         <title>Creative Geometric Enemies</title>
         <style>
           body {
             display: flex;
             flex-wrap: wrap;
             gap: 20px;
             padding: 20px;
             background: #111;
           }
           .enemy {
             background: #222;
             padding: 10px;
             border-radius: 8px;
           }
           svg {
             width: 120px;
             height: 120px;
             display: block;
           }
         </style>
       </head>
       <body>
       
         <!-- 🔺 Red Spikehead (Triangle head + trapezoid body) -->
         <div class="enemy">
           <svg viewBox="0 0 100 100">
             <polygon points="50,10 30,40 70,40" fill="red" />
             <polygon points="30,40 70,40 80,85 20,85" fill="red" />
           </svg>
         </div>
       
         <!-- 🔷 Blue Cyclops v2 (Hex body + Rhombus eye ridge) -->
         <div class="enemy">
           <svg viewBox="0 0 100 100">
             <polygon points="50,10 85,30 85,70 50,90 15,70 15,30" fill="blue" />
             <polygon points="50,30 60,50 50,70 40,50" fill="blue" />
           </svg>
         </div>
       
         <!-- 🟢 Green Blade Bug (Hexagon body + triangle wings) -->
         <div class="enemy">
           <svg viewBox="0 0 100 100">
             <polygon points="50,20 80,35 80,65 50,80 20,65 20,35" fill="green" />
             <polygon points="10,30 30,50 10,70" fill="green" />
             <polygon points="90,30 70,50 90,70" fill="green" />
           </svg>
         </div>
       
         <!-- 🟣 Purple Starcore (Diamond center + 4 arrow tips) -->
         <div class="enemy">
           <svg viewBox="0 0 100 100">
             <polygon points="50,30 70,50 50,70 30,50" fill="purple" />
             <polygon points="50,0 56,50 50,100 44,50" fill="purple" />
             <polygon points="0,50 50,56 100,50 50,44" fill="purple" />
           </svg>
         </div>
       
         <!-- 🟠 Orange Stinger (Pentagon body + downward triangle tail) -->
         <div class="enemy">
           <svg viewBox="0 0 100 100">
             <polygon points="50,20 80,40 65,80 35,80 20,40" fill="orange" />
             <polygon points="45,80 55,80 50,100" fill="orange" />
           </svg>
         </div>
       
       </body>
       </html>
       
     */
}

