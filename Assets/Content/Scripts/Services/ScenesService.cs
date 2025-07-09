using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Content.Scripts.Services
{
    public class ScenesService : MonoBehaviour
    {
        public enum EScene
        {
            Boot,
            Menu,
            Game
        }


        public void LoadScene(EScene scene)
        {
            SceneManager.LoadScene(scene.ToString());
        }

        public EScene GetActiveScene()
        {
            var names = Enum.GetNames(typeof(EScene));
            var name = SceneManager.GetActiveScene().name;
            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] == name)
                {
                    return (EScene)i;
                }
            }

            return 0;
        }
    }
}
