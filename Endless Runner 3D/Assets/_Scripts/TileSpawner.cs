using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TempleRun
{
    public class TileSpawner : MonoBehaviour
    {
        [SerializeField] private int tileStartCount = 10;
        [SerializeField] private int minimumStraightTiles = 3;
        [SerializeField] private int maximumStraightTiles = 15;
        [SerializeField] private GameObject staringTile;
        [SerializeField] private List<GameObject> turnTile;
        [SerializeField] private List<GameObject> obstacles;

        private Vector3 _currentTileLocation = Vector3.zero;
        private Vector3 _currentTileDirection = Vector3.forward;
        private GameObject _prevTile;

        private List<GameObject> _currentTiles;
        private List<GameObject> _currentObstacles;


        private void Start()
        {
            _currentTiles = new List<GameObject>();
            _currentObstacles = new List<GameObject>();

            // Make sure to generate random values always. 
            Random.InitState(System.DateTime.Now.Millisecond);

            for (var i = 0; i < tileStartCount; i++)
            {
                SpawnTile(staringTile.GetComponent<Tile>());
            }
            
            SpawnTile(SelectRandomGameObjectFromList(turnTile).GetComponent<Tile>());
            
        }

        /// <summary>
        /// Spawn the given tile at the location and direction also calculate location and direction for next tile to spawn.
        /// </summary>
        /// <param name="tile">Type of tile to spawn.</param>
        /// <param name="spawnObstacle">If obstacle will spawn or not.</param>
        private void SpawnTile(Tile tile, bool spawnObstacle = false)
        {
            var newTileRotation = tile.gameObject.transform.rotation * Quaternion.LookRotation(_currentTileDirection, Vector3.up);
            
            _prevTile = GameObject.Instantiate(tile.gameObject, _currentTileLocation, newTileRotation);
            _currentTiles.Add(_prevTile);

            if (spawnObstacle)
            {
                SpawnObstacle();
            }

            if (tile.type == TileType.STRAIGHT)
            {
                _currentTileLocation += Vector3.Scale(_prevTile.GetComponent<Renderer>().bounds.size, _currentTileDirection);
            }
        }

        private void SpawnObstacle()
        {
            if (Random.value > 0.2f) return;

            var obstaclePrefab = SelectRandomGameObjectFromList(obstacles);
            var newObjectRotation = obstaclePrefab.gameObject.transform.rotation * Quaternion.LookRotation(_currentTileDirection, Vector3.up);
            
            var obstacle = Instantiate(obstaclePrefab, _currentTileLocation, newObjectRotation);
            _currentObstacles.Add(obstacle);
        }

        /// <summary>
        /// Select a random tile from the list of tiles.
        /// </summary>
        /// <param name="list">The list to select from.</param>
        /// <returns></returns>
        private GameObject SelectRandomGameObjectFromList(List<GameObject> list)
        {
            if (list.Count == 0) return null;
            
            return list[Random.Range(0, list.Count)];
        }

        public void AddNewDirection(Vector3 direction)
        {
            _currentTileDirection = direction;
            DeletePreviousTiles();

            Vector3 tilePlacementScale;
            if (_prevTile.GetComponent<Tile>().type == TileType.SIDEWAYS)
            {
                tilePlacementScale = Vector3.Scale((_prevTile.GetComponent<Renderer>().bounds.size / 2) + (Vector3.one * staringTile.GetComponent<BoxCollider>().size.z / 2), _currentTileDirection);
            }
            else
            {
                // Left or right tiles
                tilePlacementScale = Vector3.Scale((_prevTile.GetComponent<Renderer>().bounds.size - (2 * Vector3.one)) + (Vector3.one * staringTile.GetComponent<BoxCollider>().size.z / 2), _currentTileDirection);
            }
            
            _currentTileLocation += tilePlacementScale;

            var currentPathLength = Random.Range(minimumStraightTiles, maximumStraightTiles);
            for (var i = 0; i < currentPathLength; i++)
            {
                SpawnTile(staringTile.GetComponent<Tile>(), (i == 0)? false: true);
            }
            
            SpawnTile(SelectRandomGameObjectFromList(turnTile).GetComponent<Tile>());
        }

        /// <summary>
        /// Deletes previous tiles from list after turning. 
        /// </summary>
        private void DeletePreviousTiles()
        {
            while (_currentTiles.Count != 1)
            {
                var tile = _currentTiles[0];
                _currentTiles.RemoveAt(0);
                Destroy(tile);
            }
            
            while (_currentObstacles.Count != 0)
            {
                var obstacle = _currentObstacles[0];
                _currentObstacles.RemoveAt(0);
                Destroy(obstacle);
            }
        }
    }

}
