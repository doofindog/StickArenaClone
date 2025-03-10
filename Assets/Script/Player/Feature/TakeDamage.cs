using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeDamage : PlayerFeature
{
    private PlayerAnimationController m_animController;
    private NetStateProcessor m_stateProcessor;
    private PlayerData m_playerData;
    private Player m_player;

    public override void Init(Player pPlayer)
    {
        pPlayer.TakeDamageEvent += DealDamage;

        m_animController = pPlayer.playerAnimController;
        m_stateProcessor = pPlayer.stateProcessor;
        m_playerData = pPlayer.playerData;
        m_player = pPlayer;
    }

    public override void Process(NetInputPayLoad pInputPayLoad)
    {
                   
    }

    public override void Process(NetStatePayLoad pStatePayLoad)
    {

    }

    private void DealDamage(HitResponseData pHitResponse)
    {
        void HandleServer()
        {
            bool failedCheck = m_playerData.health.Value <= 0 ||
                               pHitResponse == null ||
                               m_stateProcessor.frameHistory.Count == 0;

            if (failedCheck)
            {
                Debugger.Log("Damage Check Failed");
                return;
            }

            NetStatePayLoad stateToCheck = new NetStatePayLoad();
            LinkedList<NetStatePayLoad> history = m_stateProcessor.frameHistory;

            bool scheduleRewind = true;
            float oldestHistoryTime = history.Last.Value.time;
            float newestHistoryTime = history.First.Value.time;

            Debug.Log($"Check Time : {pHitResponse.hitTime} : {oldestHistoryTime} : {newestHistoryTime}");

            //Drop Check if hitTime is lesser that the oldest frame Time
            if(oldestHistoryTime > pHitResponse.hitTime)
            {
                Debugger.Log($"Histroy Too Far Back Skipping Hit");
                return;
            }

            if(oldestHistoryTime == pHitResponse.hitTime)
            {
                stateToCheck = history.Last.Value;
                scheduleRewind = false;
            }

            if(newestHistoryTime <= pHitResponse.hitTime)
            {
                stateToCheck = history.First.Value;
                scheduleRewind = true;
            }


            LinkedListNode<NetStatePayLoad> older = history.Last;
            LinkedListNode<NetStatePayLoad> younger = history.First;
            while (older.Value.time > pHitResponse.hitTime)
            {
                if(older.Previous == null)
                {
                    break;
                }

                older = older.Previous;
                if(older.Value.time > pHitResponse.hitTime)
                {
                    younger = older;
                }
            }

            if(older.Value.time == pHitResponse.hitTime)
            {
                stateToCheck = older.Value;
                scheduleRewind = false;
            }

            if(scheduleRewind)
            {
                float distance = younger.Value.time - older.Value.time;
                float interpFractor = Mathf.Clamp01((pHitResponse.hitTime - older.Value.time) / distance);
                stateToCheck.position = Vector3.Lerp(older.Value.position, younger.Value.position, interpFractor);
                stateToCheck.time = pHitResponse.hitTime;
            }

            if(ConfirmHit(pHitResponse))
            {
                if(m_playerData.health.Value > 0)
                {
                    m_playerData.health.Value -= 1;
                }
            }

        }

        void HandleClient()
        {
            m_animController.PlayTakeDamage(false);
        }

        GameUtilt.ExecuteNetworkCode(HandleServer, HandleClient);
    }

    private bool ConfirmHit(HitResponseData pHitData)
    {
        Vector3 simPosition = pHitData.traceStart;
        float simTimer = 0.0f;
        float frequency = pHitData.hitVelocity;
        float maxSimTime = 5.0f;
        float subStep = maxSimTime / Time.fixedDeltaTime;
        while(simTimer <= maxSimTime)
        {
            simTimer += Time.fixedDeltaTime;
            simPosition += pHitData.projectileDirection.normalized * pHitData.hitVelocity * Time.deltaTime;
            Collider2D col = Physics2D.OverlapBox(simPosition, Vector2.one, 0);
            if(col != null && col.gameObject == m_player.gameObject)
            {
                return true;
            }
        }

        return false;
    }
}
