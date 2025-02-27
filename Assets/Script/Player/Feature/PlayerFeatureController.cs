using System.Collections.Generic;
using UnityEngine;

public class PlayerFeatureController : MonoBehaviour
{
    [SerializeField] private List<PlayerFeature> m_features;

    public void Init(NetController netController)
    {
        m_features = new List<PlayerFeature>();

        m_features.Add(new Movement());
        m_features.Add(new Flip());
        m_features.Add(new Aim());
        m_features.Add(new Shoot());

        foreach(PlayerFeature feature in m_features)
        {
            feature.Init(netController);
        }
    }

    public void ProcessFeature(NetInputPayLoad pInputPayload)
    {
        foreach(var feature in m_features)
        {
            feature.Process(pInputPayload);
        }
    }

    public void ProcessFeature(NetStatePayLoad pStatePayload)
    {
        foreach(var feature in m_features)
        {
            feature.Process(pStatePayload);
        }
    }
}
