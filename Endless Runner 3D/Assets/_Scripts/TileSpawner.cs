using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TempleRun
{
    public class TileSpawner : MonoBehaviour
    {
        [SerializeField] private int tileStartCount = 10;
        [SerializeField] private int minimumStraightTileCount = 3;
        [SerializeField] private int maximumStraightTileCount = 15;
        
        [SerializeField] private GameObject startingTile;
        [SerializeField] private List<GameObject> turnTiles;
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
            
            // Make sures that random number is different and unique every time
            Random.InitState(System.DateTime.Now.Millisecond);

            // Spawn starting straight tiles
            for (var i = 0; i < tileStartCount; i++)
            {   
                SpawnTile(startingTile.GetComponent<Tile>());
            }
            
            SpawnTile(SelectRandomGameObjectFromList(turnTiles).GetComponent<Tile>());
        }

        /// <summary>
        /// Calculates the next tile location, direction and spawns the tile of the given type.
        /// </summary>
        /// <param name="tile">Type of tile to spawn.</param>
        /// <param name="spawnObstacle">Should a obstacle be instantiate or not.</param>
        private void SpawnTile(Tile tile, bool spawnObstacle = false)
        {
            var newTileRotation = tile.gameObject.transform.rotation * Quaternion.LookRotation(_currentTileDirection, Vector3.up);
            
            _prevTile = Instantiate(tile.gameObject, _currentTileLocation, newTileRotation);
            _currentTiles.Add(_prevTile);

            if (spawnObstacle) SpawnObstacle();
            
            if (tile.type == TileType.STRAIGHT)
                _currentTileLocation += Vector3.Scale(_prevTile.GetComponent<Renderer>().bounds.size, _currentTileDirection);
        }
        
        public void AddNewDirection(Vector3 newDirection)
        {
            _currentTileDirection = newDirection;
            DeletePreviousTiles();

            // Calculate the next tile location.
            Vector3 tilePlacementScale;
            if (_prevTile.GetComponent<Tile>().type == TileType.SIDEWAYS)
            {
                tilePlacementScale = Vector3.Scale(
                    (_prevTile.GetComponent<Renderer>().bounds.size / 2) +
                    (Vector3.one * startingTile.GetComponent<BoxCollider>().size.z / 2),
                    _currentTileDirection
                );
            }
            else
            {
                // Left or right tiles.
                tilePlacementScale = Vector3.Scale(
                    (_prevTile.GetComponent<Renderer>().bounds.size - (Vector3.one * 2)) +
                    (Vector3.one * startingTile.GetComponent<BoxCollider>().size.z / 2),
                    _currentTileDirection
                );
            }
            
            _currentTileLocation += tilePlacementScale;
            
            var currentPathLength = Random.Range(minimumStraightTileCount, maximumStraightTileCount);
            for (var i = 0; i < currentPathLength; i++)
            {
                SpawnTile(startingTile.GetComponent<Tile>(), (i == 0)? false : true);
            }
            
            SpawnTile(SelectRandomGameObjectFromList(turnTiles).GetComponent<Tile>());
        }

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

        private void SpawnObstacle()
        {
            if (Random.value > 0.2f) return;
            
            var obstaclePrefab = SelectRandomGameObjectFromList(obstacles);
            var newObstacleRotation = obstaclePrefab.transform.rotation * Quaternion.LookRotation(_currentTileDirection, Vector3.up);
            var obstacle = Instantiate(obstaclePrefab, _currentTileLocation, newObstacleRotation);
            _currentObstacles.Add(obstacle);
        }

        /// <summary>
        /// Randomly selects a game object from a list.
        /// </summary>
        /// <param name="list">List of the game objects.</param>
        /// <returns>Selected game object.</returns>
        private GameObject SelectRandomGameObjectFromList(List<GameObject> list)
        {
            return list.Count == 0 ? null : list[Random.Range(0, list.Count)];
        }
    }

}
