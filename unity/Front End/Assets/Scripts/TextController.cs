using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

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

    private enum State {
        Starting,
        Ending,
        BeRightBack
    }
    private bool typing = false;
    private string text = "";
    private State state = State.Starting;
    void Start()
    {
        startingMesh.SetActive(useMesh);
        endingMesh.SetActive(false);
        beRightBackMesh.SetActive(false);
        textMeshPro.gameObject.SetActive(!useMesh);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Home)) {
            textMeshPro.text = starting;
            state = State.Starting;
        } else if (Input.GetKeyDown(KeyCode.End)) {
            textMeshPro.text = ending;
            state = State.Ending;
        } else if (Input.GetKeyDown(KeyCode.Delete)) {
            textMeshPro.text = beRightBack;
            state = State.BeRightBack;
        } else if (Input.GetKeyDown(KeyCode.Backspace)) {
            text = "";
            typing = true;
        }

        startingMesh.SetActive(state == State.Starting && useMesh);
        endingMesh.SetActive(state == State.Ending && useMesh);
        beRightBackMesh.SetActive(state == State.BeRightBack && useMesh);

        if(typing) {
            if(Input.GetKeyDown(KeyCode.Return)) {
                text += Input.inputString;
                Regex rgx = new("[^a-zA-Z0-9 -]");
                text = rgx.Replace(text, "");

                textMeshPro.text= text;

                typing = false;
                text = "";
            }
            else if(Input.anyKeyDown) {
                text += Input.inputString;
            }
        }
    }
}
