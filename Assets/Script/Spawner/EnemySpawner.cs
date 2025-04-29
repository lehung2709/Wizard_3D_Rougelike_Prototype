using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    private int currentEnemyCount = 0;
    private bool IsSpawning=true ;


    [SerializeField] private PlayerStats player;
    private NavMeshTriangulation triangulation;
    [SerializeField] private EnemySO[] enemySOs;
    [SerializeField] private int maxDifficultyPoint=2;
    [SerializeField] private int currentDifficultyPoint=1;
    [SerializeField] private int difficultyPointIncreaseRate=2;
    [SerializeField] private float spawnInterval;
    [SerializeField] private float spawnIntervalIncreaseRate=1;
    [SerializeField] private EnemySO defaultEnemySO;
    [SerializeField] private SpawnData spawnData;
    private Dictionary<string, Queue<Enemy>> EnemyPoolDictionary = new Dictionary<string, Queue<Enemy>>();


    private void Start()
    {
        enemySOs=spawnData.enemySOs;
        maxDifficultyPoint=spawnData.maxDifficultyPoint;
        spawnInterval=spawnData.spawnInterval;
        spawnIntervalIncreaseRate=spawnData.spawnIntervalIncreaseRate;
        difficultyPointIncreaseRate=spawnData.difficultyPointIncreaseRate;
        triangulation = NavMesh.CalculateTriangulation();
        defaultEnemySO=enemySOs[0];
        foreach (var enemySO in enemySOs)
        {
            if (enemySO.difficultyPoint < defaultEnemySO.difficultyPoint) defaultEnemySO = enemySO;
        }
        currentDifficultyPoint = defaultEnemySO.difficultyPoint;
        StartCoroutine(SpawnEnemyCoroutine());
    }

    private void FixedUpdate()
    {
        if(currentEnemyCount<=0 && !IsSpawning)
        {
            LevelManager.Instance.Win();
        }    
    }
    public Enemy Get(EnemySO enemySO)
    {
        string prefabKey = enemySO.name;

        if (!EnemyPoolDictionary.ContainsKey(prefabKey))
        {
            EnemyPoolDictionary[prefabKey] = new Queue<Enemy>();
        }

        if (EnemyPoolDictionary[prefabKey].Count > 0)
        {
            Enemy obj = EnemyPoolDictionary[prefabKey].Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }

        Enemy newObj = Object.Instantiate(enemySO.prefab);
        newObj.GetComponent<EnemyStats>().SetEnemyStatsSO(enemySO.stats);
        newObj.Key = prefabKey;
        newObj.Spawner=this;
        return newObj;
    }
    public void Return(string key, Enemy obj)
    {

        if (EnemyPoolDictionary.ContainsKey(key))
        {
            obj.gameObject.SetActive(false);

            EnemyPoolDictionary[key].Enqueue(obj);

            currentEnemyCount--;
        }

    }
    private void SpawnEnemy()
    {
        int VertexIndex;

        NavMeshHit Hit;

        EnemySO enemySO;

        int remainingDifficulty = currentDifficultyPoint;

        while (remainingDifficulty > 0)
        {
            if (remainingDifficulty <= defaultEnemySO.difficultyPoint)
            {
                enemySO = defaultEnemySO;
            }
            else
            {
                enemySO = enemySOs[Random.Range(0, enemySOs.Length)];
                if (enemySO.difficultyPoint > remainingDifficulty) continue;
            }

            do
            {
                VertexIndex = Random.Range(0, triangulation.vertices.Length);

            } while (!NavMesh.SamplePosition(triangulation.vertices[VertexIndex], out Hit, 2f, NavMesh.AllAreas));
            

            
            remainingDifficulty -= enemySO.difficultyPoint;

            Enemy enemy = Get(enemySO);

            enemy.SetPlayerAndPos(player, Hit.position);

            currentEnemyCount++;


        }

    }
    private IEnumerator SpawnEnemyCoroutine()
    {
        while (true)
        {
            
            yield return new WaitForSeconds(spawnInterval);
            SpawnEnemy();
            currentDifficultyPoint += difficultyPointIncreaseRate;
            if (currentDifficultyPoint > maxDifficultyPoint)
            {
                IsSpawning = false;
                break;
            }
            spawnInterval += spawnIntervalIncreaseRate;
            
        } 
            
    }    
}
