using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//very similar to a syncedobject, except from the fact that it always will belong to the server and must belong to a scene object
public class SceneSyncedObject : SyncedObject
{
    public new void Start()
    {
        //do all the normal stuff for a synced object
        base.Start();

        //if a server exists, we're locally owned, otherwise it can be set false
        localOwned = server != null;
        Debug.LogError(localOwned.ToString());
    }
}
