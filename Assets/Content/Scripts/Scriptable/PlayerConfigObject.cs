using System.IO;
using Content.Scripts.Game.IO;
using Content.Scripts.Game.Services;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Content.Scripts.Scriptable
{
    [CreateAssetMenu(menuName = "Create PlayerConfigObject", fileName = "PlayerConfigObject", order = 0)]
    public class PlayerConfigObject : ScriptableObjectInstaller, IFileInput<PlayerConfigObject>, IFileOutput, IFileSO
    {
        [SerializeField] private float sens = 5;
        [SerializeField] private float fov = 90;
        [SerializeField] private int gameFps = 250;
        [SerializeField] private int menuFps = 30;
        [SerializeField] private string playerName = "Player";
        [SerializeField] private string playerColor = "#FF3100";


        [System.NonSerialized] public UnityEvent OnConfigChanged = new UnityEvent();
        
        public int MenuFps => menuFps;

        public int GameFps => gameFps;

        public float FOV => fov;

        public float Sens => sens;

        public string PlayerName => playerName;

        public string PlayerColor => playerColor;


        public void SetGameFPS(int value)
        {
            gameFps = value;
            OnConfigChanged.Invoke();
        }

        public void SetMenuFPS(int value)
        {
            menuFps = value;
            OnConfigChanged.Invoke();
        }

        public void SetSens(float value)
        {
            sens = value;
            OnConfigChanged.Invoke();
        }

        public void SetFov(int value)
        {
            fov = value;
            OnConfigChanged.Invoke();
        }

        public void SetNickname(string value)
        {
            playerName = value;
            OnConfigChanged.Invoke();
        }

        public void SetColor(string value)
        {
            playerColor = value;
            OnConfigChanged.Invoke();
        }


        public override void InstallBindings()
        {
            PathService.CreateFolders();
            Container.Bind<PlayerConfigObject>().FromInstance(this).AsSingle().NonLazy();
            Load();
        }


        [Button]
        public void Load()
        {
            LoadData(PathService.PlayerConfigPath);
        }

        [Button]
        public void Save()
        {
            SaveData(PathService.PlayerConfigPath);
        }

        [Button]
        public void Delete()
        {
            if (File.Exists(PathService.PlayerConfigPath))
            {
                File.Delete(PathService.PlayerConfigPath);
            }

            Load();
        }

        public PlayerConfigObject LoadData(string path)
        {
            if (File.Exists(path))
            {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(path), this);
            }
            else
            {
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(CreateInstance<PlayerConfigObject>()), this);
                Save();
            }

            return this;
        }

        public void SaveData(string path)
        {
            var save = JsonUtility.ToJson(this);
            File.WriteAllText(path, save);
        }


    }
}
