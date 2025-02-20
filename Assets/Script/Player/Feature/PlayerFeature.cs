using UnityEngine;
public abstract class PlayerFeature
{
    protected NetController controller;
    protected Transform transform;

    public PlayerFeature(NetController pController)
    {
        controller = pController;
        transform = pController.transform;
    }

    public abstract void Process(NetInputPayLoad pInputPayLoad);
}
