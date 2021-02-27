using System.Collections;
using UnityEngine;

public class DynamicObject : MonoBehaviour
{
    public delegate void LoadObjectStateEvent(ObjectState objectState);
    public event LoadObjectStateEvent loadObjectStateDelegates;

    public delegate void PrepareToSaveEvent(ObjectState objectState);
    public event PrepareToSaveEvent prepareToSaveDelegates;

    public ObjectState objectState;

    void Start()
    {
        if ((objectState != null) && objectState.isPrefab && objectState.guid.Equals(objectState.prefabGuid))
        {
            // Create a unique guid for each prefab instantiation
            objectState.guid = ObjectState.CreateGuid();
        }
    }

    void Update()
    {
        
    }

    public void Load(ObjectState objectState)
    {
        this.objectState = objectState;
        StartCoroutine(LoadAfterFrame(objectState));
    }

    private IEnumerator LoadAfterFrame(ObjectState objectState)
    {
        // Wait for the next frame so that all objects have been created from ObjectStates
        yield return null;
        loadObjectStateDelegates?.Invoke(objectState);
    }

    public void PrepareToSave()
    {
        prepareToSaveDelegates?.Invoke(objectState);
    }
}
