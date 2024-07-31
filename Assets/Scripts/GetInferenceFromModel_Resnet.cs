using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Barracuda;
using Unity.VisualScripting;
using UnityEngine;

public class GetInferenceFromModel_Resnet : MonoBehaviour
{
    public Texture2D texture;

    public NNModel modelAsset;
    private Model _runtimeModel;
    private IWorker _engine;

    public TextAsset synsetTextAsset;

    public Prediction prediction;

    private Dictionary<int, string> classNames;

    [Serializable]
    public struct Prediction
    {
        public int predictedValue;
        public float[] predicted;
        public string predictedClass;

        public void SetPrediction(Tensor t, Dictionary<int, string> classNames)
        {
            predicted = t.AsFloats();
            predictedValue = Array.IndexOf(predicted, predicted.Max());
            predictedClass = classNames.ContainsKey(predictedValue) ? classNames[predictedValue] : "Unknown";
            Debug.Log($"Predicted Class: {predictedClass}");
        }
    }

    void Start()
    {
        _runtimeModel = ModelLoader.Load(modelAsset);
        _engine = WorkerFactory.CreateWorker(_runtimeModel, WorkerFactory.Device.GPU);
        prediction = new Prediction();

        classNames = LoadClassNamesFromTextAsset(synsetTextAsset);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var channelCount = 3;
            var inputX = new Tensor(texture, channelCount);
            Tensor outputY = _engine.Execute(inputX).PeekOutput();
            inputX.Dispose();
            prediction.SetPrediction(outputY, classNames);
        }
    }

    private Dictionary<int, string> LoadClassNamesFromTextAsset(TextAsset textAsset)
    {
        var classNames = new Dictionary<int, string>();
        var lines = textAsset.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < lines.Length; i++)
        {
            var parts = lines[i].Split(new[] { ' ' }, 2);
            if (parts.Length == 2)
            {
                classNames[i] = parts[1];
            }
        }

        return classNames;
    }

    private void OnDestroy()
    {
        _engine?.Dispose();
    }


}