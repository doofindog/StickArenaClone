using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITickableEntity
{
    public void UpdateTick(int tick);
}
