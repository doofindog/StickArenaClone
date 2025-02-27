using UnityEngine;
public abstract class PlayerFeature
{
    public abstract void Init(NetController pController);
    public abstract void Process(NetInputPayLoad pInputPayLoad);
    public abstract void Process(NetStatePayLoad pStatePayLoad);
}
