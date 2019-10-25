using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameController : PersistableObject
{
    const int saveVersion = 3;

    public ShapeFactory ShapeFactory;
    public float CreationSpeed { get; set; }
    public float DestructionSpeed { get; set; }

    [SerializeField] private PersistentStorage _storage;

    [SerializeField] private KeyCode _spawnKey;
    [SerializeField] private KeyCode _destroyKey = KeyCode.X;
    [SerializeField] private KeyCode _newGameKey;
    [SerializeField] private KeyCode _saveKey = KeyCode.S;
    [SerializeField] private KeyCode _loadKey = KeyCode.L;

    private List<Shape> _shapes;
    private float _creationProgress, _destructionProgress;


    private void Awake()
    {
        _creationProgress = 0f;
        _shapes = new List<Shape>();
    }

    private void Start()
    {
#if UNITY_EDITOR
        Scene loadedLevel = SceneManager.GetSceneByName("Level1");
        if (loadedLevel.isLoaded)
        {
            SceneManager.SetActiveScene(loadedLevel);
            return;
        }
#endif

        StartCoroutine(LoadLevel());
    }

    void Update()
    {
        _creationProgress += Time.deltaTime * CreationSpeed;
        while (_creationProgress >= 1f)
        {
            _creationProgress -= 1f;
            SpawnShape();
        }

        _destructionProgress += Time.deltaTime * DestructionSpeed;
        while (_destructionProgress >= 1f)
        {
            _destructionProgress -= 1f;
            DestroyShape();
        }

        if (Input.GetKey(_spawnKey))
        {
            SpawnShape();
        }

        if (Input.GetKey(_destroyKey))
        {
            DestroyShape();
        }

        if (Input.GetKeyDown(_newGameKey))
        {
            NewGame();
        }
        else if (Input.GetKeyDown(_saveKey))
        {
            _storage.Save(this, saveVersion);
        }
        else if (Input.GetKeyDown(_loadKey))
        {
            _storage.Load(this);
        }
    }

    private IEnumerator LoadLevel()
    {
        enabled = false;
        yield return
            SceneManager.LoadSceneAsync("Level1", LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Level1"));
        enabled = true;
    }

    private void SpawnShape()
    {
        var instance = ShapeFactory.GetRandom();
        instance.transform.localPosition = Random.insideUnitSphere * 5f;
        instance.transform.localRotation = Random.rotation;
        instance.transform.localScale = Random.value * Vector3.one;
        instance.SetColor(Random.ColorHSV(
            hueMin: 0f, hueMax: 1f,
            saturationMin: 0.5f, saturationMax: 1f,
            valueMin: 0.25f, valueMax: 1f,
            alphaMin: 1f, alphaMax: 1f
        ));
        _shapes.Add(instance);
    }

    private void DestroyShape()
    {
        if (_shapes.Count > 0)
        {
            int index = Random.Range(0, _shapes.Count);
            ShapeFactory.Reclaim(_shapes[index]);
            int lastIndex = _shapes.Count - 1;
            _shapes[index] = _shapes[lastIndex];
            _shapes.RemoveAt(lastIndex);
        }
    }

    private void NewGame()
    {
        foreach (var instance in _shapes)
        {
            ShapeFactory.Reclaim(instance);
        }

        _shapes.Clear();
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(_shapes.Count);
        for (int i = 0; i < _shapes.Count; i++)
        {
            writer.Write(_shapes[i].ShapeId);
            writer.Write(_shapes[i].MaterialId);
            _shapes[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        int version = reader.Version;
        int count = version <= 0 ? -version : reader.ReadInt();
        if (version > saveVersion)
        {
            Debug.LogError("Unsupported future save version " + version);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var shapeId = version > 0 ? reader.ReadInt() : 0;
            var materialId = version > 1 ? reader.ReadInt() : 0;
            Shape instance = ShapeFactory.Get(shapeId, materialId);
            instance.Load(reader);
            _shapes.Add(instance);
        }
    }
}