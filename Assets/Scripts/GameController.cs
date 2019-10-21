using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DefaultNamespace;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : PersistableObject
{
    [SerializeField]
    private PersistableObject _prefab;

    [SerializeField] 
    private PersistentStorage _storage;

    [SerializeField] 
    private KeyCode _spawnKey;
    [SerializeField] 
    private KeyCode _newGameKey;
    [SerializeField] 
    private KeyCode _saveKey = KeyCode.S;
    [SerializeField] 
    private KeyCode _loadKey = KeyCode.L;

    private List<PersistableObject> _objects;
    

    private void Awake()
    {
        _objects = new List<PersistableObject>();
    }
    
    void Update()
    {
        if (Input.GetKey(_spawnKey))
        {
            SpawnObject();
        }
        
        if (Input.GetKeyDown(_newGameKey))
        {
            NewGame();
        }
        else if (Input.GetKeyDown(_saveKey))
        {
            _storage.Save(this);
        }
        else if (Input.GetKeyDown(_loadKey))
        {
            _storage.Load(this);
        }
    }

    private void SpawnObject()
    {
        var instance = Instantiate(_prefab);
        instance.transform.localPosition = Random.insideUnitSphere * 5f;
        instance.transform.localRotation = Random.rotation;
        instance.transform.localScale = Random.value * Vector3.one;
        _objects.Add(instance);
    }

    private void NewGame()
    {
        foreach (var instance in _objects)
        {
            Destroy(instance.gameObject);
        }
        
        _objects.Clear();
    }
    
    public override void Save (GameDataWriter writer) {
        writer.Write(_objects.Count);
        for (int i = 0; i < _objects.Count; i++) {
            _objects[i].Save(writer);
        }
    }
    
    public override void Load (GameDataReader reader) {
        int count = reader.ReadInt();
        for (int i = 0; i < count; i++) {
            PersistableObject instance = Instantiate(_prefab);
            instance.Load(reader);
            _objects.Add(instance);
        }
    }
}
