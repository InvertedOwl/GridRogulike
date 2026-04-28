using System.Collections;
using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using StateManager;
using TMPro;
using UnityEngine;
using Util;

namespace Grid {
    public class HexClickPlayerController : MonoBehaviour
    {
        public bool isMoving = false;
        public bool isAttacking = false;

        public List<AttackCardEvent> ToAttack = new List<AttackCardEvent>();
        private CardMonobehaviour _pendingCard;
        private bool _pendingCardHasStarted;

        public void AddToAttack(IEnumerable<AbstractCardEvent> cardEvents)
        {
            foreach (AbstractCardEvent cardEvent in cardEvents)
            {
                ToAttack.Add((AttackCardEvent) cardEvent);
            }
        }

        public void BeginCardAttack(List<AttackCardEvent> cardEvents, CardMonobehaviour card)
        {
            ClearPendingAttacks();

            ToAttack.AddRange(cardEvents);
            _pendingCard = card;
            _pendingCardHasStarted = false;
            SetupAttack();
        }

        public void PreviewAttackEvents(IEnumerable<AttackCardEvent> cardEvents)
        {
            ClearToAttackEmitters();

            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsCurrent<PlayingState>())
                return;

            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();

            foreach (AttackCardEvent attackCardEvent in cardEvents)
            {
                if (!TryGetAttackPosition(attackCardEvent, playingState.player.positionRowCol, out Vector2Int attackPosition))
                    continue;

                ShowAttackPreview(attackPosition);
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

        public void Awake ()
        {
            instance = this;
        }

        public void Update()
        {
            if (ToAttack.Count > 0 && !isAttacking)
            {
                SetupAttack();
            }

            if (ToAttack.Count == 0)
            {
                isAttacking = false;
            }
        }

        public void SetupAttack()
        {
            if (ToAttack.Count == 0)
            {
                isAttacking = false;
                return;
            }

            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            
            Dictionary<Vector2Int, int> noBlockerMapFromPlayer = HexGridManager.Instance.CalculateDistanceMap(playingState.player.positionRowCol, new List<Vector2Int>());

            int numEntitiesToHit = 0;
            
            foreach (AbstractEntity entity in playingState.GetEntities())
            {
                if (!(entity is NonPlayerEntity))
                    continue;

                if (noBlockerMapFromPlayer[entity.positionRowCol] > ToAttack[0].distance ||
                    noBlockerMapFromPlayer[entity.positionRowCol] == -1)
                    continue;
                EaseColor tileColor = HexGridManager.Instance.GetWorldHexObject(entity.positionRowCol)
                    .GetComponent<GOList>().GetValue("RedGlow").GetComponent<EaseColor>();
                Debug.Log("Settin red glow to 60 percent opacity");
                Debug.Log(tileColor);
                tileColor.SendToColor(new Color(tileColor.targetColor.r, tileColor.targetColor.g, tileColor.targetColor.b, 0.6f));
                numEntitiesToHit += 1;
            }

            if (numEntitiesToHit == 0)
            {
                if (_pendingCard != null)
                    ClearPendingAttacks();
                else
                    ResolveCurrentAttack();

                isAttacking = false;
                return;
            }

            isAttacking = true;
        }

        public void ClearPendingAttacks()
        {
            if (_pendingCard != null)
            {
                if (_pendingCardHasStarted)
                    _pendingCard.FinishManualAttackResolution();
                else
                    _pendingCard.CancelManualAttackTargeting();
            }

            ClearToAttackEmitters();
            if (SpriteArrowManager.Instance != null)
                SpriteArrowManager.Instance.DestroyArrow(arrowUUID);
            ToAttack.Clear();
            _pendingCard = null;
            _pendingCardHasStarted = false;
            isAttacking = false;
        }

        private void ResolveCurrentAttack()
        {
            if (ToAttack.Count == 0)
                return;

            ToAttack.RemoveAt(0);
        }

        private void FinishPendingCardAttack()
        {
            if (_pendingCard != null)
                _pendingCard.FinishManualAttackResolution();

            _pendingCard = null;
            _pendingCardHasStarted = false;
            isAttacking = false;
        }

        public void ClearToAttackEmitters()
        {
            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsCurrent<PlayingState>())
                return;

            foreach (Vector2Int position in HexGridManager.Instance.BoardDictionary.Keys)
            {
                EaseColor tileColor = HexGridManager.Instance.GetWorldHexObject(position)
                    .GetComponent<GOList>()
                    .GetValue("RedGlow")
                    .GetComponent<EaseColor>();
                tileColor.targetColor = new Color(tileColor.targetColor.r, tileColor.targetColor.g, tileColor.targetColor.b, 0.0f);
            }
        }

        private bool TryGetAttackPosition(AttackCardEvent attackCardEvent, Vector2Int origin, out Vector2Int attackPosition)
        {
            if (!attackCardEvent.usePosition && string.IsNullOrEmpty(attackCardEvent.direction) && attackCardEvent.distance > 0)
            {
                attackPosition = Vector2Int.zero;
                return false;
            }

            attackPosition = attackCardEvent.usePosition
                ? attackCardEvent.position
                : HexGridManager.MoveHex(origin, attackCardEvent.direction, attackCardEvent.distance);

            return HexGridManager.Instance.BoardDictionary.ContainsKey(attackPosition);
        }

        private void ShowAttackPreview(Vector2Int attackPosition)
        {
            EaseColor tileColor = HexGridManager.Instance.GetWorldHexObject(attackPosition)
                .GetComponent<GOList>()
                .GetValue("RedGlow")
                .GetComponent<EaseColor>();
            tileColor.SendToColor(new Color(tileColor.targetColor.r, tileColor.targetColor.g, tileColor.targetColor.b, 0.6f));
        }

        public void UpdateMovableParticles(PlayingState playingState)
        {

            if (!GameStateManager.Instance.IsCurrent<PlayingState>())
                return;
            
            Dictionary<Vector2Int, int> currentMap =
                CalculateDistanceMap(playingState.player.positionRowCol, playingState);
            
            foreach (var key in currentMap.Keys)
            {
                if (currentMap[key] <= RunInfo.Instance.CurrentSteps && currentMap[key] > 0 && playingState.CurrentTurn.entityType == EntityType.Player)
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
            
            if (playingState.CurrentTurn.entityType != EntityType.Player)
                return;

            if (!playingState.AllowUserInput && !isAttacking)
                return;
            
            Dictionary<Vector2Int, int> distanceMap = CalculateDistanceMap(hexPosition, playingState);
            Dictionary<Vector2Int, int> noBlockerMapFromPlayer = HexGridManager.Instance.CalculateDistanceMap(playingState.player.positionRowCol, new List<Vector2Int>());
            
            List<AbstractEntity> entitiesOnHex = new List<AbstractEntity>();
            playingState.EntitiesOnHex(hexPosition, out entitiesOnHex);
            
            // If player is attacking, and the target is within the distance in the event
            // and the tile has an entity and that entity is not the player
            if (isAttacking)
            {
                bool validAttackTarget =
                    ToAttack.Count > 0 &&
                    noBlockerMapFromPlayer.TryGetValue(hexPosition, out int distanceFromPlayer) &&
                    distanceFromPlayer >= 0 &&
                    distanceFromPlayer <= ToAttack[0].distance &&
                    entitiesOnHex.Count > 0 &&
                    !entitiesOnHex.Contains(playingState.player);

                if (!validAttackTarget)
                    return;

                if (_pendingCard != null && !_pendingCardHasStarted)
                {
                    if (!_pendingCard.TryStartManualAttackPlay(out List<AttackCardEvent> attackCardEvents))
                    {
                        ClearPendingAttacks();
                        return;
                    }

                    ToAttack.Clear();
                    ToAttack.AddRange(attackCardEvents);
                    _pendingCardHasStarted = true;

                    if (ToAttack.Count == 0)
                    {
                        ClearToAttackEmitters();
                        FinishPendingCardAttack();
                        playingState.CaptureFinish();
                        return;
                    }
                }

                AttackCardEvent attack = ToAttack[0];

                // Attack entity
                playingState.DamageEntities(hexPosition, attack.amount, attack.status);
                
                // Little animation for free!
                playingState.player.transform.localPosition += (entitiesOnHex[0].transform.position 
                                                                - playingState.player.transform.position).normalized * 0.5f;
                
                // Reset situation
                SpriteArrowManager.Instance.DestroyArrow(arrowUUID);
                ResolveCurrentAttack();
                ClearToAttackEmitters();

                if (ToAttack.Count > 0)
                {
                    SetupAttack();
                }
                else
                {
                    FinishPendingCardAttack();
                }

                playingState.CaptureFinish();
                return;
            }
            
            // If player exists, and has enough steps, and is not already moving
            if (distanceMap.ContainsKey(playingState.player.positionRowCol) &&
                distanceMap[playingState.player.positionRowCol] <= RunInfo.Instance.CurrentSteps && !isMoving)
            {
                StartCoroutine(MovePlayer(distanceMap, playingState, hexPosition));
            }
            
            playingState.CaptureFinish();
        }

        public Dictionary<Vector2Int, int> CalculateDistanceMap(Vector2Int hexPosition, PlayingState playingState)
        {
            List<Vector2Int> blockers = new List<Vector2Int>();
            
            foreach (AbstractEntity abstractEntity in playingState.GetEntities())
            {
                if (abstractEntity.entityType == EntityType.Player)
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
                            yield return new WaitForSeconds(0.3f * (1/GameplayNavSettings.speed));
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
