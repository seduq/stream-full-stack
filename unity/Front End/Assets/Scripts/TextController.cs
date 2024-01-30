using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
        TextController.SimpleListenerExample(new string[] { "start", "end", "brb" });
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

    public static void SimpleListenerExample(string[] prefixes) {
        //if (!HttpListener.IsSupported) {
        //    Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
        //    return;
        //}
        //// URI prefixes are required,
        //// for example "http://contoso.com:8080/index/".
        //if (prefixes == null || prefixes.Length == 0)
        //    throw new ArgumentException("prefixes");

        //// Create a listener.
        //HttpListener listener = new HttpListener();
        //// Add the prefixes.
        //foreach (string s in prefixes) {
        //    listener.Prefixes.Add(s);
        //}
        //listener.Start();
        //Console.WriteLine("Listening...");
        





        //// Note: The GetContext method blocks while waiting for a request.
        //HttpListenerContext context = listener.GetContextAsync();
        //HttpListenerRequest request = context.Request;
        //// Obtain a response object.
        //HttpListenerResponse response = context.Response;
        //// Construct a response.
        //string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
        //byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        //// Get a response stream and write the response to it.
        //response.ContentLength64 = buffer.Length;
        //System.IO.Stream output = response.OutputStream;
        //output.Write(buffer, 0, buffer.Length);
        //// You must close the output stream.
        //output.Close();
        //listener.Stop();
    }
}
