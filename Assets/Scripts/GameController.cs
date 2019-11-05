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

    const int saveVersion = 10;
    public float CreationSpeed { get; set; }
    public float DestructionSpeed { get; set; }

    [SerializeField] Slider _creationSpeedSlider;
    [SerializeField] Slider _destructionSpeedSlider;
    [SerializeField] float destroyDuration;

    [SerializeField] private ShapeFactory[] _shapeFactories;
    [SerializeField] private PersistentStorage _storage;

    [SerializeField] private bool _reseedOnLoad;

    [SerializeField] private KeyCode _spawnKey;
    [SerializeField] private KeyCode _destroyKey = KeyCode.X;
    [SerializeField] private KeyCode _newGameKey;
    [SerializeField] private KeyCode _saveKey = KeyCode.S;
    [SerializeField] private KeyCode _loadKey = KeyCode.L;

    private List<Shape> _shapes;
    private List<ShapeInstance> _killList, _markAsDyingList;
    private bool _inGameUpdateLoop;
    private float _creationProgress, _destructionProgress;
    private int _loadedLevelBuildIndex;
    private Random.State _mainRandomState;
    private int _dyingShapeCount;

    private void Awake()
    {
        Instance = this;
        _mainRandomState = Random.state;
        _creationProgress = 0f;
        _shapes = new List<Shape>();
        _killList = new List<ShapeInstance>();
        _markAsDyingList = new List<ShapeInstance>();
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
        _inGameUpdateLoop = true;
        foreach (var shape in _shapes)
        {
            shape.GameUpdate();
        }
        _inGameUpdateLoop = false;

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

        var limit = GameLevel.Current.PopulationLimit;
        if (limit > 0)
        {
            while (_shapes.Count - _dyingShapeCount > limit)
            {
                DestroyShape();
            }
        }
        
        if (_killList.Count > 0) {
            for (int i = 0; i < _killList.Count; i++) {
                if (_killList[i].IsValid)
                {
                    KillImmediately(_killList[i].Shape);
                }
            }
            _killList.Clear();
        }
        if (_markAsDyingList.Count > 0) {
            for (int i = 0; i < _markAsDyingList.Count; i++) {
                if (_markAsDyingList[i].IsValid) {
                    MarkAsDyingImmediately(_markAsDyingList[i].Shape);
                }
            }
            _markAsDyingList.Clear();
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

    public void AddShape(Shape shape)
    {
        shape.SaveIndex = _shapes.Count;
        _shapes.Add(shape);
    }

    public Shape GetShape (int index) {
        return _shapes[index];
    }

    public void Kill(Shape shape)
    {
        if (_inGameUpdateLoop) {
            _killList.Add(shape);
        }
        else {
            KillImmediately(shape);
        }
    }
    
    private void KillImmediately (Shape shape) {
        int index = shape.SaveIndex;
        shape.Recycle();
        
        if (index < _dyingShapeCount && index < --_dyingShapeCount) {
            _shapes[_dyingShapeCount].SaveIndex = index;
            _shapes[index] = _shapes[_dyingShapeCount];
            index = _dyingShapeCount;
        }
        
        int lastIndex = _shapes.Count - 1;
        if (index < lastIndex)
        {
            _shapes[lastIndex].SaveIndex = index;
            _shapes[index] = _shapes[lastIndex];
        }

        _shapes.RemoveAt(lastIndex);
    }
    
    public void MarkAsDying (Shape shape) {
        if (_inGameUpdateLoop) {
            _markAsDyingList.Add(shape);
        }
        else {
            MarkAsDyingImmediately(shape);
        }
    }
    
    void MarkAsDyingImmediately (Shape shape) {
        int index = shape.SaveIndex;
        if (index < _dyingShapeCount) {
            return;
        }
        _shapes[_dyingShapeCount].SaveIndex = index;
        _shapes[index] = _shapes[_dyingShapeCount];
        shape.SaveIndex = _dyingShapeCount;
        _shapes[_dyingShapeCount++] = shape;
    }
    
    public bool IsMarkedAsDying (Shape shape) {
        return shape.SaveIndex < _dyingShapeCount;
    }
    
    private void DestroyShape()
    {
        if (_shapes.Count- _dyingShapeCount > 0)
        {
            var shape = _shapes[Random.Range(_dyingShapeCount, _shapes.Count)];
            if (destroyDuration <= 0f) {
                KillImmediately(shape);
            }
            else {
                shape.AddBehaviour<DyingShapeBehaviour>().Initialize(
                    shape, destroyDuration
                );
            }
        }
    }

    private void NewGame()
    {
        _creationSpeedSlider.value = CreationSpeed = 0;
        _destructionSpeedSlider.value = DestructionSpeed = 0;

        Random.state = _mainRandomState;
        int seed = Random.Range(0, int.MaxValue) ^ (int) Time.unscaledTime;
        _mainRandomState = Random.state;
        Random.InitState(seed);

        foreach (var instance in _shapes)
        {
            instance.Recycle();
        }

        _shapes.Clear();
        _dyingShapeCount = 0;
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

        if (version >= 6)
        {
            GameLevel.Current.Load(reader);
        }

        for (int i = 0; i < count; i++)
        {
            int factoryId = version >= 8 ? reader.ReadInt() : 0;
            var shapeId = version > 0 ? reader.ReadInt() : 0;
            var materialId = version > 1 ? reader.ReadInt() : 0;
            Shape instance = _shapeFactories[factoryId].Get(shapeId, materialId);
            instance.Load(reader);
        }

        foreach (var shape in _shapes)
        {
            shape.ResolveShapeInstances();
        }
    }
}