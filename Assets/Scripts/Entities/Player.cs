using System;
using TMPro;
using UnityEngine;

namespace Entities
{
    public class Player : AbstractEntity
    {

        public static Player Instance;

        public void Awake()
        {
            if (Instance != null)
            {
                throw new Exception("Duplicate player instance");
            }
            
            Instance = this;
        }

        public override void Die()
        {
            
        }
    }
}