using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterSpawnManager : MonoBehaviour
{
    [Serializable]
    public class SpawnEntry
    {
        public string label;            // 에디터 표시용
        public GameObject prefab;          // 몬스터 프리팹
        public Collider spawnArea;        // 스폰 영역 Collider 
        public Collider roamArea;         // 로밍 영역 Collider 
        public int maxCount = 10;    // 최대 마릿수
        public float spawnInterval = 10f; // 스폰 주기 
        [HideInInspector]
        public List<GameObject> spawned = new List<GameObject>();
    }

    [Header("몬스터 스폰 설정")]
    [SerializeField] private List<SpawnEntry> spawnList;

    [Header("유효성 검사")]
    [Tooltip("NavMesh.SamplePosition 반경")]
    [SerializeField] private float sampleRange = 2f;
    [Tooltip("스폰 위치 주변 장애물 체크 반경")]
    [SerializeField] private float obstacleCheckRadius = 0.5f;
    [Tooltip("장애물 레이어 마스크")]
    [SerializeField] private LayerMask obstacleMask;

    private void Start()
    {
        foreach (var e in spawnList)
        {
            SpawnUpToMax(e);
            StartCoroutine(SpawnRoutine(e));
        }
    }

    private IEnumerator SpawnRoutine(SpawnEntry e)
    {
        while (true)
        {
            yield return new WaitForSeconds(e.spawnInterval);
            e.spawned.RemoveAll(o => o == null);
            SpawnUpToMax(e);
        }
    }

    private void SpawnUpToMax(SpawnEntry e)
    {
        int need = e.maxCount - e.spawned.Count;
        for (int i = 0; i < need; i++)
        {
            if (!TryGetValidSpawnPoint(e.spawnArea, out Vector3 pos))
            {
                break;
            }
            var m = Instantiate(e.prefab, pos, Quaternion.identity);
            e.spawned.Add(m);
            if (m.TryGetComponent<MonsterAI>(out var ai))
                ai.roamArea = e.roamArea;
        }
    }

    private bool TryGetValidSpawnPoint(Collider area, out Vector3 result, int maxAttempts = 10)
    {
        Bounds b = area.bounds;
        for (int i = 0; i < maxAttempts; i++)
        {
            // X/Z 랜덤
            float x = UnityEngine.Random.Range(b.min.x, b.max.x);
            float z = UnityEngine.Random.Range(b.min.z, b.max.z);

            // Y 높이 결정
            float y;
            var terrain = area.GetComponent<Terrain>();
            if (terrain != null)
            {
                y = terrain.SampleHeight(new Vector3(x, 0, z))
                  + terrain.transform.position.y;
            }
            else
            {
                // 지면 높이 구하기
                RaycastHit hit;
                Vector3 top = new Vector3(x, b.max.y + 2f, z);
                if (!Physics.Raycast(top, Vector3.down, out hit,
                                     b.size.y + 4f,
                                     1 << area.gameObject.layer))
                    continue;
                y = hit.point.y;
            }

            Vector3 cand = new Vector3(x, y, z);

            // NavMesh 상 유효 지점으로 보정
            if (!NavMesh.SamplePosition(cand, out var navHit, sampleRange, NavMesh.AllAreas))
                continue;

            // 장애물 겹침 검사
            if (Physics.CheckSphere(navHit.position, obstacleCheckRadius, obstacleMask))
                continue;

            result = navHit.position;
            return true;
        }

        result = default;
        return false;
    }
}
