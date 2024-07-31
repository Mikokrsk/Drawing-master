using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Barracuda;
using Unity.VisualScripting;
using UnityEngine;

public class GetInferenceFromModel : MonoBehaviour
{
    public Texture2D texture;

    public NNModel modelAsset;
    private Model _runtimeModel;
    private IWorker _engine;

    [Serializable]
    public struct Prediction
    {
        public int predictedValue;
        public float[] predicted;

        public void SetPrediction(Tensor t)
        {
            predicted = t.AsFloats();
            predictedValue = Array.IndexOf(predicted, predicted.Max());
            Debug.Log($"Predicted {predictedValue}");
        }
    }

    public Prediction prediction;

    void Start()
    {
        _runtimeModel = ModelLoader.Load(modelAsset);
        _engine = WorkerFactory.CreateWorker(_runtimeModel, WorkerFactory.Device.GPU);
        prediction = new Prediction();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var channelCount = 1;
            var inputX = new Tensor(texture, channelCount);
            Tensor outputY = _engine.Execute(inputX).PeekOutput();
            inputX.Dispose();
            prediction.SetPrediction(outputY);
        }
    }

    private void OnDestroy()
    {
        _engine?.Dispose();
    }
}