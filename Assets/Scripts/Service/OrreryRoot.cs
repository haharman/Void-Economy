using System;
using System.Collections.Generic;
using UnityEngine;
using Model;
using Model.Economy;
using Presenter;
using View;
using Core;
using Service;
using Yarn.Unity;

public class OrreryRoot : MonoBehaviour
{
    private PlanetRoot _planetRoot;
    
    // Service
    private UpdateService _orreryUpdateService;
    
    // Model
    private QuestModel _questModel;
    private EconomyEngine _economyEngine;
    private EntityModel _entityModel;
    private JetpackModel _jetpackModel;
    private InventoryModel _inventoryModel;
    private OrreryModel _orreryModel;
    
    // View
    [SerializeField] private YarnView yarnView;
    [SerializeField] private PlayerView playerView;
    [SerializeField] private InteractionGuideView interactionGuideView;
    
    // Presenter
    private AudioPresenter _audioPresenter;
    private PlayerPresenter _playerPresenter;
    private OrreryPresenter _orreryPresenter;
    private DialoguePresenter _dialoguePresenter;
    
    // Scriptable Objects
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private InMemoryVariableStorage yarnVariableStorage;
    [SerializeField] private OrrerySettingsSo orrerySettingsSo;
    
    // Planet Kvp
    [Serializable]
    public class PlanetKvp
    {
        public CoordSystemId PlanetId;
        public PlanetRoot planetRoot;
        public Transform coordSystemTransform;
    }
    [SerializeField] private List<PlanetKvp> planetKvpList;
    
    private IReadOnlyDictionary<CoordSystemId, PlanetRoot> _planetRootDictionary;
    private IUpdateService _systemUpdateService;
    private ISaveDataService _saveDataService;


    public void Bootstrap(AudioView audioView, InputService inputService, ISaveDataService saveDataService)
    {
        _saveDataService = saveDataService;
        _orreryUpdateService = new UpdateService();
        
        var planetRootDictionary = new Dictionary<CoordSystemId, PlanetRoot>();
        foreach (var planetKvp in planetKvpList)
        {
            planetRootDictionary[planetKvp.PlanetId] = planetKvp.planetRoot;
        }
        _planetRootDictionary = planetRootDictionary;
        
        // Model
        _economyEngine = new EconomyEngine();
        _dialoguePresenter = new DialoguePresenter(dialogueRunner ,yarnVariableStorage, inputService);
        _entityModel = new EntityModel(_dialoguePresenter);
        _inventoryModel = new InventoryModel();
        _orreryModel = new OrreryModel(orrerySettingsSo);
        _jetpackModel = new JetpackModel(_entityModel, _orreryModel);
        _questModel = new QuestModel();
        
        // Presenter
        _audioPresenter = new AudioPresenter(audioView, _jetpackModel, this.destroyCancellationToken);
        
        var coordSystemTransformDictionary = new Dictionary<CoordSystemId, Transform>();
        foreach (var planetKvp in planetKvpList)
        {
            if (planetKvp.planetRoot != null)
                coordSystemTransformDictionary.Add(planetKvp.PlanetId, planetKvp.coordSystemTransform);
            else
                Debug.LogError("[OrreryRoot] PlanetRoot is null for ID: " + planetKvp.PlanetId);
        }
        _playerPresenter = new PlayerPresenter(_jetpackModel, playerView, inputService,
            _audioPresenter, this.destroyCancellationToken, coordSystemTransformDictionary);
        
        _orreryPresenter = new OrreryPresenter(_orreryModel);
        
        _saveDataService.RegisterReader(_orreryPresenter);
        _saveDataService.RegisterWriter(_orreryPresenter);
        _saveDataService.RegisterReader(_playerPresenter);
        _saveDataService.RegisterWriter(_playerPresenter);
        
        foreach (var (id, planetRoot) in _planetRootDictionary)
        {
            if (planetRoot == null)
            {
                Debug.LogError($"PlanetRoot {id} is null");
                continue;
            }
            planetRoot.Bootstrap(id, _orreryModel.GetPlanetModel(id),
                _jetpackModel, saveDataService);
        }
    }

    // Bootstrap と Initialize の間に必ずLoadが入る
    
    public void Initialize(IUpdateService systemUpdateService)
    {
        _systemUpdateService = systemUpdateService;
        
        _orreryUpdateService.Register(_playerPresenter);
        _orreryUpdateService.Register(_orreryPresenter);
        _orreryUpdateService.Register(_audioPresenter);
        
        foreach (var (id, planetRoot) in _planetRootDictionary)
        {
            if (planetRoot == null)
            {
                Debug.LogError($"PlanetRoot {id} is null");
                continue;
            }
            planetRoot.Initialize(_orreryUpdateService);
        }
        
        _systemUpdateService.Register(_orreryUpdateService);
    }

    public void Terminate()
    {
        _systemUpdateService.Unregister(_orreryUpdateService);
        _saveDataService.UnregisterReader(_orreryPresenter);
        _saveDataService.UnregisterWriter(_orreryPresenter);
        _saveDataService.UnregisterReader(_playerPresenter);
        _saveDataService.UnregisterWriter(_playerPresenter);
        foreach (var planetRoot in _planetRootDictionary.Values)
        {
            planetRoot.Terminate();
        }
    }
}