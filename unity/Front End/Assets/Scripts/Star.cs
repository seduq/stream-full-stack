using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Line
{
    public Star star;
    public LineRenderer renderer;
    public bool running;
    public Line(Star star, LineRenderer renderer) {
        this.star = star;
        this.renderer = renderer;
        running = false;
    }
}
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Light))]
public class Star : MonoBehaviour
{
    [Header("Star")]
    public List<Star> nextStars = new();

    private Light _light;
    private SpriteRenderer sprite;

    private bool shining = false;
    private bool created = false;

    private readonly List<Line> lines = new();

    public Light Light { get => _light; }
    public SpriteRenderer Sprite { get => sprite; }

    private void Awake() {
        _light = GetComponent<Light>();
        sprite = GetComponent<SpriteRenderer>();
    }

    public void Create(ConstellationController controller) {
        if (created) { return; }
        created = true;
        transform.localScale = Vector3.one * Random.Range(controller.starSize.min, controller.starSize.max);
        Light.intensity = controller.starIntensity.min;
        Light.range = controller.starRange;
        foreach (var star in nextStars) {
            GameObject line = Instantiate(controller.linePrefab);
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
            line.transform.SetParent(transform);

            lines.Add(new(star, lineRenderer));

            lineRenderer.SetPositions(new[] { transform.position, transform.position });
            lineRenderer.enabled = false;
            star.Create(controller);
        }
    }

    public void Play(ConstellationController controller) {
        if (shining) return;
        shining = true;
        StartCoroutine(Blink(controller, true));
        foreach (var line in lines) {
            LineRenderer lineRenderer = line.renderer;
            lineRenderer.enabled = true;
            line.running = true;
            StartCoroutine(LineMove(controller, line, true));
        }
    }

    public void Stop(ConstellationController controller) {
        if (!shining) return;
        shining = false;

        StartCoroutine(Blink(controller, false));
        foreach (var line in lines) {
            line.running = true;
            StartCoroutine(LineMove(controller, line, false));
        }
    }
    public IEnumerator Blink(ConstellationController controller, bool start) {
        float startTime = Time.time;
        float deltaTime = controller.starShineDelay;

        while (Time.time < startTime + deltaTime) {
            float t = Time.time - startTime;
            if (start)
                Light.intensity = Mathf.Lerp(controller.starIntensity.min, controller.starIntensity.max, t / deltaTime);
            else
                Light.intensity = Mathf.Lerp(controller.starIntensity.max, controller.starIntensity.min, t / deltaTime);
            yield return null;
        }

        if (start)
            Light.intensity = controller.starIntensity.max;
        else
            Light.intensity = controller.starIntensity.min;

        yield return null;
    }

    public IEnumerator LineMove(ConstellationController controller, Line line, bool start) {
        Star star = line.star;
        LineRenderer lineRenderer = line.renderer;

        float startTime = Time.time;

        Vector3 direction = star.transform.position - transform.position;
        float distance = direction.magnitude;
        direction.Normalize();

        Vector3 offset = direction * controller.lineOffset;
        Vector3 startPosition = transform.position + offset * transform.localScale.magnitude;
        Vector3 endPosition = star.transform.position - offset * star.transform.localScale.magnitude;

        float deltaTime = distance / controller.lineVelocity;

        while (Time.time < startTime + deltaTime) {
            float t = Time.time - startTime;
            if (start)
                lineRenderer.SetPositions(new[] { startPosition, Vector3.Lerp(startPosition, endPosition, t / deltaTime) });
            else
                lineRenderer.SetPositions(new[] { Vector3.Lerp(startPosition, endPosition, t / deltaTime), endPosition });
            yield return null;
        }

        if (start)
            lineRenderer.SetPositions(new[] { startPosition, endPosition });
        else
            lineRenderer.enabled = false;

        foreach (var nextStar in nextStars) {
            if (start)
                nextStar.Play(controller);
            else
                nextStar.Stop(controller);
        }

        line.running = false;
    }


    public bool IsCompleted() {
        return lines.TrueForAll(line => line.running == false);
    }

    public void GetAllStars(List<Star> stars) {
        if(!stars.Contains(this))
            stars.Add(this);
        foreach (var star in nextStars) {
            star.GetAllStars(stars);
        }
    }

    public void SetColor(Color color) {
        Sprite.color = color;
    }
}
