using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
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

    [Header("Void Stars")]
    public GameObject empty;

    [Header("Cross Constellation")]
    public int numberOfCrossStars = 10;
    public float starMaxDistanceY = 0.75f;


    private readonly List<Constellation> constellations = new();
    private readonly List<Star> stars = new();
    private readonly List<Star> orderedStars = new();
    private readonly List<Tuple<Star, Vector3>> starPositions = new();
    private readonly List<Tuple<Star, Star, GameObject>> lines = new();


    private float nextStart = 0;
    private float nextLifespan = 0;
    private int nextIndex = 0;

    private bool paused = false;

    void Awake() {
        constellations.AddRange(transform.GetComponentsInChildren<Constellation>());
    }

    private void Start() {
        constellations.ForEach(constellation => {
            Color color = starColors[Random.Range(0, starColors.Count)];
            constellation.AddConstellationStars(stars, starPositions);
            constellation.SetColor(color);
        });

        orderedStars.AddRange(
            stars
            .Where(star => { return Mathf.Abs(star.transform.position.y) < starMaxDistanceY; })
            .OrderBy(star => star.transform.position.x));

        for (int i = 0; i < numberOfCrossStars - 1; i++) {
            var line = Instantiate(linePrefab);
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
            lineRenderer.enabled = false;
            lines.Add(new(null, null, line));
        }

        //StartCoroutine(Cluster());
    }

    private IEnumerator Cluster() {
        var waitForSeconds = new WaitForSeconds(0.5f);
        float aspect = Camera.main.aspect;
        float clusterRatio = 5;
        var clusterSize = new Vector2Int(Mathf.CeilToInt(aspect * clusterRatio), Mathf.CeilToInt(clusterRatio / aspect)); // 10 * 1,6, 10 / 1,6
        List<List<Tuple<Star, GameObject>>> clusters = Enumerable.Repeat(new List<Tuple<Star, GameObject>>(), clusterSize.x * clusterSize.y).ToList();
        List<GameObject> roots = new();
        //for (int i = 0; i < clusterSize.x * clusterSize.y; i++) {
        //    Instantiate(empty);
        //    roots.Add(empty);
        //    yield return waitForSeconds;
        //}
        while ( starPositions.Count > 0) {
            Star star = starPositions[0].Item1;
            Vector3 position = starPositions[0].Item2;
            Vector2 viewportPosition = Camera.main.WorldToViewportPoint(position);
            Vector2 clusterPosition = Vector2.Scale(viewportPosition, clusterSize); // ([0,1], [0,1]) * (x, y)

            int index = Mathf.FloorToInt(clusterPosition.y * clusterSize.x) + Mathf.FloorToInt(clusterPosition.y);

            //clusters[index].Add(new(star, Instantiate(empty, position, Quaternion.identity)));
            //yield return waitForSeconds;

            clusters[index].Add(new(star, null));
            yield return null;
        }
        Debug.Break();
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

        if (!paused && Time.time > nextStart) {
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
            constellations.ForEach(constellation => { constellation.Stop(); });
            stars.ForEach(star => { star.Light.enabled = paused; });
            StartCoroutine(CrossContellation(!paused));
            paused = !paused;
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
}
