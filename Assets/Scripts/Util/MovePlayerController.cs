using System;
using System.Collections;
using System.Collections.Generic;
using Entities;
using Grid;
using StateManager;
using TMPro;
using UnityEngine;

namespace Util {
    public class MovePlayerController : MonoBehaviour
    {
        public bool isMoving = false;
        
        public static void StaticHexClickCallback(Vector2Int hexPosition, GameObject go)
        {
            MovePlayerController.instance.HexClickCallback(hexPosition);
        }
        public static MovePlayerController instance;

        public void Awake()
        {
            instance = this;
        }

        public void UpdateMovableParticles(PlayingState playingState)
        {
            
            Dictionary<Vector2Int, int> currentMap =
                CalculateDistanceMap(playingState.player.positionRowCol, playingState);
            
            foreach (var key in currentMap.Keys)
            {
                if (currentMap[key] <= RunInfo.Instance.CurrentSteps && currentMap[key] > 0 && playingState.CurrentTurn is Player)
                {
                    GOList list = HexGridManager.Instance.GetWorldHexObject(key)
                        .GetComponent<GOList>();
                    list.GetValue("Particles").SetActive(true);
                }
                else
                {
                    GOList list = HexGridManager.Instance.GetWorldHexObject(key)
                        .GetComponent<GOList>();
                    list.GetValue("Particles").SetActive(false);
                }
            }
        }
        

        public void HexClickCallback(Vector2Int hexPosition)
        {
            Debug.Log("CLICKED " + hexPosition);
            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            
            if (!(playingState.CurrentTurn is Player))
                return;
            
            
            Dictionary<Vector2Int, int> distanceMap = CalculateDistanceMap(hexPosition, playingState);
            
            // If player exists, and has enough steps, and is not already moving
            if (distanceMap.ContainsKey(playingState.player.positionRowCol) &&
                distanceMap[playingState.player.positionRowCol] <= RunInfo.Instance.CurrentSteps && !isMoving)
            {
                StartCoroutine(MovePlayer(distanceMap, playingState, hexPosition));
            }
        }

        public Dictionary<Vector2Int, int> CalculateDistanceMap(Vector2Int hexPosition, PlayingState playingState)
        {
            List<Vector2Int> blockers = new List<Vector2Int>();
            
            foreach (AbstractEntity abstractEntity in playingState.GetEntities())
            {
                if (abstractEntity is Player)
                    continue;
                
                blockers.Add(abstractEntity.positionRowCol);
            }
            
            Dictionary<Vector2Int, int> distanceMap = HexGridManager.Instance.CalculateDistanceMap(hexPosition, blockers);
            
            foreach (Vector2Int pos in distanceMap.Keys)
            {
                HexGridManager.Instance.GetWorldHexObject(pos).transform.GetChild(5).GetComponent<TextMeshPro>().SetText("" + distanceMap[pos]);
            }
            
            return distanceMap;
        }

        IEnumerator MovePlayer(Dictionary<Vector2Int, int> distanceMap, PlayingState playingState, Vector2Int targetPosition)
        {
            isMoving = true;
            Dictionary<Vector2Int, int> currentMap = CalculateDistanceMap(targetPosition, playingState);

            // While we are walking to point
            while (currentMap[playingState.player.positionRowCol] != 0)
            {
                List<Vector2Int> positions =  HexGridManager.AdjacentHexes(playingState.player.positionRowCol);

                if (RunInfo.Instance.CurrentSteps == 0)
                {
                    break;
                }
                
                bool moved = false;
                foreach (Vector2Int pos in positions)
                {
                    if (currentMap.ContainsKey(pos) &&
                        currentMap[pos] < distanceMap[playingState.player.positionRowCol])
                    {
                        if (playingState.MoveEntity(playingState.player, pos)) 
                        {
                            yield return new WaitForSeconds(0.15f);
                            moved = true;
                            RunInfo.Instance.CurrentSteps -= 1;
                                        
                            break;
                        }

                    }
                }
                
                if (!moved) 
                {
                    break;
                }


                currentMap = CalculateDistanceMap(targetPosition, playingState);
            }
            isMoving = false;
            
            UpdateMovableParticles(playingState);
        }
    }
}
