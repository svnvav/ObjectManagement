﻿using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameController : PersistableObject
{
    const int saveVersion = 6;
    public float CreationSpeed { get; set; }
    public float DestructionSpeed { get; set; }

    [SerializeField] private ShapeFactory _shapeFactory;
    [SerializeField] private PersistentStorage _storage;

    [SerializeField] private bool _reseedOnLoad;

    [SerializeField] private KeyCode _spawnKey;
    [SerializeField] private KeyCode _destroyKey = KeyCode.X;
    [SerializeField] private KeyCode _newGameKey;
    [SerializeField] private KeyCode _saveKey = KeyCode.S;
    [SerializeField] private KeyCode _loadKey = KeyCode.L;
    
    private List<Shape> _shapes;
    private float _creationProgress, _destructionProgress;
    private int _loadedLevelBuildIndex;
    private Random.State _mainRandomState;

    private void Awake()
    {
        _mainRandomState = Random.state;
        _creationProgress = 0f;
        _shapes = new List<Shape>();
    }

    private void Start()
    {
        NewGame();
        
#if UNITY_EDITOR
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene loadedScene = SceneManager.GetSceneAt(i);
            if (loadedScene.name.Contains("Level"))
            {
                SceneManager.SetActiveScene(loadedScene);
                _loadedLevelBuildIndex = loadedScene.buildIndex;
                return;
            }
        }
#endif

        StartCoroutine(LoadLevel(1));
    }

    private void Update()
    {
        for (int i = 1; i <= SceneManager.sceneCountInBuildSettings; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                NewGame();
                StartCoroutine(LoadLevel(i));
                return;
            }
        }

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
            StartCoroutine(LoadLevel(_loadedLevelBuildIndex));
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

    private IEnumerator LoadLevel(int levelBuildIndex)
    {
        enabled = false;
        if (_loadedLevelBuildIndex > 0)
        {
            yield return SceneManager.UnloadSceneAsync(_loadedLevelBuildIndex);
        }
        yield return
            SceneManager.LoadSceneAsync(levelBuildIndex, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(levelBuildIndex));
        _loadedLevelBuildIndex = levelBuildIndex;
        enabled = true;
    }

    private void SpawnShape()
    {
        var instance = _shapeFactory.GetRandom();
        instance.transform.localPosition = GameLevel.Current.SpawnPoint;
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
            _shapeFactory.Reclaim(_shapes[index]);
            int lastIndex = _shapes.Count - 1;
            _shapes[index] = _shapes[lastIndex];
            _shapes.RemoveAt(lastIndex);
        }
    }

    private void NewGame()
    {
        Random.state = _mainRandomState;
        int seed = Random.Range(0, int.MaxValue) ^ (int)Time.unscaledTime;
        _mainRandomState = Random.state;
        Random.InitState(seed);

        foreach (var instance in _shapes)
        {
            _shapeFactory.Reclaim(instance);
        }

        _shapes.Clear();
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(_shapes.Count);
        writer.Write(Random.state);
        writer.Write(_loadedLevelBuildIndex);
        GameLevel.Current.Save(writer);

        for (int i = 0; i < _shapes.Count; i++)
        {
            writer.Write(_shapes[i].ShapeId);
            writer.Write(_shapes[i].MaterialId);
            _shapes[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        var version = reader.Version;
        if (version > saveVersion)
        {
            Debug.LogError("Unsupported future save version " + version);
            return;
        }

        StartCoroutine(LoadGame(reader));
        
    }

    private IEnumerator LoadGame(GameDataReader reader)
    {
        var version = reader.Version;
        
        int count = version <= 0 ? -version : reader.ReadInt();
        
        if (version >= 5)
        {
            var randomState = reader.ReadRandomState();
            if (!_reseedOnLoad)
            {
                Random.state = randomState;
            }
        }
        
        yield return LoadLevel(version < 4 ? 1 : reader.ReadInt());
        
        if (version >= 6) {
            GameLevel.Current.Load(reader);
        }

        for (int i = 0; i < count; i++)
        {
            var shapeId = version > 0 ? reader.ReadInt() : 0;
            var materialId = version > 1 ? reader.ReadInt() : 0;
            Shape instance = _shapeFactory.Get(shapeId, materialId);
            instance.Load(reader);
            _shapes.Add(instance);
        }
    }
}