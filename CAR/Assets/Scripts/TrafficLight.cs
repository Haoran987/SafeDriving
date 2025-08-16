using UnityEngine;
using System.Collections.Generic;

public class TrafficLight : MonoBehaviour
{
    public enum LightState
    {
        Red,
        Yellow,
        Green
    }

    [Tooltip("0: Red bright, 1: Yellow bright, 2: Green bright, 3: Red dark, 4: Yellow dark, 5: Green dark")]
    public List<Material> lightMaterials;

    [Tooltip("0: Top Red, 1: Top Yellow, 2: Top Green, 3: Bottom Red, 4: Bottom Yellow, 5: Bottom Green")]
    public List<Renderer> lightRenderers;

    public LightState TopLightState { get; private set; }
    public LightState BottomLightState { get; private set; }

    public bool autoUpdate = true;
    public float redLifetime = 30.0f;
    public float yellowLifetime = 10.0f;
    public float greenLifetime = 20.0f;
    private float updateInterval = 10.0f; 
    public LightState initialTopLightState = LightState.Red;
    public LightState initialBottomLightState = LightState.Red;

    public void SetTopLightState(LightState state)
    {
        TopLightState = state;
        UpdateLightMaterials(0, state);
        switch (state)
        {
            case LightState.Red:
                updateInterval = redLifetime;
                break;
            case LightState.Yellow:
                updateInterval = yellowLifetime;
                break;
            case LightState.Green:
                updateInterval = greenLifetime;
                break;
        }
    }

    public void SetBottomLightState(LightState state)
    {
        BottomLightState = state;
        UpdateLightMaterials(3, state);
    }

    private void UpdateLightMaterials(int startIndex, LightState state)
    {
        // i==0 → Red, 1 → Yellow, 2 → Green
        for (int i = 0; i < 3; i++)
        {
            bool isActive = (i == (int)state);
            int materialIndex = isActive ? i : i + 3;
            lightRenderers[startIndex + i].material = lightMaterials[materialIndex];
        }
    }

    public void CycleLights()
    {
        if (TopLightState == LightState.Red)
        {
            SetTopLightState(LightState.Green);
            SetBottomLightState(LightState.Green);
            updateInterval = greenLifetime;
        }
        else if (TopLightState == LightState.Green)
        {
            SetTopLightState(LightState.Yellow);
            SetBottomLightState(LightState.Yellow);
            updateInterval = yellowLifetime; // shorter yellow light
        }
        else if (TopLightState == LightState.Yellow)
        {
            SetTopLightState(LightState.Red);
            SetBottomLightState(LightState.Red);
            updateInterval = redLifetime; // longer red light
        }
    }

    void Start()
    {
        SetTopLightState(initialTopLightState);
        SetBottomLightState(initialBottomLightState);
    }

    void Update()
    {
        if (!autoUpdate) return;

        updateInterval -= Time.deltaTime;
        if (updateInterval <= 0f)
        {
            CycleLights();
            // updateInterval = 10.0f;
        }
    }
}
