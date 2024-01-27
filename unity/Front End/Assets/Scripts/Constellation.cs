using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class MinMax
{
    public float min;
    public float max;
}

public class Constellation : MonoBehaviour
{
    public List<Star> rootStars = new();
    private float stopTime = 0;
    private readonly List<Star> allStars = new();
    private bool playing = false;
    private ConstellationController controller;

    private void Awake() {
        controller = transform.GetComponentInParent<ConstellationController>();
    }
    private void Start() {
        rootStars.ForEach(star => {
            star.Create(controller);
        });

    }

    public void Queue(float lifespan) {
        stopTime = Time.time + lifespan;
        Play();
    }

    private void Update() {
        if (Time.time > stopTime && playing) {
            if (rootStars.TrueForAll(star => star.IsCompleted()))
                Stop();
        }
    }

    public void Play() {
        playing = true;
        rootStars.ForEach(star => {
            star.Play(controller);
        });
    }

    public void Stop() {
        playing = false;
        rootStars.ForEach(star => {
            star.Stop(controller);
        });
    }

    public void AddConstellationStars(List<Star> stars, List<Tuple<Star, Vector3>> starPositions) {
        rootStars.ForEach(star => {
            allStars.Add(star);
            star.GetAllStars(allStars, starPositions);
        });

        stars.AddRange(allStars);
    }

    public void SetColor(Color color) {
        allStars[Random.Range(0, allStars.Count)].SetColor(color);
    }
}
