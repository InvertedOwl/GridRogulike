using System;
using UnityEngine;

namespace Util
{
    [Serializable]
    public class InfoEntry
    {
        [SerializeField]
        public string name;
        
        [SerializeField]
        public string formattedName;
        
        [SerializeField]
        public GameObject infoPanel;
    }
}