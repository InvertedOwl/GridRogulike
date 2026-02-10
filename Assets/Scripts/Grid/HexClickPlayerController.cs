using System.Collections;
using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using StateManager;
using TMPro;
using UnityEngine;

namespace Grid {
    public class HexClickPlayerController : MonoBehaviour
    {
        public bool isMoving = false;
        public bool isAttacking = false;

        public List<AttackCardEvent> ToAttack = new List<AttackCardEvent>();

        public void AddToAttack(List<AbstractCardEvent> cardEvents)
        {
            foreach (AbstractCardEvent cardEvent in cardEvents)
            {
                ToAttack.Add((AttackCardEvent) cardEvent);
            }

            
        }
        
        
        public static void StaticHexClickCallback(Vector2Int hexPosition, GameObject go)
        {
            HexClickPlayerController.instance.HexClickCallback(hexPosition);
        }

        public static void StaticHexHoverOnCallback(Vector2Int hexPosition, GameObject go)
        {
            HexClickPlayerController.instance.HexHoverOnCallback(hexPosition);
        }
        public static void StaticHexHoverOffCallback(Vector2Int hexPosition, GameObject go)
        {
            HexClickPlayerController.instance.HexHoverOffCallback(hexPosition);
        }
        
        public static HexClickPlayerController instance;

        public void Awake()
        {
            instance = this;
        }

        public void Update()
        {
            if (ToAttack.Count > 0 && isAttacking == false)
            {
                SetupAttack();
            }
            
            if (ToAttack.Count > 0)
            {
                isAttacking = true;
            }
            else
            {
                isAttacking = false;
            }
        }

        public void SetupAttack()
        {
            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            playingState.AllowUserInput = false;
            
            Dictionary<Vector2Int, int> noBlockerMapFromPlayer = HexGridManager.Instance.CalculateDistanceMap(playingState.player.positionRowCol, new List<Vector2Int>());

            int numEntitiesToHit = 0;
            
            foreach (AbstractEntity entity in playingState.GetEntities())
            {
                if (!(entity is Enemy))
                    continue;

                if (noBlockerMapFromPlayer[entity.positionRowCol] > ToAttack[0].distance ||
                    noBlockerMapFromPlayer[entity.positionRowCol] == -1)
                    continue;
                
                HexGridManager.Instance.GetWorldHexObject(entity.positionRowCol).GetComponent<GOList>().GetValue("ToAttack").SetActive(true);
                numEntitiesToHit += 1;
            }

            if (numEntitiesToHit == 0)
            {
                ToAttack.RemoveAt(0);
                isAttacking = false;
                playingState.AllowUserInput = true;
            }
        }

        public void ClearToAttackEmitters()
        {
            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            
            
            foreach (AbstractEntity entity in playingState.GetEntities())
            {
                if (!(entity is Enemy))
                    continue;
                HexGridManager.Instance.GetWorldHexObject(entity.positionRowCol).GetComponent<GOList>().GetValue("ToAttack").SetActive(false);
            }
        }

        public void UpdateMovableParticles(PlayingState playingState)
        {

            if (!GameStateManager.Instance.IsCurrent<PlayingState>())
                return;
            
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

        private string arrowUUID;
        
        public void HexHoverOnCallback(Vector2Int hexPosition)
        {
            if (!GameStateManager.Instance.IsCurrent<PlayingState>())
                return;

            if (!isAttacking)
                return;
        
            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            Dictionary<Vector2Int, int> distanceMap = HexGridManager.Instance.CalculateDistanceMap(playingState.player.positionRowCol, new List<Vector2Int>());


            bool isHoveringEntity = false;
            foreach (AbstractEntity entity in playingState.GetEntities())
            {
                if (entity.positionRowCol == hexPosition)
                {
                    isHoveringEntity = true;
                }
            }

            if (distanceMap[hexPosition] == 1 && isHoveringEntity)
            {
                arrowUUID = SpriteArrowManager.Instance.CreateArrow(playingState.player.positionRowCol, hexPosition, Color.red, "AttackIcon", ToAttack[0].amount);
            }
        }
        public void HexHoverOffCallback(Vector2Int hexPosition)
        {
            SpriteArrowManager.Instance.DestroyArrow(arrowUUID);
        }

        public void HexClickCallback(Vector2Int hexPosition)
        {
            Debug.Log("CLICKED " + hexPosition);
            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            
            if (!(playingState.CurrentTurn is Player))
                return;

            if (!playingState.AllowUserInput && !isAttacking)
                return;
            
            Dictionary<Vector2Int, int> distanceMap = CalculateDistanceMap(hexPosition, playingState);
            Dictionary<Vector2Int, int> noBlockerMapFromPlayer = HexGridManager.Instance.CalculateDistanceMap(playingState.player.positionRowCol, new List<Vector2Int>());
            
            List<AbstractEntity> entitiesOnHex = new List<AbstractEntity>();
            playingState.EntitiesOnHex(hexPosition, out entitiesOnHex);
            
            // If player is attacking, and the target is within the distance in the event
            // and the tile has an entity and that entity is not the player
            if (isAttacking 
                    && noBlockerMapFromPlayer[hexPosition] <= ToAttack[0].distance 
                    && entitiesOnHex.Count > 0 && !entitiesOnHex.Contains(playingState.player))
            {
                Debug.Log("Attacking " + hexPosition);
                // NEED TO CHECK FOR IF PLAYER CANNOT ATTACK
                // MIGHT NOT BE HERE BUT STILL!!

                // Attack entity
                playingState.DamageEntities(hexPosition, ToAttack[0].amount, ToAttack[0].status);
                
                // Reset situation
                SpriteArrowManager.Instance.DestroyArrow(arrowUUID);
                ToAttack.RemoveAt(0);
                playingState.AllowUserInput = true;
                ClearToAttackEmitters();

                if (ToAttack.Count > 0)
                {
                    HexHoverOnCallback(hexPosition);
                }
                isAttacking = false;
            }
            
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
            playingState.AllowUserInput = false;
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
                            yield return new WaitForSeconds(0.3f);
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
            playingState.AllowUserInput = true;
        }
    }
}
