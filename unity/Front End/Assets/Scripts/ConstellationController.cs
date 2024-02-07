using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.VolumeComponent;
using Random = UnityEngine.Random;


public class ConstellationController : MonoBehaviour
{
    [Header("Constellation Properties")]
    public MinMax constDelay;
    public MinMax constLifespan;

    [Header("Line Properties")]
    public float lineOffset = 0.25f;
    public float lineVelocity = 5;
    public GameObject linePrefab;

    [Header("Star Properties")]
    public GameObject starPrefab;
    public float starShineDelay = 1;
    public float starRange = 3;
    public MinMax starIntensity;
    public MinMax starSize;
    public List<Color> starColors;
    public float colorProbability = 0.25f;

    [Header("Cross Constellation")]
    public int numberOfCrossStars = 10;
    public float starMaxDistanceY = 0.75f;


    private readonly List<Constellation> constellations = new();
    private readonly List<Star> stars = new();
    private readonly List<Star> orderedStars = new();
    private readonly List<Tuple<Star, Star, GameObject>> lines = new();


    private float nextStart = 0;
    private float nextLifespan = 0;
    private int nextIndex = 0;

    private bool running = false;

    void Awake() {
        constellations.AddRange(transform.GetComponentsInChildren<Constellation>());
        changeState = false;
    }

    private void Start() {
        constellations.ForEach(constellation => {
            constellation.AddConstellationStars(stars);
            if (Random.value < colorProbability) {
                Color color = starColors[Random.Range(0, starColors.Count)];
                constellation.SetColor(color);
            }
        });

        orderedStars.AddRange(
            stars
            .Where(star => { return Mathf.Abs(star.transform.position.y) < starMaxDistanceY; })
            .OrderBy(star => star.transform.position.x));

        var crossConstellation = Instantiate(empty, Vector3.zero, Quaternion.identity);
        crossConstellation.name = "Cross Constellation";

        for (int i = 0; i < numberOfCrossStars - 1; i++) {
            var line = Instantiate(linePrefab);
            line.transform.parent = crossConstellation.transform;
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
            lineRenderer.enabled = false;
            lines.Add(new(null, null, line));
        }

        clusters = new List<Star>[startSize.x, startSize.y];
        //roots = new GameObject[startSize.x, startSize.y];
        var parent = Instantiate(empty, Vector3.zero, Quaternion.identity);
        parent.name = "Clusters";
        Vector3 nClusters = new(startSize.x, startSize.y, 1);
        Vector3 start = new(0, 0, 1);
        Vector3 size = new(1, 1, 1);
        StartCoroutine(Cluster(stars, clusters, start, nClusters, size, 0));
    }

    [Header("Clustering")]
    public GameObject empty;
    public Transform randomStarsParent;
    public Vector2Int startSize = new Vector2Int(8, 4);
    public float clusterDivision = 2f;
    public int minStars = 3;
    public int jump = 1;
    public float clusterStarProbability = 0.5f;
    public float clusterStarColorProbability = 0.1f;
    private List<Star>[,] clusters;
    //private GameObject[,] roots;

    private IEnumerator Cluster(List<Star> stars, List<Star>[,] clusters, Vector3 start, Vector3 nClusters, Vector3 size, int jump) {

        var clusterSize = new Vector3(size.x / nClusters.x, size.y / nClusters.y, size.z / nClusters.z);
        var inverted = new Vector3(nClusters.x / size.x, nClusters.y / size.y, nClusters.z / size.z);

        if (nClusters.x < 2 || nClusters.y < 2) {
            if (Random.value > clusterStarProbability)
                yield break;
            var viewportPosition = new Vector3(
                Random.Range(start.x, start.x + clusterSize.x),
                Random.Range(start.y, start.y + clusterSize.y),
                0f);
            var position = Camera.main.ViewportToWorldPoint(viewportPosition);
            position.z = starPrefab.transform.localPosition.z;

            var star = Instantiate(starPrefab, position, Quaternion.identity);
            star.transform.parent = randomStarsParent;

            var starComponent = star.GetComponent<Star>();
            starComponent.Light.enabled= false;
            starComponent.Create(this);
            if (Random.value < clusterStarColorProbability) {
                Color color = starColors[Random.Range(0, starColors.Count)];
                starComponent.SetColor(color);
            }

            this.stars.Add(starComponent);
            yield break;
        }


        for (int i = 0; i < nClusters.x; i++) {
            for (int j = 0; j < nClusters.y; j++) {
                var cluster = new Vector3(i, j, 1);
                var viewportPosition = start + Vector3.Scale(cluster, clusterSize);
                var position = Camera.main.ViewportToWorldPoint(viewportPosition);
                //var root = Instantiate(empty, position, Quaternion.identity);
                //root.transform.parent = parent.transform;
                //root.name = $"X {i}, Y {j}";
                //roots[i, j] = root;
            }
        }

        for (int i = 0; i < stars.Count; i++) {
            var star = stars[i];
            //var constellation = star.transform.parent.gameObject;
            var position = stars[i].transform.position;

            var viewportPosition = Camera.main.WorldToViewportPoint(position);
            var clusterPosition = Vector3.Scale(viewportPosition - start, inverted);
            var index = new Vector2Int(Mathf.FloorToInt(clusterPosition.x), Mathf.FloorToInt(clusterPosition.y));

            if (index.x < 0
                || index.y < 0
                || index.x >= nClusters.x
                || index.y >= nClusters.y) {
                continue;
            }

            //var emptyPosition = Instantiate(empty, position, Quaternion.identity);
            //emptyPosition.name = $"{constellation.name} {star.name}";
            //emptyPosition.transform.parent = roots[index.x, index.y].transform;

            if (clusters[index.x, index.y] == null)
                clusters[index.x, index.y] = new();

            clusters[index.x, index.y].Add(star);
            //roots[index.x, index.y].name = $"X {index.x}, Y {index.y}: {roots[index.x, index.y].transform.childCount}";
        }

        for (int i = 0; i < nClusters.x; i++) {
            for (int j = 0; j < nClusters.y; j++) {
                var cluster = clusters[i, j];
                if (cluster == null)
                    cluster = new();

                if (cluster.Count < minStars - this.jump * jump) {
                    var x = Mathf.Max(Mathf.FloorToInt(nClusters.x / clusterDivision), 1);
                    var y = Mathf.Max(Mathf.FloorToInt(nClusters.y / clusterDivision), 1);

                    var subClusters = new List<Star>[x, y];
                    //var subRoots = new GameObject[x, y];

                    //var clusterStars = cluster.Select(a => a.Item1).ToList();
                    var clusterPosition = new Vector3(i, j, 1);
                    var viewportPosition = start + Vector3.Scale(clusterPosition, clusterSize);
                    yield return Cluster(stars, subClusters, viewportPosition, new Vector3(x, y, 1), clusterSize, jump + 1);
                }
            }
        }
    }

    IEnumerator CrossContellation(bool start) {
        int lastIndex = 0;
        for (int i = 0; i < numberOfCrossStars - 1; i++) {
            float startTime = Time.time;

            GameObject line = lines[i].Item3;
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();

            int randEnd = Random.Range(0, 5);
            int indexEnd = Math.Min((i + 1) * orderedStars.Count / numberOfCrossStars + randEnd, orderedStars.Count - 1);
            Star startStar = start ? orderedStars[lastIndex] : lines[i].Item1;
            Star endStar = start ? orderedStars[indexEnd] : lines[i].Item2;
            lastIndex = indexEnd;

            if (start)
                lines[i] = new(startStar, endStar, line);

            Vector3 direction = endStar.transform.position - startStar.transform.position;
            float distance = direction.magnitude;
            direction.Normalize();

            Vector3 offset = direction * lineOffset;
            Vector3 startPosition = startStar.transform.position + offset * startStar.transform.localScale.magnitude;
            Vector3 endPosition = endStar.transform.position - offset * endStar.transform.localScale.magnitude;


            float deltaTime = distance / lineVelocity;

            if (start)
                lineRenderer.enabled = true;
            startStar.Light.enabled = true;
            StartCoroutine(startStar.Blink(this, start));
            while (Time.time < startTime + deltaTime) {
                float t = Time.time - startTime;
                if (start)
                    lineRenderer.SetPositions(new[] { startPosition, Vector3.Lerp(startPosition, endPosition, t / deltaTime) });
                else
                    lineRenderer.SetPositions(new[] { Vector3.Lerp(startPosition, endPosition, t / deltaTime), endPosition });
                yield return null;
            }

            if (!start)
                lineRenderer.enabled = false;
        }
        StartCoroutine(orderedStars[lastIndex].Blink(this, start));
    }
    void Update() {

        if (changeState) {
            constellations.ForEach(constellation => { constellation.Stop(); });
            stars.ForEach(star => { star.Light.enabled = running; });
            changeState = false;
        }

        if (running && Time.time > nextStart) {
            if (nextIndex > constellations.Count)
                return;

            Constellation next = constellations[nextIndex];
            nextLifespan = Random.Range(constLifespan.min, constLifespan.max);
            nextStart = Time.time + Random.Range(constDelay.min, constDelay.max);

            next.Queue(nextLifespan);

            nextIndex++;
            if (nextIndex + 1 >= constellations.Count) {
                Shuffle(constellations);
                nextIndex = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            SetState(!running);
        }
    }
    public void Shuffle<T>(IList<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private bool changeState = false;

    public void SetState(bool running) {
        changeState = true;
        this.running = running;
        Debug.Log(running);
    }
}
