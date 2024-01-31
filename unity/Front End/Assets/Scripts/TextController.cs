using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using TMPro;
using UnityEditor;
using UnityEngine;
using OBSWebsocketDotNet;
using System.Threading.Tasks;

public class TextController : MonoBehaviour
{
    [Header("Text Mesh")]
    public string starting;
    public string ending;
    public string beRightBack;
    public TextMeshPro textMeshPro;

    [Header("Mesh")]
    public bool useMesh = false;
    public GameObject startingMesh;
    public GameObject endingMesh;
    public GameObject beRightBackMesh;

    public enum State
    {
        Starting,
        Ending,
        BeRightBack,
        None
    }

    private State state;
    private bool typing = false;
    private string text = "";

    void Start() {
        SetState(State.Starting);
        textMeshPro.gameObject.SetActive(!useMesh);
    }

    void Update() {

        if (Input.GetKeyDown(KeyCode.Home)) {
            SetState(State.Starting);
        } else if (Input.GetKeyDown(KeyCode.End)) {
            SetState(State.Ending);
        } else if (Input.GetKeyDown(KeyCode.Delete)) {
            SetState(State.BeRightBack);
        } else if (Input.GetKeyDown(KeyCode.Backspace)) {
            text = "";
            typing = true;
        }

        textMeshPro.text = text;
        startingMesh.SetActive(state == State.Starting && useMesh);
        endingMesh.SetActive(state == State.Ending && useMesh);
        beRightBackMesh.SetActive(state == State.BeRightBack && useMesh);


        if (typing) {
            if (Input.GetKeyDown(KeyCode.Return)) {
                text += Input.inputString;
                Regex rgx = new("[^a-zA-Z0-9 -]");
                text = rgx.Replace(text, "");

                textMeshPro.text = text;

                typing = false;
                text = "";
            } else if (Input.anyKeyDown) {
                text += Input.inputString;
            }
        }
    }

    public void SetState(State state) {
        this.state= state;
        switch (state) {
            case State.Starting:
                text = starting;
                break;
            case State.Ending:
                text = ending;
                break;
            case State.BeRightBack:
                text = beRightBack;
                break;
            case State.None:
                text = "";
                break;
            default:
                break;
        }
    }
}
