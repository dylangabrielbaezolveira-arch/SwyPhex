using UnityEngine;

namespace SwyPhexLeague.ScriptableObjects.Maps
{
    [CreateAssetMenu(fileName = "MapData", menuName = "SwyPhex/Map Data")]
    public class MapData : ScriptableObject
    {
        [Header("Map Information")]
        public string mapId = "NeonDocks";
        public string displayName = "Neon Docks";
        public string description = "Classic arena with normal gravity";
        public Sprite previewImage;
        public SceneReference scene;
        
        [Header("Gameplay Settings")]
        public Core.GravityManager.GravityType gravityType = Core.GravityManager.GravityType.Normal;
        public bool useVariableGravity = false;
        public float gravityChangeInterval = 30f;
        public Core.GravityManager.GravityType[] gravitySequence;
        
        [Header("Map Size")]
        public Vector2 mapSize = new Vector2(24, 16);
        public Vector2 ballSpawn = Vector2.zero;
        public Vector2[] team1Spawns;
        public Vector2[] team2Spawns;
        
        [Header("Environment")]
        public GameObject[] obstacles;
        public Vector2[] obstaclePositions;
        public GameObject[] boostOrbs;
        public Vector2[] boostOrbPositions;
        public float boostOrbRespawnTime = 10f;
        
        [Header("Visuals")]
        public Material skybox;
        public Color ambientLight = Color.white;
        public GameObject[] decorativeElements;
        public AudioClip backgroundMusic;
        public AudioClip ambientSound;
        
        [Header("Game Modes")]
        public bool availableInRanked = true;
        public bool availableInCasual = true;
        public int maxPlayers = 4;
        
        public bool IsAvailableForMode(GameMode mode)
        {
            return mode switch
            {
                GameMode.Ranked => availableInRanked,
                GameMode.Casual => availableInCasual,
                GameMode.Training => true,
                _ => true
            };
        }
        
        public Vector2 GetRandomSpawnPoint(int team)
        {
            Vector2[] spawns = team == 0 ? team1Spawns : team2Spawns;
            if (spawns.Length == 0) return Vector2.zero;
            
            return spawns[Random.Range(0, spawns.Length)];
        }
        
        public enum GameMode
        {
            Ranked,
            Casual,
            Training,
            Tournament
        }
        
        [System.Serializable]
        public class SceneReference
        {
            public string sceneName;
            public int buildIndex;
            
            #if UNITY_EDITOR
            public UnityEditor.SceneAsset sceneAsset;
            
            private void OnValidate()
            {
                if (sceneAsset != null)
                {
                    sceneName = sceneAsset.name;
                }
            }
            #endif
        }
    }
}
