
using System;
using System.Collections;
using System.Collections.Generic;
using Catlike.ObjectManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : PersistableObject
{
    public static GameController Instance { get; private set; }
    
    const int saveVersion = 9;
    public float CreationSpeed { get; set; }
    public float DestructionSpeed { get; set; }

    [SerializeField] Slider _creationSpeedSlider;
    [SerializeField] Slider _destructionSpeedSlider;
    
    [SerializeField] private ShapeFactory[] _shapeFactories;
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
        Instance = this;
        _mainRandomState = Random.state;
        _creationProgress = 0f;
        _shapes = new List<Shape>();
    }

    private void OnEnable()
    {
        if (_shapeFactories[0].FactoryId != 0)
        {
            for (int i = 0; i < _shapeFactories.Length; i++)
            {
                _shapeFactories[i].FactoryId = i;
            }
        }
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

        if (Input.GetKeyDown(_spawnKey))
        {
            GameLevel.Current.SpawnShape();
        }

        if (Input.GetKeyDown(_destroyKey))
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

    private void FixedUpdate()
    {
        foreach (var shape in _shapes)
        {
            shape.GameUpdate();
        }
        
        _creationProgress += Time.deltaTime * CreationSpeed;
        while (_creationProgress >= 1f)
        {
            _creationProgress -= 1f;
            GameLevel.Current.SpawnShape();
        }

        _destructionProgress += Time.deltaTime * DestructionSpeed;
        while (_destructionProgress >= 1f)
        {
            _destructionProgress -= 1f;
            DestroyShape();
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

    public void AddShape (Shape shape) {
        _shapes.Add(shape);
    }

    private void DestroyShape()
    {
        if (_shapes.Count > 0)
        {
            int index = Random.Range(0, _shapes.Count);
            _shapes[index].Recycle();
            int lastIndex = _shapes.Count - 1;
            _shapes[index] = _shapes[lastIndex];
            _shapes.RemoveAt(lastIndex);
        }
    }

    private void NewGame()
    {
        _creationSpeedSlider.value = CreationSpeed = 0;
        _destructionSpeedSlider.value = DestructionSpeed = 0;
        
        Random.state = _mainRandomState;
        int seed = Random.Range(0, int.MaxValue) ^ (int)Time.unscaledTime;
        _mainRandomState = Random.state;
        Random.InitState(seed);

        foreach (var instance in _shapes)
        {
            instance.Recycle();
        }

        _shapes.Clear();
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(_shapes.Count);
        writer.Write(Random.state);
        writer.Write(CreationSpeed);
        writer.Write(_creationProgress);
        writer.Write(DestructionSpeed);
        writer.Write(_destructionProgress);
        writer.Write(_loadedLevelBuildIndex);
        GameLevel.Current.Save(writer);

        for (int i = 0; i < _shapes.Count; i++)
        {
            writer.Write(_shapes[i].OriginFactory.FactoryId);
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

            _creationSpeedSlider.value = CreationSpeed = reader.ReadFloat();
            _creationProgress = reader.ReadFloat();
            _destructionSpeedSlider.value = DestructionSpeed = reader.ReadFloat();
            _destructionProgress = reader.ReadFloat();
        }
        
        yield return LoadLevel(version < 4 ? 1 : reader.ReadInt());
        
        if (version >= 6) {
            GameLevel.Current.Load(reader);
        }

        for (int i = 0; i < count; i++)
        {
            int factoryId = version >= 8 ? reader.ReadInt() : 0;
            var shapeId = version > 0 ? reader.ReadInt() : 0;
            var materialId = version > 1 ? reader.ReadInt() : 0;
            Shape instance = _shapeFactories[factoryId].Get(shapeId, materialId);
            instance.Load(reader);
            //_shapes.Add(instance);
        }
    }
}